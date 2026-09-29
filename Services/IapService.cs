using PersonalBlog.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PersonalBlog.Services
{
    public class IapService
    {
        private readonly SerialPortService _serial;
        private readonly IapSettings _settings;
        private readonly ConcurrentQueue<byte> _receiveBuffer = new ConcurrentQueue<byte>();

        public event Action<string> Log;
        public event Action<IapProgress> ProgressChanged;
        public event Action<string> StatusChanged;

        public IapService(SerialPortService serial, IapSettings settings)
        {
            _serial = serial ?? throw new ArgumentNullException(nameof(serial));
            _settings = settings ?? new IapSettings();
            _serial.DataReceived += OnSerialDataReceived;
        }

        private void OnSerialDataReceived(byte[] data)
        {
            foreach (var b in data) _receiveBuffer.Enqueue(b);
            // log raw received hex
            Log?.Invoke("RX: " + ToHexString(data));
            // notify listeners if needed
        }

        private async Task<byte[]> ReadResponseAsync(int expectedLength, int timeoutMs, CancellationToken ct)
        {
            var ms = System.Diagnostics.Stopwatch.StartNew();
            var list = new System.Collections.Generic.List<byte>();
            while (!ct.IsCancellationRequested)
            {
                while (_receiveBuffer.TryDequeue(out var b))
                {
                    list.Add(b);
                    if (list.Count >= expectedLength)
                    {
                        return list.ToArray();
                    }
                }

                if (ms.ElapsedMilliseconds > timeoutMs) break;
                await Task.Delay(5, ct).ConfigureAwait(false);
            }

            return list.ToArray();
        }

        private ushort CalcChecksum(byte[] bytes)
        {
            ulong sum = 0;
            //foreach (var b in bytes) sum += b;
            for(int i = 2; i < bytes.Length; i++) sum += bytes[i];
            ushort s = (ushort)((~sum + 1) & 0xFFFF);
            return s;
        }

        private byte[] ChecksumToBytes(ushort checksum)
        {
            if (_settings.ChecksumLittleEndian)
            {
                return new byte[] { (byte)(checksum & 0xFF), (byte)((checksum >> 8) & 0xFF) };
            }
            else
            {
                return new byte[] { (byte)((checksum >> 8) & 0xFF), (byte)(checksum & 0xFF) };
            }
        }

        private byte[] BuildFrame(byte cmd, byte seq, byte[] payload)
        {
            using (var ms = new MemoryStream())
            {
                // header
                ms.WriteByte(0x55);
                ms.WriteByte(0xAA);
                ms.WriteByte(cmd);
                ms.WriteByte(seq);
                byte len = (byte)(payload?.Length ?? 0);
                ms.WriteByte(len);
                if (payload != null && payload.Length > 0)
                    ms.Write(payload, 0, payload.Length);

                var body = ms.ToArray();
                // checksum over all bytes
                ushort checksum = CalcChecksum(body);
                var cs = ChecksumToBytes(checksum);
                ms.Write(cs, 0, cs.Length);
                var frame = ms.ToArray();
                // log raw sent hex
                Log?.Invoke("TX: " + ToHexString(frame));
                return frame;
            }
        }

        private bool TryParseAckFrame(byte[] buffer, out IapAck ack)
        {
            ack = null;
            if (buffer == null || buffer.Length < 7) return false; // header(2)+cmd+ack+seq+cs(2)=7
            // find header
            int idx = -1;
            for (int i = 0; i <= buffer.Length - 7; i++)
            {
                if (buffer[i] == 0x55 && buffer[i + 1] == 0xAA)
                {
                    idx = i; break;
                }
            }
            if (idx < 0) return false;
            int baseIndex = idx;
            byte cmd = buffer[baseIndex + 2];
            byte ackByte = buffer[baseIndex + 3];
            byte seq = buffer[baseIndex + 4];
            // checksum
            if (buffer.Length < baseIndex + 7) return false;
            ushort csReceived;
            if (_settings.ChecksumLittleEndian)
            {
                csReceived = (ushort)(buffer[baseIndex + 5] | (buffer[baseIndex + 6] << 8));
            }
            else
            {
                csReceived = (ushort)((buffer[baseIndex + 5] << 8) | buffer[baseIndex + 6]);
            }
            // compute checksum over header..seq
            int lenForCalc = 5; // bytes from header to seq inclusive
            var arr = new byte[lenForCalc];
            for (int i = 0; i < lenForCalc; i++) arr[i] = buffer[baseIndex + i];
            ushort csCalc = CalcChecksum(arr);
            if (csCalc != csReceived) return false;
            ack = new IapAck { Cmd = cmd, Ack = ackByte, Seq = seq };
            return true;
        }

        public async Task<bool> StartIapAsync(string binFilePath, CancellationToken ct)
        {
            if (!File.Exists(binFilePath))
            {
                StatusChanged?.Invoke("文件不存在。");
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(binFilePath);
            int totalFrames = (fileBytes.Length + 127) / 128;
            StatusChanged?.Invoke($"开始 IAP，共 {totalFrames} 帧。");

            // send erase (CMD 0x01) using seq 0
            byte seq = 0;
            var eraseFrame = BuildFrame(0x01, seq, null);
            int tryCount = 0;
            bool eraseAcked = false;
            while (tryCount < _settings.MaxRetries && !ct.IsCancellationRequested)
            {
                Log?.Invoke($"发送 擦除 CMD, seq={seq}, 尝试 {tryCount + 1}");
                await _serial.WriteAsync(eraseFrame).ConfigureAwait(false);
                var resp = await ReadResponseAsync(7, _settings.TimeoutMs, ct).ConfigureAwait(false);
                // log raw received hex for the response
                if (resp != null && resp.Length > 0) Log?.Invoke("RX: " + ToHexString(resp));
                if (TryParseAckFrame(resp, out var ack) && ack.Cmd == 0x01 && ack.Seq == seq && ack.Ack == 0x01)
                {
                    eraseAcked = true; break;
                }
                tryCount++;
            }

            if (!eraseAcked)
            {
                StatusChanged?.Invoke("擦除未收到 ACK，终止升级。");
                return false;
            }

            // start sending data frames
            long sentBytes = 0;
            for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                if (ct.IsCancellationRequested) { StatusChanged?.Invoke("用户取消。"); return false; }
                int offset = frameIndex * 128;
                int remain = Math.Min(128, fileBytes.Length - offset);
                byte[] payload = new byte[remain];
                Array.Copy(fileBytes, offset, payload, 0, remain);
                seq = (byte)((frameIndex + 1) & 0xFF); // seq starts at 1 for data frames
                var frame = BuildFrame(0x02, seq, payload);

                bool frameAcked = false;
                int attempts = 0;
                while (attempts < _settings.MaxRetries && !ct.IsCancellationRequested)
                {
                    Log?.Invoke($"发送 数据帧 idx={frameIndex + 1}/{totalFrames} seq={seq} 尝试 {attempts + 1}");
                    await _serial.WriteAsync(frame).ConfigureAwait(false);
                    var resp = await ReadResponseAsync(7, _settings.TimeoutMs, ct).ConfigureAwait(false);
                    if (resp != null && resp.Length > 0) Log?.Invoke("RX: " + ToHexString(resp));
                    if (TryParseAckFrame(resp, out var ack) && ack.Cmd == 0x02 && ack.Seq == seq && ack.Ack == 0x01)
                    {
                        frameAcked = true; break;
                    }
                    attempts++;
                }

                if (!frameAcked)
                {
                    StatusChanged?.Invoke($"数据帧 {frameIndex + 1} 超时多次，终止升级。");
                    return false;
                }

                sentBytes += remain;
                ProgressChanged?.Invoke(new IapProgress { CurrentFrame = frameIndex + 1, TotalFrames = totalFrames, SentBytes = sentBytes, TotalBytes = fileBytes.Length });
            }

            // send jump CMD 0x03, seq 0
            var jumpFrame = BuildFrame(0x03, 0, null);
            await _serial.WriteAsync(jumpFrame).ConfigureAwait(false);
            var finalResp = await ReadResponseAsync(7, _settings.TimeoutMs, ct).ConfigureAwait(false);
            if (finalResp != null && finalResp.Length > 0) Log?.Invoke("RX: " + ToHexString(finalResp));
            if (TryParseAckFrame(finalResp, out var finalAck) && finalAck.Cmd == 0x03 && finalAck.Ack == 0x01)
            {
                StatusChanged?.Invoke("升级完成。");
                return true;
            }

            StatusChanged?.Invoke("跳转指令未收到 ACK，升级可能未完成。");
            return false;
        }

        private static string ToHexString(byte[] bs)
        {
            if (bs == null || bs.Length == 0) return string.Empty;
            char[] c = new char[bs.Length * 2];
            byte b;
            for (int bx = 0, cx = 0; bx < bs.Length; ++bx, ++cx)
            {
                b = ((byte)(bs[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(bs[bx] & 0x0F));
                ++cx;
                c[cx] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }
    }
}
