using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FieldScan
{
    public partial class ProbeManagementWindow : Window
    {
        public ObservableCollection<Probe> Probes { get; set; }

        public ProbeManagementWindow(ObservableCollection<Probe> probes)
        {
            InitializeComponent();
            this.Probes = probes;
            ProbeListView.ItemsSource = this.Probes;
        }

        private void ProbeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProbeListView.SelectedItem is Probe selectedProbe)
            {
                FactorDataGrid.ItemsSource = selectedProbe.AntennaFactors;
            }
            else
            {
                FactorDataGrid.ItemsSource = null;
            }
        }

        private void AddProbe_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("请输入新探头的名称:", "新探头");
            if (dlg.ShowDialog() == true)
            {
                Probes.Add(new Probe { Name = dlg.Answer });
            }
        }

        private void DeleteProbe_Click(object sender, RoutedEventArgs e)
        {
            if (ProbeListView.SelectedItem is Probe selectedProbe)
            {
                if (MessageBox.Show($"确定要删除探头 '{selectedProbe.Name}' 吗?", "确认删除", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Probes.Remove(selectedProbe);
                }
            }
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (!(ProbeListView.SelectedItem is Probe selectedProbe))
            {
                MessageBox.Show("请先选择一个要导入数据的探头。", "提示");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
                Title = "请选择探头校准文件"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(ofd.FileName);
                    var newFactors = new List<AntennaFactorPoint>();
                    foreach (var line in lines.Skip(1)) // 跳过表头
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 2)
                        {
                            newFactors.Add(new AntennaFactorPoint
                            {
                                FrequencyHz = double.Parse(parts[0], CultureInfo.InvariantCulture),
                                FactorDb = double.Parse(parts[1], CultureInfo.InvariantCulture)
                            });
                        }
                    }
                    selectedProbe.AntennaFactors = newFactors;
                    FactorDataGrid.ItemsSource = null;
                    FactorDataGrid.ItemsSource = selectedProbe.AntennaFactors;
                    MessageBox.Show($"成功为 '{selectedProbe.Name}' 导入 {newFactors.Count} 个校准点。", "导入成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入文件失败: " + ex.Message, "错误");
                }
            }
        }
    }

    // 一个简单的输入对话框帮助类
    public class InputDialog : Window
    {
        public string Answer { get; private set; }
        private TextBox txtAnswer;

        public InputDialog(string question, string defaultAnswer = "")
        {
            Width = 300; Height = 150; Title = question;
            var panel = new StackPanel { Margin = new Thickness(10) };
            txtAnswer = new TextBox { Text = defaultAnswer };
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            var okButton = new Button { Content = "确定", IsDefault = true, Width = 80, Margin = new Thickness(5) };
            okButton.Click += (s, e) => { Answer = txtAnswer.Text; DialogResult = true; };
            var cancelButton = new Button { Content = "取消", IsCancel = true, Width = 80, Margin = new Thickness(5) };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(txtAnswer);
            panel.Children.Add(buttonPanel);
            Content = panel;
        }
    }
}