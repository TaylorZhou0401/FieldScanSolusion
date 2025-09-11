using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading; // 需要添加这个 using

namespace FieldScan
{
    public partial class CalibrationWindow : Window
    {
        private ScanClass _scanClass;
        private MainWindow _mainWindow;

        // ... (框选和坐标记录的变量保持不变)
        private Point startPoint;
        private Rectangle selectionRect;
        private Point[] imagePoints = new Point[4];
        private Pt[] robotPoints = new Pt[4];
        private bool[] isPointRecorded = new bool[4];

        // --- 新增：从 RobotControlWindow 移植过来的控制逻辑 ---
        private float _speed = 30;
        private DispatcherTimer _moveTimer;
        private string _currentMoveDirection = "";

        public BitmapImage DutImage { get; private set; }
        // ---------------------------------------------------

        public CalibrationWindow(ScanClass scanClass, MainWindow mainWindow)
        {
            InitializeComponent();
            _scanClass = scanClass;
            _mainWindow = mainWindow;

            // --- 新增：初始化移动定时器 ---
            _moveTimer = new DispatcherTimer();
            _moveTimer.Interval = TimeSpan.FromMilliseconds(100);
            _moveTimer.Tick += MoveTimer_Tick;

            this.Closing += (s, e) => _moveTimer?.Stop();
            // -----------------------------
        }

        // ... (LoadImage_Click 和框选逻辑保持不变)
        #region 框选逻辑
        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                // 将加载的图片同时赋值给公共属性和界面控件
                DutImage = new BitmapImage(new Uri(ofd.FileName));
                BackgroundImage.Source = DutImage;
            }
        }
        private void SelectionCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(SelectionCanvas);
            SelectionCanvas.Children.Clear();
            selectionRect = new Rectangle { Stroke = Brushes.Red, StrokeThickness = 2 };
            Canvas.SetLeft(selectionRect, startPoint.X);
            Canvas.SetTop(selectionRect, startPoint.Y);
            SelectionCanvas.Children.Add(selectionRect);
        }
        private void SelectionCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && selectionRect != null)
            {
                Point currentPoint = e.GetPosition(SelectionCanvas);
                var x = Math.Min(currentPoint.X, startPoint.X);
                var y = Math.Min(currentPoint.Y, startPoint.Y);
                var w = Math.Abs(currentPoint.X - startPoint.X);
                var h = Math.Abs(currentPoint.Y - startPoint.Y);
                selectionRect.Width = w;
                selectionRect.Height = h;
                Canvas.SetLeft(selectionRect, x);
                Canvas.SetTop(selectionRect, y);
            }
        }
        private void SelectionCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectionRect != null)
            {
                imagePoints[0] = new Point(Canvas.GetLeft(selectionRect), Canvas.GetTop(selectionRect));
                imagePoints[1] = new Point(Canvas.GetLeft(selectionRect) + selectionRect.Width, Canvas.GetTop(selectionRect));
                imagePoints[2] = new Point(Canvas.GetLeft(selectionRect) + selectionRect.Width, Canvas.GetTop(selectionRect) + selectionRect.Height);
                imagePoints[3] = new Point(Canvas.GetLeft(selectionRect), Canvas.GetTop(selectionRect) + selectionRect.Height);
                BtnRecordP1.IsEnabled = true; BtnRecordP2.IsEnabled = true; BtnRecordP3.IsEnabled = true; BtnRecordP4.IsEnabled = true;
                HighlightNextPointToRecord();
            }
            selectionRect = null;
        }
        #endregion

        // ... (记录和状态更新的逻辑保持不变)
        #region 记录与状态
        private void RecordPoint_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int pointIndex = int.Parse(button.Tag.ToString()) - 1;
            robotPoints[pointIndex] = _scanClass.GetPos();
            isPointRecorded[pointIndex] = true;
            UpdateStatus(pointIndex);
            HighlightNextPointToRecord();
        }
        private void UpdateStatus(int pointIndex)
        {
            var pos = robotPoints[pointIndex];
            string statusText = $"已记录: ({pos.X:F1}, {pos.Y:F1})";
            switch (pointIndex)
            {
                case 0: StatusP1.Text = statusText; StatusP1.Foreground = Brushes.Green; break;
                case 1: StatusP2.Text = statusText; StatusP2.Foreground = Brushes.Green; break;
                case 2: StatusP3.Text = statusText; StatusP3.Foreground = Brushes.Green; break;
                case 3: StatusP4.Text = statusText; StatusP4.Foreground = Brushes.Green; break;
            }
            if (isPointRecorded[0] && isPointRecorded[1] && isPointRecorded[2] && isPointRecorded[3])
            {
                BtnApply.IsEnabled = true;
            }
        }
        private void HighlightNextPointToRecord()
        {
            BorderP1.Background = Brushes.White; BorderP2.Background = Brushes.White;
            BorderP3.Background = Brushes.White; BorderP4.Background = Brushes.White;
            if (!isPointRecorded[0]) BorderP1.Background = Brushes.LightYellow;
            else if (!isPointRecorded[1]) BorderP2.Background = Brushes.LightYellow;
            else if (!isPointRecorded[2]) BorderP3.Background = Brushes.LightYellow;
            else if (!isPointRecorded[3]) BorderP4.Background = Brushes.LightYellow;
        }
        #endregion

        // --- 新增：移植过来的移动控制逻辑 ---
        private void StepButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            _currentMoveDirection = button.Tag.ToString();
            _moveTimer.Start();
        }

        private void StepButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _moveTimer.Stop();
            _currentMoveDirection = "";
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            PerformStepMove();
        }

        private void PerformStepMove()
        {
            if (string.IsNullOrEmpty(_currentMoveDirection)) return;
            if (!float.TryParse(txtStep.Text, out float step))
            {
                step = 1.0f; // 如果输入无效，则使用默认步长
            }

            try
            {
                var currentPos = _scanClass.GetPos();
                float targetX = currentPos.X;
                float targetY = currentPos.Y;
                float targetZ = currentPos.Z;

                switch (_currentMoveDirection)
                {
                    case "X+": targetX += step; break;
                    case "X-": targetX -= step; break;
                    case "Y+": targetY += step; break;
                    case "Y-": targetY -= step; break;
                    case "Z+": targetZ += step; break;
                    case "Z-": targetZ -= step; break;
                }

                _scanClass.StartMove(targetX, targetY, targetZ, currentPos.R, _speed);
            }
            catch (Exception ex)
            {
                _moveTimer.Stop();
                MessageBox.Show("移动失败: " + ex.Message);
            }
        }
        // ---------------------------------------------------

        // ... (ApplyCalibration_Click 逻辑保持不变)
        private void ApplyCalibration_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.TstartX = Math.Min(robotPoints[0].X, robotPoints[3].X);
            _mainWindow.TstartY = Math.Min(robotPoints[0].Y, robotPoints[1].Y);
            _mainWindow.TstopX = Math.Max(robotPoints[1].X, robotPoints[2].X);
            _mainWindow.TstopY = Math.Max(robotPoints[2].Y, robotPoints[3].Y);
            MessageBox.Show("校准成功！扫描范围已自动更新到主界面。");
            this.DialogResult = true;
            this.Close();
        }
    }
}