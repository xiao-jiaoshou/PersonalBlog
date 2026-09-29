using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace PersonalBlog.Services
{
    public class SerialPortService : IDisposable
    {
        private SerialPort _port;

        public event Action<byte[]> DataReceived;
        public bool IsOpen => _port != null && _port.IsOpen;

        public void Configure(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (_port != null)
            {
                if (_port.IsOpen) _port.Close();
                _port.DataReceived -= Port_DataReceived;
                _port.Dispose();
            }

            _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            _port.DataReceived += Port_DataReceived;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = _port.BytesToRead;
                if (bytesToRead <= 0) return;
                var buffer = new byte[bytesToRead];
                int read = _port.Read(buffer, 0, bytesToRead);
                if (read > 0)
                {
                    DataReceived?.Invoke(buffer);
                }
            }
            catch
            {
                // 忽略临时读取错误，具体交由调用方处理
            }
        }

        public Task OpenAsync()
        {
            return Task.Run(() =>
            {
                if (_port == null) throw new InvalidOperationException("Serial port not configured.");
                if (!_port.IsOpen)
                    _port.Open();
            });
        }

        public Task CloseAsync()
        {
            return Task.Run(() =>
            {
                if (_port != null && _port.IsOpen)
                    _port.Close();
            });
        }

        public Task WriteAsync(byte[] data)
        {
            return Task.Run(() =>
            {
                if (_port == null || !_port.IsOpen) throw new InvalidOperationException("Serial port is not open.");
                _port.Write(data, 0, data.Length);
            });
        }

        public void Dispose()
        {
            if (_port != null)
            {
                try { _port.DataReceived -= Port_DataReceived; } catch { }
                try { if (_port.IsOpen) _port.Close(); } catch { }
                _port.Dispose();
                _port = null;
            }
        }
    }
}
