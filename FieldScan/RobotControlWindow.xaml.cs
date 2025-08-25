using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FieldScan
{
    public partial class RobotControlWindow : Window
    {
        private ScanClass _scanClass;
        private float _speed = 30;

        // --- 用于实现加速移动的新变量 ---
        private float _initialStep = 0.01f;   // 初始步进，用于微调
        private float _currentStep;             // 当前步进，会动态变化
        private float _maxStep = 0.5f;          // 最大步进（速度上限）
        private float _acceleration = 0.005f; // 每100ms增加的步进值（加速度）
        // --------------------------------

        private DispatcherTimer _moveTimer;
        private DispatcherTimer _updatePosTimer;
        private string _currentMoveDirection = "";

        public RobotControlWindow(ScanClass scanClass)
        {
            InitializeComponent();
            _scanClass = scanClass;

            _moveTimer = new DispatcherTimer();
            _moveTimer.Interval = TimeSpan.FromMilliseconds(100);
            _moveTimer.Tick += MoveTimer_Tick;

            _updatePosTimer = new DispatcherTimer();
            _updatePosTimer.Interval = TimeSpan.FromMilliseconds(200);
            _updatePosTimer.Tick += UpdatePosTimer_Tick;
            _updatePosTimer.Start();

            this.Closing += (s, e) =>
            {
                _moveTimer?.Stop();
                _updatePosTimer?.Stop();
            };

            UpdateCurrentPosition();
        }

        private void UpdatePosTimer_Tick(object sender, EventArgs e)
        {
            if (!_moveTimer.IsEnabled)
            {
                UpdateCurrentPosition();
            }
        }

        private void UpdateCurrentPosition()
        {
            if (_scanClass != null)
            {
                var pos = _scanClass.GetPos();
                txtX.Text = pos.X.ToString("F2");
                txtY.Text = pos.Y.ToString("F2");
                txtZ.Text = pos.Z.ToString("F2");
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                float targetX = float.Parse(txtX.Text);
                float targetY = float.Parse(txtY.Text);
                float targetZ = float.Parse(txtZ.Text);
                var currentPos = _scanClass.GetPos();
                if (_scanClass.CanGo(targetX, targetY, targetZ, currentPos.R))
                {
                    _scanClass.Go(targetX, targetY, targetZ, currentPos.R, _speed);
                }
                else
                {
                    MessageBox.Show("目标位置无法到达！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("输入无效或移动失败: " + ex.Message);
            }
        }

        private void StepButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            _currentMoveDirection = button.Tag.ToString();

            // 每次按下时，都将当前步进重置为初始的微调值
            _currentStep = _initialStep;

            PerformStepMove();
            _moveTimer.Start();
        }

        private void StepButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _moveTimer.Stop();
            _currentMoveDirection = "";
            UpdateCurrentPosition();
        }

        // 定时器触发，实现持续移动和加速
        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            PerformStepMove();

            // 增加步进值，实现加速效果
            _currentStep += _acceleration;

            // 限制最大速度
            if (_currentStep > _maxStep)
            {
                _currentStep = _maxStep;
            }
        }

        private void PerformStepMove()
        {
            if (string.IsNullOrEmpty(_currentMoveDirection)) return;

            try
            {
                var currentPos = _scanClass.GetPos();
                float targetX = currentPos.X;
                float targetY = currentPos.Y;
                float targetZ = currentPos.Z;

                // 使用_currentStep进行移动
                switch (_currentMoveDirection)
                {
                    case "X+": targetX += _currentStep; break;
                    case "X-": targetX -= _currentStep; break;
                    case "Y+": targetY += _currentStep; break;
                    case "Y-": targetY -= _currentStep; break;
                    case "Z+": targetZ += _currentStep; break;
                    case "Z-": targetZ -= _currentStep; break;
                }

                _scanClass.StartMove(targetX, targetY, targetZ, currentPos.R, _speed);

                txtX.Text = targetX.ToString("F2");
                txtY.Text = targetY.ToString("F2");
                txtZ.Text = targetZ.ToString("F2");
            }
            catch (Exception ex)
            {
                _moveTimer.Stop();
                MessageBox.Show("移动失败: " + ex.Message);
            }
        }

        // --- 用于解决文本框无法编辑的方法 ---
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _updatePosTimer.Stop();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _updatePosTimer.Start();
        }
        // ------------------------------------
    }
}