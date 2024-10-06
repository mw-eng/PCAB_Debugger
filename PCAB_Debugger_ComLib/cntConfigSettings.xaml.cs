using System;
using System.Windows;
using System.Windows.Controls;
using static PCAB_Debugger_ComLib.cntConfigPorts;

namespace PCAB_Debugger_ComLib
{
    /// <summary>
    /// cntConfigSettings.xaml の相互作用ロジック
    /// </summary>
    public partial class cntConfigSettings : UserControl
    {
        public enum CheckBoxCategory
        {
            StandbyAMP,
            StandbyDRA,
            LowPowerMode,
            StandbyLNA,
            NULL
        }

        public delegate void CheckboxClickEventHandler(object sender, RoutedEventArgs e, CheckBoxCategory category, bool? isChecked);
        public event CheckboxClickEventHandler CheckboxClickEvent;
        public bool? StandbyAMP
        {
            get { return STBAMP_CHECKBOX.IsChecked; }
            set {
                switch (value)
                {
                    case true:
                        CHECKBOX_Checked("STBAMP", null);
                        break;
                    case false:
                        CHECKBOX_Unchecked("STBAMP", null);
                        break;
                    default:
                        CHECKBOX_Indeterminate("STBAMP", null);
                        break;
                }
            }
        }
        public bool? StandbyDRA
        {
            get { return STBDRA_CHECKBOX.IsChecked; }
            set
            {
                switch (value)
                {
                    case true:
                        CHECKBOX_Checked("STBDRA", null);
                        break;
                    case false:
                        CHECKBOX_Unchecked("STBDRA", null);
                        break;
                    default:
                        CHECKBOX_Indeterminate("STBDRA", null);
                        break;
                }
            }
        }
        public bool? LowPowerMode
        {
            get { return SETLPM_CHECKBOX.IsChecked; }
            set
            {
                switch (value)
                {
                    case true:
                        CHECKBOX_Checked("LPM", null);
                        break;
                    case false:
                        CHECKBOX_Unchecked("LPM", null);
                        break;
                    default:
                        CHECKBOX_Indeterminate("LPM", null);
                        break;
                }
            }
        }
        public bool? StandbyLNA
        {
            get { return CONFIG_PORTS.StandbyLNA; }
            set { CONFIG_PORTS.StandbyLNA = value; }
        }

        public void SetDSA(uint number, int value) { CONFIG_PORTS.SetDSA(number, value);     }
        public int GetDSA(uint number){ return CONFIG_PORTS.GetDSA(number); }
        public void SetDPS(uint number, int value) { CONFIG_PORTS.SetDPS(number,value); }
        public int GetDPS(uint number) { return CONFIG_PORTS.GetDPS(number); }
        public string SerialNumber { get; private set; }

        public bool? ALL_DPS { get { return CONFIG_PORTS.ALL_DPS_CHECKBOX.IsChecked; } set { CONFIG_PORTS.ALL_DPS_CHECKBOX.IsChecked = value; } }
        public bool? ALL_DSA { get { return CONFIG_PORTS.ALL_DSA_CHECKBOX.IsChecked; } set { CONFIG_PORTS.ALL_DSA_CHECKBOX.IsChecked = value; } }

        public cntConfigSettings() : this("SN", ROTATE.ZERO) { }
        public cntConfigSettings(string serialNumber, ROTATE _turn)
        {
            InitializeComponent();
            CONFIG_PORTS.STBLNA_CheckboxClickEvent += STBLNA_CheckboxClickEvent;
            SerialNumber = serialNumber;
            CONFIG_PORTS.TURN = _turn;
            VIEW_COMBOBOX.SelectedIndex = (int)CONFIG_PORTS.TURN;
        }

        public void CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            if (typeof(CheckBox) == sender.GetType())
            {
                CheckBoxCategory cat = CheckBoxCategory.NULL;
                switch (((CheckBox)sender).Name)
                {
                    case "STBAMP_CHECKBOX":
                        cat = CheckBoxCategory.StandbyAMP;
                        break;
                    case "STBDRA_CHECKBOX":
                        cat = CheckBoxCategory.StandbyDRA;
                        break;
                    case "STBLNA_CHECKBOX":
                        cat = CheckBoxCategory.StandbyLNA;
                        break;
                    case "SETLPM_CHECKBOX":
                        cat = CheckBoxCategory.LowPowerMode;
                        break;
                    default: break;
                }
                CheckboxClickEvent?.Invoke(this, e, cat, true);
            }
            else
            {
                switch ((string)sender)
                {
                    case "STBAMP":
                        STBAMP_CHECKBOX.IsChecked = true;
                        break;
                    case "STBDRA":
                        STBDRA_CHECKBOX.IsChecked = true;
                        break;
                    case "STBLNA":
                        StandbyLNA = true;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = true;
                        break;
                    default: break;
                }
            }
        }

        public void CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            if (typeof(CheckBox) == sender.GetType())
            {
                CheckBoxCategory cat = CheckBoxCategory.NULL;
                switch (((CheckBox)sender).Name)
                {
                    case "STBAMP_CHECKBOX":
                        cat = CheckBoxCategory.StandbyAMP;
                        break;
                    case "STBDRA_CHECKBOX":
                        cat = CheckBoxCategory.StandbyDRA;
                        break;
                    case "STBLNA_CHECKBOX":
                        cat = CheckBoxCategory.StandbyLNA;
                        break;
                    case "SETLPM_CHECKBOX":
                        cat = CheckBoxCategory.LowPowerMode;
                        break;
                    default: break;
                }
                CheckboxClickEvent?.Invoke(this, e, cat, false);
            }
            else
            {
                switch ((string)sender)
                {
                    case "STBAMP":
                        STBAMP_CHECKBOX.IsChecked = false;
                        break;
                    case "STBDRA":
                        STBDRA_CHECKBOX.IsChecked = false;
                        break;
                    case "STBLNA":
                        StandbyLNA = false;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = false;
                        break;
                    default: break;
                }
            }
        }

        public void CHECKBOX_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (typeof(CheckBox) == sender.GetType())
            {
                CheckBoxCategory cat = CheckBoxCategory.NULL;
                switch (((CheckBox)sender).Name)
                {
                    case "STBAMP_CHECKBOX":
                        cat = CheckBoxCategory.StandbyAMP;
                        break;
                    case "STBDRA_CHECKBOX":
                        cat = CheckBoxCategory.StandbyDRA;
                        break;
                    case "STBLNA_CHECKBOX":
                        cat = CheckBoxCategory.StandbyLNA;
                        break;
                    case "SETLPM_CHECKBOX":
                        cat = CheckBoxCategory.LowPowerMode;
                        break;
                    default: break;
                }
                CheckboxClickEvent?.Invoke(this, e, cat, null);
            }
            else
            {
                switch ((string)sender)
                {
                    case "STBAMP":
                        STBAMP_CHECKBOX.IsChecked = null;
                        break;
                    case "STBDRA":
                        STBDRA_CHECKBOX.IsChecked = null;
                        break;
                    case "STBLNA":
                        StandbyLNA = null;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = null;
                        break;
                    default: break;
                }
            }
        }

        private void VIEW_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if(VIEW_COMBOBOX.SelectedIndex != (int)CONFIG_PORTS.TURN) { CONFIG_PORTS.TURN = (ROTATE)VIEW_COMBOBOX.SelectedIndex; }
        }

        private void STBLNA_CheckboxClickEvent(object sender, RoutedEventArgs e, bool? isChecked)
        {
            CheckboxClickEvent?.Invoke(this, e, CheckBoxCategory.StandbyLNA, isChecked);
        }

    }
}
