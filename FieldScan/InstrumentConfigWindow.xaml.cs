using System;
using System.Collections.Generic;
using System.Windows;

namespace FieldScan
{
    public partial class InstrumentConfigWindow : Window
    {
        public InstrumentSettings Settings { get; private set; }

        // 字典用于单位和乘数之间的换算
        private readonly Dictionary<string, double> unitMultipliers = new Dictionary<string, double>
        {
            { "Hz", 1 },
            { "KHz", 1e3 },
            { "MHz", 1e6 },
            { "GHz", 1e9 }
        };

        public InstrumentConfigWindow(InstrumentSettings currentSettings)
        {
            InitializeComponent();
            this.Settings = currentSettings;
            DisplaySettings();
        }

        // 将设置显示在界面上（带单位换算）
        private void DisplaySettings()
        {
            txtIpAddress.Text = Settings.IpAddress;
            txtPort.Text = Settings.Port.ToString();
            txtPoints.Text = Settings.Points.ToString();

            // 显示中心频率
            cmbCenterFreqUnit.SelectedItem = Settings.CenterFrequencyUnit;
            double centerMultiplier = unitMultipliers[Settings.CenterFrequencyUnit];
            txtCenterFreq.Text = (Settings.CenterFrequencyHz / centerMultiplier).ToString("G");

            // 显示频域宽度
            cmbSpanUnit.SelectedItem = Settings.SpanUnit;
            double spanMultiplier = unitMultipliers[Settings.SpanUnit];
            txtSpan.Text = (Settings.SpanHz / spanMultiplier).ToString("G");
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // --- 从界面读取并保存 ---
                Settings.IpAddress = txtIpAddress.Text;
                Settings.Port = int.Parse(txtPort.Text);
                Settings.Points = int.Parse(txtPoints.Text);

                // 读取中心频率（带单位换算）
                string centerUnit = cmbCenterFreqUnit.SelectedItem.ToString();
                double centerValue = double.Parse(txtCenterFreq.Text);
                Settings.CenterFrequencyUnit = centerUnit;
                Settings.CenterFrequencyHz = centerValue * unitMultipliers[centerUnit];

                // 读取频域宽度（带单位换算）
                string spanUnit = cmbSpanUnit.SelectedItem.ToString();
                double spanValue = double.Parse(txtSpan.Text);
                Settings.SpanUnit = spanUnit;
                Settings.SpanHz = spanValue * unitMultipliers[spanUnit];
                // --------------------------

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("输入格式错误: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}