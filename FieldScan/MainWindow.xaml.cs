using OxyPlot.Axes;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot.Series;
using OxyPlot.Legends;
using System.Threading;
using System.IO;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel; // 需要添加
using System.Xml.Serialization; // 需要添加

namespace FieldScan
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ... 其他属性
        private InstrumentSettings saSettings = new InstrumentSettings();
        public ObservableCollection<Probe> Probes { get; set; } = new ObservableCollection<Probe>();
        private Probe activeProbe;
        public Probe ActiveProbe
        {
            get { return activeProbe; }
            set { SetProperty(ref activeProbe, value); }
        }
        private string probeLibraryFilePath = "ProbeLibrary.xml";
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadProbeLibrary(); // <-- 添加这行
            this.InitPlot();
        }
        #region 属性

        private float _Cx;
        public float Cx
        {
            get { return _Cx; }
            set { SetProperty(ref _Cx, value); }
        }
        private float _Cy;
        public float Cy
        {
            get { return _Cy; }
            set { SetProperty(ref _Cy, value); }
        }
        private float _Cz;
        public float Cz
        {
            get { return _Cz; }
            set { SetProperty(ref _Cz, value); }
        }
        private float _Cc;
        public float Cc
        {
            get { return _Cc; }
            set { SetProperty(ref _Cc, value); }
        }
        private float _Tx;
        public float Tx
        {
            get { return _Tx; }
            set { SetProperty(ref _Tx, value); }
        }
        private float _Ty;
        public float Ty
        {
            get { return _Ty; }
            set { SetProperty(ref _Ty, value); }
        }
        private float _Tz = float.NaN;
        public float Tz
        {
            get { return _Tz; }
            set { SetProperty(ref _Tz, value); }
        }
        private float _Tc = float.NaN;
        public float Tc
        {
            get { return _Tc; }
            set { SetProperty(ref _Tc, value); }
        }
        private float _TstartX;
        public float TstartX
        {
            get { return _TstartX; }
            set { SetProperty(ref _TstartX, value); }
        }
        private float _TstartY;
        public float TstartY
        {
            get { return _TstartY; }
            set { SetProperty(ref _TstartY, value); }
        }
        private float _TstopX;
        public float TstopX
        {
            get { return _TstopX; }
            set { SetProperty(ref _TstopX, value); }
        }
        private float _TstopY;
        public float TstopY
        {
            get { return _TstopY; }
            set { SetProperty(ref _TstopY, value); }
        }
        private int _NumX;
        public int NumX
        {
            get { return _NumX; }
            set { SetProperty(ref _NumX, value); }
        }
        private int _NumY;
        public int NumY
        {
            get { return _NumY; }
            set { SetProperty(ref _NumY, value); }
        }
        
        private ImageSource dutImageSource;
        public ImageSource DutImageSource
        {
            get { return dutImageSource; }
            set { SetProperty(ref dutImageSource, value); }
        }

        #endregion
        #region Plot
        private double[,] recPowers;
        private float[] xArray;
        private float[] yArray;
        private PlotModel model;
        //private double[,] plotShowMatrix;

        private HeatMapSeries heatMap;
        private void InitPlot()
        {
            this.model = new PlotModel();
            OxyColor[] rainbowColors = new OxyColor[3];
            //model.Legends.Add(new Legend { LegendPosition = LegendPosition.RightTop, LegendPlacement = LegendPlacement.Outside });
            rainbowColors[0] = OxyColor.FromRgb(0, 0, 255); //darkRed
            rainbowColors[1] = OxyColor.FromRgb(0, 255, 0); //red
            rainbowColors[2] = OxyColor.FromRgb(255, 0, 0);
            //rainbowColors[3] = OxyColor.FromRgb(0, 255, 0);
            //rainbowColors[4] = OxyColor.FromRgb(0, 0, 255);
            //rainbowColors[5] = OxyColor.FromRgb(153, 102, 255);
            //rainbowColors[6] = OxyColor.FromRgb(153, 102, 255);
            var numberOfColors = 128;
            var myRainbow = OxyPalette.Interpolate(
                numberOfColors,
                rainbowColors);
            model.Axes.Add(new LinearColorAxis { Palette = myRainbow });
            plot1.Model = this.model;
            InitPlotData();
        }
        private void InitPlotData()
        {
            model.Series.Clear();
            recPowers = new double[60, 40];
            for (int i = 0; i < 60; i++)
            {
                for (int j = 0; j < 40; j++)
                {
                    recPowers[i, j] = i + j;
                }
            }
            heatMap = new HeatMapSeries
            {
                X0 = -100,
                X1 = 100,
                Y0 = -100,
                Y1 = 100,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Rectangles,
                Data = recPowers
            };
            model.Series.Add(heatMap);
            model.InvalidatePlot(true);

        }
        private void SetAxisAndDataSource()
        {
            heatMap.X0 = TstartX;
            heatMap.X1 = TstopX;
            heatMap.Y0 = TstartY;
            heatMap.Y1 = TstopY;
            heatMap.Data = recPowers;
        }
        private void UpdatePloatShow()
        {
            model.InvalidatePlot(true);
        }
        private void SetBitMapMode(bool isBitMap)
        {
            if (isBitMap)
            {
                heatMap.RenderMethod = HeatMapRenderMethod.Bitmap;
            }
            else
            {
                heatMap.RenderMethod = HeatMapRenderMethod.Rectangles;
            }
            UpdatePloatShow();
        }
        int delayMs = 1000;
        private void AutoPlotShow()
        {
            while (true)
            {
                UpdatePloatShow();
                if (isNScan) break;
                Thread.Sleep(delayMs > 1000 ? delayMs : 1000);
            }
        }
        #endregion
        #region 连接断开
        private ScanClass scanClass = new ScanClass();
        //private bool IsConnected = false;
        private bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
            set { SetProperty(ref isConnected, value); }
        }
        private float speed = 30;
        private void Button_Click_Connect(object sender, RoutedEventArgs e)
        {
            if (IsConnected) return;
            try
            {
                scanClass.Init();
                var pos = scanClass.GetPos();
                Cx = pos.X; Cy = pos.Y; Cz = pos.Z; Cc = pos.R;
                Tx = Cx; Ty = Cy; Tz = Cz; Tc = Cc;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }


            Task.Factory.StartNew(() =>
            {
                IsConnected = true;
                try
                {
                    //isConnect = true;
                    while (IsConnected)
                    {
                        var pos = scanClass.GetPos();
                        Cx = pos.X; Cy = pos.Y; Cz = pos.Z; Cc = pos.R;
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    IsConnected = false;
                }

            }
            , TaskCreationOptions.LongRunning
            );
        }
        private void Button_Click_DisConnect(object sender, RoutedEventArgs e)
        {
            if (!IsNScan) return;
            if (IsConnected)
            {
                scanClass.Close();
                IsConnected = false;
            }
        }
        #endregion
        #region 设置位置
        private void Button_Click_SetPos(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;
            if (!IsNScan) return;
            try
            {
                if (scanClass.CanGo(Tx, Ty, Tz, Tc))
                {
                    scanClass.Go(Tx, Ty, Tz, Tc, speed);
                }
                else
                {
                    MessageBox.Show("无法到达！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        #endregion

        #region 测试
        private Sa sa = new Sa();
        //private bool IsNScan = true;
        private bool isNScan = true;
        public bool IsNScan
        {
            get { return isNScan; }
            set { SetProperty(ref isNScan, value); }
        }
        private double minStep = 0.1;//mm
        private void Button_Click_Scan(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) { MessageBox.Show("未连接！"); return; }
            if (float.IsNaN(Tz)) Tz = (float)Math.Round(Cz, 1);
            if (float.IsNaN(Tc)) Tc = (float)Math.Round(Cc, 1);

            //Pos
            float stepX = (TstopX - TstartX) / (NumX - 1);
            if (Math.Abs(stepX) < minStep) { MessageBox.Show("X坐标点数太多"); return; }
            float stepY = (TstopY - TstartY) / (NumY - 1);
            if (Math.Abs(stepY) < minStep) { MessageBox.Show("Y坐标点数太多"); return; }

            recPowers = new double[NumX, NumY];
            //plotShowMatrix = new double[NumX, NumY];
            for (int i = 0; i < NumX; i++)
            {
                for (int j = 0; j < _NumY; j++)
                {
                    recPowers[i, j] = double.NaN;
                }

            }
            delayMs = NumX * NumY / 5;

            xArray = new float[NumX];
            yArray = new float[NumY];
            for (int x = 0; x < NumX - 1; x++)
            {
                xArray[x] = (float)Math.Round(TstartX + stepX * x, 1);
            }
            for (int y = 0; y < NumY - 1; y++)
            {
                yArray[y] = (float)Math.Round(TstartY + stepY * y, 1);
            }
            xArray[NumX - 1] = TstopX;
            yArray[NumY - 1] = TstopY;

            if (!scanClass.CanGo(TstartX, TstartY, Tz, Tc)
                || !scanClass.CanGo(TstopX, TstopY, Tz, Tc)
                || !scanClass.CanGo((float)((TstartX + TstopX) / 2.0), TstopY, Tz, Tc)
                )
            {
                MessageBox.Show("目标不能到达。");
                return;
            }
            IsNScan = false;
            SetAxisAndDataSource();
            Task.Factory.StartNew(AutoPlotShow, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ScanData, TaskCreationOptions.LongRunning);
        }
        private void ScanData()
        {
            try
            {
                SetBitMapMode(false);

                // 1. 使用新设置进行连接
                sa.Connect(saSettings.IpAddress, saSettings.Port);

                // 2. 发送参数设置指令
                sa.SetCenterFrequency(saSettings.CenterFrequencyHz);
                sa.SetSpan(saSettings.SpanHz);
                sa.SetSweepPoints(saSettings.Points);

                // 3. 设置参考电压
                sa.SetReferenceLevel(saSettings.ReferenceLevelDb);
                // --------------------------

                // 1. 自动获取扫描时间
                double baseSweepTimeMs = sa.GetSweepTimeMillis();
                // 2. 计算动态冗余时间 (例如，基础时间的50% + 100ms的固定通信延迟)
                int autoScanDelayMs = (int)Math.Ceiling(baseSweepTimeMs * 1.5 + 100);
                // --------------------------


                StringBuilder sb = new StringBuilder(4096);
                CreateNewDataFile();
                WriteDataFile($"Height:{Tz},R:{Tc}");
                WriteDataFile($"Begin({TstartX}:{TstartY}),Stop({TstopX}:{TstopY}),Points({NumX}:{NumY})");
                sb.Clear();
                for (int i = 0; i < NumX; i++)
                {
                    sb.Append(xArray[i].ToString("F3") + ",");
                }
                WriteDataFile(sb.ToString());
                List<string> frequencyHeaders = new List<string>();
                double startFreq = saSettings.CenterFrequencyHz - (saSettings.SpanHz / 2);
                double stepFreq = saSettings.SpanHz / (saSettings.Points - 1);
                for (int i = 0; i < saSettings.Points; i++)
                {
                    double currentFreq = startFreq + (i * stepFreq);
                    string freqString;
                    if (currentFreq >= 1e9) freqString = (currentFreq / 1e9).ToString("F3") + "GHz";
                    else if (currentFreq >= 1e6) freqString = (currentFreq / 1e6).ToString("F3") + "MHz";
                    else if (currentFreq >= 1e3) freqString = (currentFreq / 1e3).ToString("F3") + "KHz";
                    else freqString = currentFreq.ToString("F3") + "Hz";
                    frequencyHeaders.Add(freqString);
                }
                WriteAllDataFile($"X(mm),Y(mm),{string.Join(",", frequencyHeaders)}");

                // 创建一个新的校准数据文件
                string correctedDataPath = pathAllData.Replace("_All.csv", "_Corrected_All.csv");
                WriteCorrectedDataFile(correctedDataPath, $"X(mm),Y(mm) (Corrected dBuV/m),{string.Join(",", frequencyHeaders)}");
                //isNScan = false;
                //Scan
                for (int idxY = 0; idxY < NumY; idxY++)
                {
                    int idxX = 0;
                    bool isFwd = true;
                    if (idxY % 2 == 0)
                    {
                        isFwd = true;
                        idxX = 0;
                    }
                    else
                    {
                        isFwd = false;
                        idxX = NumX - 1;
                    }
                    for (int i = 0; i < NumX; i++)
                    {
                        if (isNScan || !isConnected) return;
                        scanClass.Go(xArray[idxX], yArray[idxY], Tz, Tc, speed);
                        if (isNScan || !isConnected) return;

                        // --- 这里是关键修改 ---
                        // 1. 读取功率值和原始踪迹数据，并分别存入变量
                        recPowers[idxX, idxY] = sa.ReNewMaxHoldAndRead(autoScanDelayMs); // autoScanDelayMs 来自自动扫描时间的功能
                        string rawTrace = sa.ReadTrace(); // 将踪迹数据存入 rawTrace 变量

                        if (isNScan || !isConnected) return;

                        // 2. 使用 rawTrace 变量写入原始数据文件
                        WriteAllDataFile($"{xArray[idxX].ToString("F3")},{yArray[idxY].ToString("F3")}," + rawTrace);

                        // 3. 使用 rawTrace 变量进行探头校准计算
                        var powerValues = rawTrace.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => double.Parse(p.Trim())).ToList();
                        var correctedValues = new List<string>();
                        for (int j = 0; j < Math.Min(powerValues.Count, saSettings.Points); j++)
                        {
                            double freq = startFreq + j * stepFreq;
                            double powerDbm = powerValues[j];
                            double powerDbuV = powerDbm + 107; // 转换为 dBuV (50欧姆系统)
                            double antennaFactor = ActiveProbe.GetFactorAtFrequency(freq);
                            double correctedFieldStrength = powerDbuV + antennaFactor; // 单位 dBuV/m
                            correctedValues.Add(correctedFieldStrength.ToString("F2"));
                        }
                        WriteCorrectedDataFile(correctedDataPath, $"{xArray[idxX].ToString("F3")},{yArray[idxY].ToString("F3")}," + string.Join(",", correctedValues));
                        // --- 修改结束 ---

                        if (isNScan || !isConnected) return;
                        if (isFwd) idxX++;
                        else idxX--;
                    }

                    //存  单x行
                    sb.Clear();
                    sb.Append(yArray[idxY].ToString("F3") + ",");
                    for (int i = 0; i < NumX; i++)
                    {
                        sb.Append(recPowers[i, idxY].ToString("E7") + ",");
                    }
                    WriteDataFile(sb.ToString());
                }

                SetBitMapMode(true);
                IsNScan = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                IsNScan = true;
            }
        }

        private void Button_Click_ScanStop(object sender, RoutedEventArgs e)
        {
            IsNScan = true;
        }
        #endregion

        #region 输出
        private void Button_Click_Export(object sender, RoutedEventArgs e)
        {
            if (!IsNScan) return;
            //输出
        }
        string path;
        string pathAllData;
        private void CreateNewDataFile()
        {
            // 1. 获取所有需要加入文件名的部分 (已移除 sideName 和 layerName)
            string angleString = $"{Tc:F1}deg";

            // 2. 将中心频率从Hz转换为带单位的易读格式
            string freqString;
            double centerFreq = saSettings.CenterFrequencyHz;
            if (centerFreq >= 1e9)
            {
                freqString = (centerFreq / 1e9).ToString("G3") + "GHz";
            }
            else if (centerFreq >= 1e6)
            {
                freqString = (centerFreq / 1e6).ToString("G3") + "MHz";
            }
            else if (centerFreq >= 1e3)
            {
                freqString = (centerFreq / 1e3).ToString("G3") + "KHz";
            }
            else
            {
                freqString = centerFreq.ToString("G3") + "Hz";
            }

            // 3. 构建新的文件名 (已移除 sideName 和 layerName)
            string baseFileName = $"{freqString}_{angleString}";

            // 4. 检查文件是否已存在... (后续逻辑不变)
            string dir = $"{Environment.CurrentDirectory}\\Data";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string finalFileName = baseFileName;
            int counter = 1;
            while (System.IO.File.Exists(System.IO.Path.Combine(dir, finalFileName + ".csv")))
            {
                finalFileName = $"{baseFileName}({counter})";
                counter++;
            }

            path = System.IO.Path.Combine(dir, finalFileName + ".csv");
            pathAllData = System.IO.Path.Combine(dir, finalFileName + "_All.csv");

            // 创建空文件
            StreamWriter sw = new StreamWriter(path, false);
            sw.Close();
            sw.Dispose();
            StreamWriter sw2 = new StreamWriter(pathAllData, false);
            sw2.Close();
            sw2.Dispose();
        }

        private void WriteDataFile(string s)
        {
            StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine(s);
            sw.Close();
            sw.Dispose();
        }
        private void WriteAllDataFile(string s)
        {
            StreamWriter sw = new StreamWriter(pathAllData, true);
            sw.WriteLine(s);
            sw.Close();
            sw.Dispose();
        }
        private void ReadDataFile()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = ".csv";
                ofd.Filter = "csv file|*.csv";
                ofd.InitialDirectory = Environment.CurrentDirectory + "\\Data";
                if (ofd.ShowDialog() == true)
                {
                    string path = ofd.FileName;
                    StreamReader sr = new StreamReader(path);
                    for (int i = 0; i < 2; i++)
                    {
                        sr.ReadLine();
                    }
                    List<string> list = new List<string>();
                    while (!sr.EndOfStream)
                    {
                        list.Add(sr.ReadLine());

                    }

                    for (int line = 0; line < list.Count; line++)
                    {
                        var datas = list[line].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (line == 0) recPowers = new double[datas.Length, list.Count];
                        for (int i = 0; i < datas.Length; i++)
                        {
                            recPowers[i, line] = double.Parse(datas[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion


        #region 界面相关

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        #endregion

        private void Button_Click_Show(object sender, RoutedEventArgs e)
        {

            MessageBox.Show(model.Width.ToString());
            MessageBox.Show(model.Height.ToString());
            return;
            ReadDataFile();
            UpdatePloatShow();
        }
        private void Button_Click_OpenControlWindow(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
            {
                MessageBox.Show("请先连接机械臂！");
                return;
            }

            // 创建并显示控制窗口
            RobotControlWindow controlWindow = new RobotControlWindow(this.scanClass);
            controlWindow.Show();
        }

        // 添加这三个方法，用于控制新窗口按钮
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InstrumentConnect_Click(object sender, RoutedEventArgs e)
        {
            // 传入当前设置，打开设置窗口
            InstrumentConfigWindow configWindow = new InstrumentConfigWindow(saSettings);

            // 使用 ShowDialog() 来阻塞主窗口，直到设置窗口关闭
            bool? result = configWindow.ShowDialog();

            // 如果用户点击了“确认”
            if (result == true)
            {
                // 更新主窗口的设置为用户输入的新值
                saSettings = configWindow.Settings;
                MessageBox.Show("仪器参数已更新！将在下次扫描时生效。", "提示");
            }
        }
        private void ProbeLibrary_Click(object sender, RoutedEventArgs e)
        {
            ProbeManagementWindow probeWindow = new ProbeManagementWindow(Probes);
            probeWindow.ShowDialog();
            SaveProbeLibrary(); // 关闭管理窗口后，自动保存更改
        }

        private void LoadProbeLibrary()
        {
            if (File.Exists(probeLibraryFilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Probe>));
                    using (var reader = new StreamReader(probeLibraryFilePath))
                    {
                        Probes = (ObservableCollection<Probe>)serializer.Deserialize(reader);
                    }
                }
                catch (Exception ex) { MessageBox.Show("加载探头库文件失败: " + ex.Message); }
            }
            if (Probes == null || Probes.Count == 0)
            {
                Probes = new ObservableCollection<Probe>();
                Probes.Add(new Probe { Name = "默认探头 (无校准)" });
            }
            ActiveProbe = Probes[0];
        }

        private void SaveProbeLibrary()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Probe>));
                using (var writer = new StreamWriter(probeLibraryFilePath))
                {
                    serializer.Serialize(writer, Probes);
                }
            }
            catch (Exception ex) { MessageBox.Show("保存探头库文件失败: " + ex.Message); }
        }
        private void WriteCorrectedDataFile(string filePath, string s)
        {
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.WriteLine(s);
            sw.Close();
            sw.Dispose();
        }
        private void CameraCalibrate_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
            {
                MessageBox.Show("请先连接机械臂！");
                return;
            }

            CalibrationWindow calibWindow = new CalibrationWindow(this.scanClass, this);

            // 使用 ShowDialog() 来等待校准窗口关闭
            // 并检查它的返回结果是否为 "成功" (true)
            if (calibWindow.ShowDialog() == true)
            {
                // 如果校准成功，并且用户加载了图片
                if (calibWindow.DutImage != null)
                {
                    // 就把这张图片设置为热力图的背景
                    this.DutImageSource = calibWindow.DutImage;
                }
            }
        }

    } // 这是 MainWindow 类的结束括号
}