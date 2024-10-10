using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace PCAB_Debugger_ComLib
{
    /// <summary>
    /// cntLaLinput.xaml の相互作用ロジック
    /// </summary>
    public partial class cntLaLinput : UserControl
    {
        public cntLaLinput()
        {
            InitializeComponent();
        }

        private void TEXTBOX_GotFocus(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        private void DEC_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Only 0 to 9 or -
            e.Handled = !new Regex("[0-9]|-").IsMatch(e.Text);
            if (sender is TextBox tb)
            {
                string strBF = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
                if (strBF.Trim() != "-") { e.Handled = !int.TryParse(strBF, out _); }
            }
        }

        private void NUMBER_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Only 0 to 9
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void DEC_TextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //If Paste
            if (e.Command == ApplicationCommands.Paste)
            {
                string strTXT = Clipboard.GetText();
                for (int cnt = 0; cnt < strTXT.Length; cnt++)
                {
                    if (!new Regex("[0-9]|[ ]").IsMatch(strTXT[cnt].ToString()))
                    {
                        e.Handled = true;
                        break;
                    }
                }
            }
        }
        private void DEC_TextBox_PreviewLostKeyboardForcus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                uint uintVal = Convert.ToUInt32(((TextBox)sender).Text);
                if (0 <= uintVal && uintVal <= 65535) { return; }
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show("Enter in the range 0 to 65535");
                e.Handled = true;
            }
        }

        private void LATITUDE_DEGREE_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void LATITUDE_DEGREE_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void LATITUDE_DEGREE_TEXTBOX_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }

        private void LONGITUDE_DEGREE_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void LONGITUDE_DEGREE_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void LONGITUDE_DEGREE_TEXTBOX_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }

        private void MINUTES_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void MINUTES_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void MINUTES_TEXTBOX_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }

        private void SECOND_TEXTBOX_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SECOND_TEXTBOX_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void SECOND_TEXTBOX_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }
    }
}
