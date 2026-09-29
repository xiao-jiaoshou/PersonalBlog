using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PersonalBlog.Models;
using PersonalBlog.Services;

namespace PersonalBlog.ViewModels
{
    public class CrcCalculatorViewModel : INotifyPropertyChanged
    {
        private string _inputText = "123456789";
        private string _resultHex = string.Empty;
        private string _resultDec = string.Empty;
        private string _resultBin = string.Empty;
        private string _statusMessage = "输入数据后点击计算。";
        private CrcParameterModel _selectedPreset;
        private bool _isHexInput;

        public ObservableCollection<CrcParameterModel> Presets { get; } = new ObservableCollection<CrcParameterModel>();

        public ICommand CalculateCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CopyResultCommand { get; }

        public CrcCalculatorViewModel()
        {
            foreach (var preset in CrcCalculatorService.GetCommonModels())
            {
                Presets.Add(preset);
            }

            SelectedPreset = Presets[3];

            CalculateCommand = new RelayCommand(_ => Calculate());
            ClearCommand = new RelayCommand(_ => Clear());
            CopyResultCommand = new RelayCommand(_ => CopyResult());
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText == value) return;
                _inputText = value;
                OnPropertyChanged();
            }
        }

        public bool IsHexInput
        {
            get => _isHexInput;
            set
            {
                if (_isHexInput == value) return;
                _isHexInput = value;
                OnPropertyChanged();
            }
        }

        public CrcParameterModel SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (_selectedPreset == value) return;
                _selectedPreset = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Polynomial));
                OnPropertyChanged(nameof(InitialValue));
                OnPropertyChanged(nameof(ReflectInput));
                OnPropertyChanged(nameof(ReflectOutput));
                OnPropertyChanged(nameof(XorOut));
            }
        }

        public int Width
        {
            get => SelectedPreset?.Width ?? 0;
            set
            {
                if (SelectedPreset == null || SelectedPreset.Width == value) return;
                SelectedPreset.Width = value;
                OnPropertyChanged();
            }
        }

        public string Polynomial
        {
            get => SelectedPreset?.HexPolynomial ?? string.Empty;
            set
            {
                if (SelectedPreset == null) return;
                SelectedPreset.Polynomial = ParseUnsigned(value, SelectedPreset.Polynomial);
                OnPropertyChanged();
            }
        }

        public string InitialValue
        {
            get => SelectedPreset?.HexInitialValue ?? string.Empty;
            set
            {
                if (SelectedPreset == null) return;
                SelectedPreset.InitialValue = ParseUnsigned(value, SelectedPreset.InitialValue);
                OnPropertyChanged();
            }
        }

        public bool ReflectInput
        {
            get => SelectedPreset?.ReflectInput ?? false;
            set
            {
                if (SelectedPreset == null || SelectedPreset.ReflectInput == value) return;
                SelectedPreset.ReflectInput = value;
                OnPropertyChanged();
            }
        }

        public bool ReflectOutput
        {
            get => SelectedPreset?.ReflectOutput ?? false;
            set
            {
                if (SelectedPreset == null || SelectedPreset.ReflectOutput == value) return;
                SelectedPreset.ReflectOutput = value;
                OnPropertyChanged();
            }
        }

        public string XorOut
        {
            get => SelectedPreset?.HexXorOut ?? string.Empty;
            set
            {
                if (SelectedPreset == null) return;
                SelectedPreset.XorOut = ParseUnsigned(value, SelectedPreset.XorOut);
                OnPropertyChanged();
            }
        }

        public string ResultHex
        {
            get => _resultHex;
            set
            {
                if (_resultHex == value) return;
                _resultHex = value;
                OnPropertyChanged();
            }
        }

        public string ResultDec
        {
            get => _resultDec;
            set
            {
                if (_resultDec == value) return;
                _resultDec = value;
                OnPropertyChanged();
            }
        }

        public string ResultBin
        {
            get => _resultBin;
            set
            {
                if (_resultBin == value) return;
                _resultBin = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage == value) return;
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private void Calculate()
        {
            try
            {
                if (SelectedPreset == null)
                {
                    StatusMessage = "请选择 CRC 参数模型。";
                    return;
                }

                byte[] data = CrcCalculatorService.ParseInput(InputText, IsHexInput);
                CrcResult result = CrcCalculatorService.Calculate(SelectedPreset, data);
                ResultHex = result.HexValue;
                ResultDec = result.DecValue;
                ResultBin = result.BinaryValue;
                StatusMessage = $"计算完成：{result.AlgorithmName}";
            }
            catch (Exception ex)
            {
                ResultHex = string.Empty;
                ResultDec = string.Empty;
                ResultBin = string.Empty;
                StatusMessage = ex.Message;
            }
        }

        private void Clear()
        {
            InputText = string.Empty;
            ResultHex = string.Empty;
            ResultDec = string.Empty;
            ResultBin = string.Empty;
            StatusMessage = "已清空输入与结果。";
        }

        private void CopyResult()
        {
            if (string.IsNullOrWhiteSpace(ResultHex))
            {
                StatusMessage = "没有可复制的 CRC 结果。";
                return;
            }

            Clipboard.SetText(ResultHex);
            StatusMessage = "已复制十六进制结果。";
        }

        private static ulong ParseUnsigned(string value, ulong fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            string text = value.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(2);

            if (ulong.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out ulong hexValue))
                return hexValue;

            if (ulong.TryParse(text, out ulong decValue))
                return decValue;

            return fallback;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
