using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace FieldScan
{
    public partial class StyleDict
    {
        private void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb.IsSelectionActive)
            {
                if (tb.Text.Contains("-") && tb.SelectionStart == 0)
                {
                    return;
                }
                int selectionIndexBuf = tb.SelectionStart;
                var index = tb.Text.IndexOf('.');
                if (index == -1)
                {
                    index = tb.Text.Length;
                }
                int dev = selectionIndexBuf - index;
                //SelectionStart
                double result = 0;
                if (double.TryParse(tb.Text, out result))
                {

                    double buf = 1;
                    if (tb.SelectionStart > index)
                    {
                        for (int i = 0; i < tb.SelectionStart - index - 1; i++)
                        {
                            buf *= 0.1;
                        }

                    }
                    else
                    {
                        if (tb.SelectionStart < index)
                        {
                            for (int i = 0; i < index - tb.SelectionStart; i++)
                            {
                                buf *= 10;
                            }
                        }
                    }
                    result += buf * (e.Delta / 120);
                }
                else
                {
                    result += e.Delta / 120;
                }
                tb.Text = result.ToString("F7");
                tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                //光标重新定位
                index = tb.Text.IndexOf('.');
                if (index == -1)
                {
                    index = tb.Text.Length;
                }
                tb.Select(index + dev, 0);
            }
        }
        private void TextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tbx = sender as TextBox;
                if (tbx.Text == null || tbx.Text == "" || tbx.Text == "-")
                {
                    tbx.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                }
                else
                {
                    tbx.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                }

            }
        }

        private void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            var tbx = sender as TextBox;

            if (tbx.IsSelectionActive)
            {
                Regex re = new Regex("^-?\\d*\\.?\\d*$");
                string newStr = tbx.Text.Insert(tbx.SelectionStart, e.Text);
                e.Handled = !re.IsMatch(newStr);
            }

            //Regex re = new Regex("[^0-9.-]+");
            //e.Handled = re.IsMatch(e.Text);
        }

        public void Enter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private void tb_PreviewTextInputNormal(object sender, TextCompositionEventArgs e)
        {

            var tbx = sender as TextBox;

            if (tbx.IsSelectionActive)
            {
                Regex re = new Regex("^\\d*$");
                string newStr = tbx.Text.Insert(tbx.SelectionStart, e.Text);
                e.Handled = !re.IsMatch(newStr);
            }

            //Regex re = new Regex("[^0-9.-]+");
            //e.Handled = re.IsMatch(e.Text);
        }
    }
}
