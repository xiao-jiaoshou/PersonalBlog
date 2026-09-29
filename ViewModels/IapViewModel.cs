using PersonalBlog.Services;
using PersonalBlog.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32; // for OpenFileDialog
using System.IO.Ports;
using System.IO;
using System.Linq;

namespace PersonalBlog.ViewModels
{
    public class IapViewModel : INotifyPropertyChanged
    {
        private readonly SerialPortService _serial = new SerialPortService();
        private IapService _iapService;
        private readonly IapSettings _settings = new IapSettings();

        private string _selectedFile;
        private bool _isConnected;
        private bool _isIapRunning;
        private string _status;

        // serial config
        private string _portName;
        private int _baudRate;
        private int _dataBits;
        private Parity _parity;
        private StopBits _stopBits;

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> AvailablePorts { get; } = new ObservableCollection<string>(SerialPort.GetPortNames());
        public ObservableCollection<int> CommonBaudRates { get; } = new ObservableCollection<int>(new[] { 9600, 19200, 38400, 57600, 115200, 230400 });
        public ObservableCollection<Parity> ParityOptions { get; } = new ObservableCollection<Parity>(Enum.GetValues(typeof(Parity)).Cast<Parity>());
        public ObservableCollection<StopBits> StopBitsOptions { get; } = new ObservableCollection<StopBits>(Enum.GetValues(typeof(StopBits)).Cast<StopBits>());

        public ICommand SelectFileCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand StartIapCommand { get; }
        public ICommand StopIapCommand { get; }
        public ICommand RefreshPortsCommand { get; }
        public ICommand SaveConfigCommand { get; }
        public ICommand LoadConfigCommand { get; }

        public IapViewModel()
        {
            _iapService = new IapService(_serial, _settings);
            _iapService.Log += s => AddLog(s);
            _iapService.ProgressChanged += p => {
                Status = $"{p.CurrentFrame}/{p.TotalFrames} " + Status;
            };
            _iapService.StatusChanged += s => Status = s;

            SelectFileCommand = new RelayCommand(_ => SelectFile());
            ConnectCommand = new RelayCommand(async _ => await ConnectAsync());
            DisconnectCommand = new RelayCommand(async _ => await DisconnectAsync());
            StartIapCommand = new RelayCommand(async _ => await StartIapAsync());
            StopIapCommand = new RelayCommand(_ => StopIap());
            RefreshPortsCommand = new RelayCommand(_ => RefreshPorts());
            SaveConfigCommand = new RelayCommand(_ => SaveConfig());
            LoadConfigCommand = new RelayCommand(_ => LoadConfig());

            // defaults
            PortName = AvailablePorts.FirstOrDefault() ?? "COM1";
            BaudRate = 115200;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;

            // try load saved config
            LoadConfig();
        }

        public string SelectedFile
        {
            get => _selectedFile;
            set { _selectedFile = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        public bool IsIapRunning
        {
            get => _isIapRunning;
            set { _isIapRunning = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string PortName { get => _portName; set { _portName = value; OnPropertyChanged(); } }
        public int BaudRate { get => _baudRate; set { _baudRate = value; OnPropertyChanged(); } }
        public int DataBits { get => _dataBits; set { _dataBits = value; OnPropertyChanged(); } }
        public Parity Parity { get => _parity; set { _parity = value; OnPropertyChanged(); } }
        public StopBits StopBits { get => _stopBits; set { _stopBits = value; OnPropertyChanged(); } }

        private void AddLog(string s)
        {
            App.Current.Dispatcher.Invoke(() => Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {s}"));
        }

        private void SelectFile()
        {
            var dlg = new OpenFileDialog() { Filter = "BIN Files (*.bin)|*.bin|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                SelectedFile = dlg.FileName;
                AddLog("选择文件: " + SelectedFile);
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                _serial.Configure(PortName, BaudRate, Parity, DataBits, StopBits);
                await _serial.OpenAsync();
                IsConnected = true;
                AddLog($"串口 {PortName} 已连接，{BaudRate},{DataBits},{Parity},{StopBits}。");
                SaveConfig();
            }
            catch (Exception ex)
            {
                AddLog("连接失败: " + ex.Message);
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                await _serial.CloseAsync();
                IsConnected = false;
                AddLog("串口已断开。");
            }
            catch (Exception ex)
            {
                AddLog("断开失败: " + ex.Message);
            }
        }

        private CancellationTokenSource _cts;

        private async Task StartIapAsync()
        {
            if (!IsConnected)
            {
                AddLog("请先连接串口。");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedFile))
            {
                AddLog("请先选择 .bin 文件。");
                return;
            }

            _cts = new CancellationTokenSource();
            IsIapRunning = true;
            AddLog("开始 IAP 升级...");
            bool ok = await _iapService.StartIapAsync(SelectedFile, _cts.Token);
            AddLog(ok ? "IAP 升级成功。" : "IAP 升级失败或被取消。");
            IsIapRunning = false;
        }

        private void StopIap()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                AddLog("用户已请求停止 IAP。");
            }
        }

        private void RefreshPorts()
        {
            AvailablePorts.Clear();
            foreach (var p in SerialPort.GetPortNames()) AvailablePorts.Add(p);
            PortName = AvailablePorts.FirstOrDefault() ?? PortName;
            AddLog("串口列表已刷新。");
        }

        private void SaveConfig()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersonalBlog");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var fn = Path.Combine(dir, "iap_serial_config.txt");
                File.WriteAllLines(fn, new[] {
                    $"PortName={PortName}",
                    $"BaudRate={BaudRate}",
                    $"DataBits={DataBits}",
                    $"Parity={(int)Parity}",
                    $"StopBits={(int)StopBits}"
                });
                AddLog("串口配置已保存。");
            }
            catch (Exception ex)
            {
                AddLog("保存配置失败: " + ex.Message);
            }
        }

        private void LoadConfig()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersonalBlog");
                var fn = Path.Combine(dir, "iap_serial_config.txt");
                if (!File.Exists(fn)) return;
                var lines = File.ReadAllLines(fn);
                foreach (var line in lines)
                {
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;
                    var key = line.Substring(0, idx);
                    var val = line.Substring(idx + 1);
                    switch (key)
                    {
                        case "PortName": PortName = val; break;
                        case "BaudRate": if (int.TryParse(val, out var br)) BaudRate = br; break;
                        case "DataBits": if (int.TryParse(val, out var db)) DataBits = db; break;
                        case "Parity": if (int.TryParse(val, out var p) && Enum.IsDefined(typeof(Parity), p)) Parity = (Parity)p; break;
                        case "StopBits": if (int.TryParse(val, out var s) && Enum.IsDefined(typeof(StopBits), s)) StopBits = (StopBits)s; break;
                    }
                }
                AddLog("串口配置已加载。");
            }
            catch (Exception ex)
            {
                AddLog("加载配置失败: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
