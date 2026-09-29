using System;
using System.Collections.Generic;
using System.Text;
using PersonalBlog.Models;

namespace PersonalBlog.Services
{
    public static class CrcCalculatorService
    {
        public static CrcResult Calculate(CrcParameterModel model, byte[] data)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            ulong crc = model.InitialValue & CreateMask(model.Width);
            ulong mask = CreateMask(model.Width);
            ulong topBit = 1UL << (model.Width - 1);

            foreach (byte currentByte in data)
            {
                byte b = model.ReflectInput ? Reverse(currentByte) : currentByte;
                crc ^= (ulong)b << (model.Width - 8);
                crc &= mask;

                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & topBit) != 0)
                    {
                        crc = ((crc << 1) ^ model.Polynomial) & mask;
                    }
                    else
                    {
                        crc = (crc << 1) & mask;
                    }
                }
            }

            if (model.ReflectOutput)
            {
                crc = Reverse(crc, model.Width);
            }

            crc = (crc ^ model.XorOut) & mask;

            return new CrcResult
            {
                AlgorithmName = model.Name,
                HexValue = crc.ToString("X"),
                DecValue = crc.ToString(),
                BinaryValue = ConvertToBinary(crc, model.Width)
            };
        }

        public static List<CrcParameterModel> GetCommonModels()
        {
            return new List<CrcParameterModel>
            {
                new CrcParameterModel { Name = "CRC-8", Width = 8, Polynomial = 0x07UL, InitialValue = 0x00UL, ReflectInput = false, ReflectOutput = false, XorOut = 0x00UL },
                new CrcParameterModel { Name = "CRC-16/MODBUS", Width = 16, Polynomial = 0x8005UL, InitialValue = 0xFFFFUL, ReflectInput = true, ReflectOutput = true, XorOut = 0x0000UL },
                new CrcParameterModel { Name = "CRC-16/CCITT-FALSE", Width = 16, Polynomial = 0x1021UL, InitialValue = 0xFFFFUL, ReflectInput = false, ReflectOutput = false, XorOut = 0x0000UL },
                new CrcParameterModel { Name = "CRC-32", Width = 32, Polynomial = 0x04C11DB7UL, InitialValue = 0xFFFFFFFFUL, ReflectInput = true, ReflectOutput = true, XorOut = 0xFFFFFFFFUL },
                new CrcParameterModel { Name = "CRC-32/BZIP2", Width = 32, Polynomial = 0x04C11DB7UL, InitialValue = 0xFFFFFFFFUL, ReflectInput = false, ReflectOutput = false, XorOut = 0xFFFFFFFFUL }
            };
        }

        public static byte[] ParseInput(string input, bool isHex)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new byte[0];

            if (!isHex)
                return Encoding.UTF8.GetBytes(input);

            string normalized = input
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace(":", string.Empty)
                .Replace(",", string.Empty)
                .Replace("0x", string.Empty)
                .Replace("0X", string.Empty);

            if (normalized.Length % 2 != 0)
                throw new FormatException("十六进制输入必须包含偶数个字符。");

            var bytes = new byte[normalized.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(normalized.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private static string ConvertToBinary(ulong value, int width)
        {
            var chars = new char[width];
            for (int i = width - 1; i >= 0; i--)
            {
                chars[i] = (value & 1UL) == 0 ? '0' : '1';
                value >>= 1;
            }
            return new string(chars);
        }

        private static byte Reverse(byte value)
        {
            byte result = 0;
            for (int i = 0; i < 8; i++)
            {
                result = (byte)((result << 1) | (value & 1));
                value >>= 1;
            }
            return result;
        }

        private static ulong Reverse(ulong value, int width)
        {
            ulong result = 0;
            for (int i = 0; i < width; i++)
            {
                result = (result << 1) | (value & 1UL);
                value >>= 1;
            }
            return result;
        }

        private static ulong CreateMask(int width)
        {
            if (width >= 64)
                return ulong.MaxValue;

            return (1UL << width) - 1UL;
        }
    }
}
