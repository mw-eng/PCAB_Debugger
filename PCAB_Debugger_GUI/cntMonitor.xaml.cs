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

namespace PCAB_Debugger_GUI
{
    /// <summary>
    /// cntMonitor.xaml の相互作用ロジック
    /// </summary>
    public partial class cntMonitor : UserControl
    {
        public string TEMPcpu
        {
            get { return SNS_CPU_TEMP_LABEL.Content.ToString(); }
            set { SNS_CPU_TEMP_LABEL.Content = value; }
        }
        public string SNSpin
        {
            get { return SNS_PIN_LABEL.Content.ToString(); }
            set { SNS_PIN_LABEL.Content = value; }
        }
        public string SNSvin
        {
            get { return SNS_VIN_LABEL.Content.ToString(); }
            set { SNS_VIN_LABEL.Content = value; }
        }
        public string SNSid
        {
            get { return SNS_ID_LABEL.Content.ToString(); }
            set { SNS_ID_LABEL.Content = value; }
        }
        public string SNSvd
        {
            get { return SNS_VD_LABEL.Content.ToString(); }
            set { SNS_VD_LABEL.Content = value; }
        }
        public string TEMP01ID
        {
            get { return TEMP01CODE.Content.ToString(); }
            set { TEMP01CODE.Content = value; }
        }
        public string TEMP02ID
        {
            get { return TEMP02CODE.Content.ToString(); }
            set { TEMP02CODE.Content = value; }
        }
        public string TEMP03ID
        {
            get { return TEMP03CODE.Content.ToString(); }
            set { TEMP03CODE.Content = value; }
        }
        public string TEMP04ID
        {
            get { return TEMP04CODE.Content.ToString(); }
            set { TEMP04CODE.Content = value; }
        }
        public string TEMP05ID
        {
            get { return TEMP05CODE.Content.ToString(); }
            set { TEMP05CODE.Content = value; }
        }
        public string TEMP06ID
        {
            get { return TEMP06CODE.Content.ToString(); }
            set { TEMP06CODE.Content = value; }
        }
        public string TEMP07ID
        {
            get { return TEMP07CODE.Content.ToString(); }
            set { TEMP07CODE.Content = value; }
        }
        public string TEMP08ID
        {
            get { return TEMP08CODE.Content.ToString(); }
            set { TEMP08CODE.Content = value; }
        }
        public string TEMP09ID
        {
            get { return TEMP09CODE.Content.ToString(); }
            set { TEMP09CODE.Content = value; }
        }
        public string TEMP10ID
        {
            get { return TEMP10CODE.Content.ToString(); }
            set { TEMP10CODE.Content = value; }
        }
        public string TEMP11ID
        {
            get { return TEMP11CODE.Content.ToString(); }
            set { TEMP11CODE.Content = value; }
        }
        public string TEMP12ID
        {
            get { return TEMP12CODE.Content.ToString(); }
            set { TEMP12CODE.Content = value; }
        }
        public string TEMP13ID
        {
            get { return TEMP13CODE.Content.ToString(); }
            set { TEMP13CODE.Content = value; }
        }
        public string TEMP14ID
        {
            get { return TEMP14CODE.Content.ToString(); }
            set { TEMP14CODE.Content = value; }
        }
        public string TEMP15ID
        {
            get { return TEMP15CODE.Content.ToString(); }
            set { TEMP15CODE.Content = value; }
        }
        public string TEMP01VALUE
        {
            get { return TEMP01VAL.Content.ToString(); }
            set { TEMP01VAL.Content = value; }
        }
        public string TEMP02VALUE
        {
            get { return TEMP02VAL.Content.ToString(); }
            set { TEMP02VAL.Content = value; }
        }
        public string TEMP03VALUE
        {
            get { return TEMP03VAL.Content.ToString(); }
            set { TEMP03VAL.Content = value; }
        }
        public string TEMP04VALUE
        {
            get { return TEMP04VAL.Content.ToString(); }
            set { TEMP04VAL.Content = value; }
        }
        public string TEMP05VALUE
        {
            get { return TEMP05VAL.Content.ToString(); }
            set { TEMP05VAL.Content = value; }
        }
        public string TEMP06VALUE
        {
            get { return TEMP06VAL.Content.ToString(); }
            set { TEMP06VAL.Content = value; }
        }
        public string TEMP07VALUE
        {
            get { return TEMP07VAL.Content.ToString(); }
            set { TEMP07VAL.Content = value; }
        }
        public string TEMP08VALUE
        {
            get { return TEMP08VAL.Content.ToString(); }
            set { TEMP08VAL.Content = value; }
        }
        public string TEMP09VALUE
        {
            get { return TEMP09VAL.Content.ToString(); }
            set { TEMP09VAL.Content = value; }
        }
        public string TEMP10VALUE
        {
            get { return TEMP10VAL.Content.ToString(); }
            set { TEMP10VAL.Content = value; }
        }
        public string TEMP11VALUE
        {
            get { return TEMP11VAL.Content.ToString(); }
            set { TEMP11VAL.Content = value; }
        }
        public string TEMP12VALUE
        {
            get { return TEMP12VAL.Content.ToString(); }
            set { TEMP12VAL.Content = value; }
        }
        public string TEMP13VALUE
        {
            get { return TEMP13VAL.Content.ToString(); }
            set { TEMP13VAL.Content = value; }
        }
        public string TEMP14VALUE
        {
            get { return TEMP14VAL.Content.ToString(); }
            set { TEMP14VAL.Content = value; }
        }
        public string TEMP15VALUE
        {
            get { return TEMP15VAL.Content.ToString(); }
            set { TEMP15VAL.Content = value; }
        }
        public bool TEMPviewIDs
        {
            get { if (TEMP01CODE.Visibility == Visibility.Visible) { return true; } else { return false; } }
            set
            {
                if (value)
                {
                    TEMP01CODE.Visibility = Visibility.Visible;
                    TEMP02CODE.Visibility = Visibility.Visible;
                    TEMP03CODE.Visibility = Visibility.Visible;
                    TEMP04CODE.Visibility = Visibility.Visible;
                    TEMP05CODE.Visibility = Visibility.Visible;
                    TEMP06CODE.Visibility = Visibility.Visible;
                    TEMP07CODE.Visibility = Visibility.Visible;
                    TEMP08CODE.Visibility = Visibility.Visible;
                    TEMP09CODE.Visibility = Visibility.Visible;
                    TEMP10CODE.Visibility = Visibility.Visible;
                    TEMP11CODE.Visibility = Visibility.Visible;
                    TEMP12CODE.Visibility = Visibility.Visible;
                    TEMP13CODE.Visibility = Visibility.Visible;
                    TEMP14CODE.Visibility = Visibility.Visible;
                    TEMP15CODE.Visibility = Visibility.Visible;
                    TEMP01VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP02VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP03VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP04VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP05VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP06VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP07VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP08VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP09VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP10VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP11VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP12VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP13VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP14VAL.Parent.SetValue(Grid.RowProperty, 1);
                    TEMP15VAL.Parent.SetValue(Grid.RowProperty, 1);
                }
                else
                {
                    TEMP01CODE.Visibility = Visibility.Hidden;
                    TEMP02CODE.Visibility = Visibility.Hidden;
                    TEMP03CODE.Visibility = Visibility.Hidden;
                    TEMP04CODE.Visibility = Visibility.Hidden;
                    TEMP05CODE.Visibility = Visibility.Hidden;
                    TEMP06CODE.Visibility = Visibility.Hidden;
                    TEMP07CODE.Visibility = Visibility.Hidden;
                    TEMP08CODE.Visibility = Visibility.Hidden;
                    TEMP09CODE.Visibility = Visibility.Hidden;
                    TEMP10CODE.Visibility = Visibility.Hidden;
                    TEMP11CODE.Visibility = Visibility.Hidden;
                    TEMP12CODE.Visibility = Visibility.Hidden;
                    TEMP13CODE.Visibility = Visibility.Hidden;
                    TEMP14CODE.Visibility = Visibility.Hidden;
                    TEMP15CODE.Visibility = Visibility.Hidden;
                    TEMP01VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP02VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP03VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP04VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP05VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP06VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP07VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP08VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP09VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP10VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP11VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP12VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP13VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP14VAL.Parent.SetValue(Grid.RowProperty, 0);
                    TEMP15VAL.Parent.SetValue(Grid.RowProperty, 0);
                }
            }
        }

        public cntMonitor()
        {
            InitializeComponent();
            TEMPviewIDs = false;
            TEMPcpu = "---";
        }

        public string GetTempID(uint id)
        {
            switch (id)
            {
                case 1:
                    return TEMP01ID;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "GetTempID[" + id + "]");
            }
        }

        public void SetTempID(uint id, string tempID)
        {
            switch (id)
            {
                case 1:
                    TEMP01ID = tempID;
                    break;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "GetTempID[" + id + "]");
            }
        }

        public string GetTempValue(uint id)
        {
            switch (id)
            {
                case 1:
                    return TEMP01VALUE;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "GetTempID[" + id + "]");
            }
        }

        public void SetTempValue(uint id, string tempValue)
        {
            switch (id)
            {
                case 1:
                    TEMP01VALUE = tempValue;
                    break;
                default:
                    throw new ArgumentException("A non-existent TEMP number was specified.", "GetTempID[" + id + "]");
            }
        }
    }

}
