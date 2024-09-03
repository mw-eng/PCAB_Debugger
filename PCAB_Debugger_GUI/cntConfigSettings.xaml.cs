using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PCAB_Debugger_GUI
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

        public delegate void CheckboxClickEventHandler(object sender, RoutedEventArgs e, CheckBoxCategory cat, bool? isChecked);
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
            get { return STBLNA_CHECKBOX.IsChecked; }
            set
            {
                switch (value)
                {
                    case true:
                        CHECKBOX_Checked("STBLNA", null);
                        break;
                    case false:
                        CHECKBOX_Unchecked("STBLNA", null);
                        break;
                    default:
                        CHECKBOX_Indeterminate("STBLNA", null);
                        break;
                }
            }
        }
        public void SetDSA(uint number, uint value)
        {
            try
            {
                switch (number)
                {
                    case 0:
                        DSA16_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 1:
                        DSA01_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 2:
                        DSA02_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 3:
                        DSA03_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 4:
                        DSA04_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 5:
                        DSA05_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 6:
                        DSA06_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 7:
                        DSA07_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 8:
                        DSA08_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 9:
                        DSA09_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 10:
                        DSA10_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 11:
                        DSA11_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 12:
                        DSA12_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 13:
                        DSA13_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 14:
                        DSA14_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 15:
                        DSA15_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    default:
                        throw new ArgumentException("A non-existent DSA number was specified.", "SetDSA[" + number + "]");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int GetDSA(uint number)
        {
            switch (number)
            {
                case 0:
                    return DSA16_COMBOBOX.SelectedIndex;
                case 1:
                    return DSA01_COMBOBOX.SelectedIndex;
                case 2:
                    return DSA02_COMBOBOX.SelectedIndex;
                case 3:
                    return DSA03_COMBOBOX.SelectedIndex;
                case 4:
                    return DSA04_COMBOBOX.SelectedIndex;
                case 5:
                    return DSA05_COMBOBOX.SelectedIndex;
                case 6:
                    return DSA06_COMBOBOX.SelectedIndex;
                case 7:
                    return DSA07_COMBOBOX.SelectedIndex;
                case 8:
                    return DSA08_COMBOBOX.SelectedIndex;
                case 9:
                    return DSA09_COMBOBOX.SelectedIndex;
                case 10:
                    return DSA10_COMBOBOX.SelectedIndex;
                case 11:
                    return DSA11_COMBOBOX.SelectedIndex;
                case 12:
                    return DSA12_COMBOBOX.SelectedIndex;
                case 13:
                    return DSA13_COMBOBOX.SelectedIndex;
                case 14:
                    return DSA14_COMBOBOX.SelectedIndex;
                case 15:
                    return DSA15_COMBOBOX.SelectedIndex;
                default:
                    throw new ArgumentException("A non-existent DSA number was specified.", "GetDSA[" + number + "]");
            }
        }
        public void SetDPS(uint number, uint value)
        {
            try
            {
                switch (number)
                {
                    case 1:
                        DPS01_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 2:
                        DPS02_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 3:
                        DPS03_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 4:
                        DPS04_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 5:
                        DPS05_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 6:
                        DPS06_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 7:
                        DPS07_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 8:
                        DPS08_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 9:
                        DPS09_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 10:
                        DPS10_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 11:
                        DPS11_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 12:
                        DPS12_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 13:
                        DPS13_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 14:
                        DPS14_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    case 15:
                        DPS15_COMBOBOX.SelectedIndex = (int)value;
                        break;
                    default:
                        throw new ArgumentException("A non-existent DPS number was specified.", "SetDPS[" + number + "]");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int GetDPS(uint number)
        {
            switch (number)
            {
                case 1:
                    return DPS01_COMBOBOX.SelectedIndex;
                case 2:
                    return DPS02_COMBOBOX.SelectedIndex;
                case 3:
                    return DPS03_COMBOBOX.SelectedIndex;
                case 4:
                    return DPS04_COMBOBOX.SelectedIndex;
                case 5:
                    return DPS05_COMBOBOX.SelectedIndex;
                case 6:
                    return DPS06_COMBOBOX.SelectedIndex;
                case 7:
                    return DPS07_COMBOBOX.SelectedIndex;
                case 8:
                    return DPS08_COMBOBOX.SelectedIndex;
                case 9:
                    return DPS09_COMBOBOX.SelectedIndex;
                case 10:
                    return DPS10_COMBOBOX.SelectedIndex;
                case 11:
                    return DPS11_COMBOBOX.SelectedIndex;
                case 12:
                    return DPS12_COMBOBOX.SelectedIndex;
                case 13:
                    return DPS13_COMBOBOX.SelectedIndex;
                case 14:
                    return DPS14_COMBOBOX.SelectedIndex;
                case 15:
                    return DPS15_COMBOBOX.SelectedIndex;
                default:
                    throw new ArgumentException("A non-existent DPS number was specified.", "GetDPS[" + number + "]");
            }
        }

        public cntConfigSettings()
        {
            InitializeComponent();
        }

        private void CHECKBOX_Checked(object sender, RoutedEventArgs e)
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
                CheckboxClickEvent?.Invoke(sender, e, cat, true);
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
                        STBLNA_CHECKBOX.IsChecked = true;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = true;
                        break;
                    default: break;
                }
            }
        }

        private void CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
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
                CheckboxClickEvent?.Invoke(sender, e, cat, false);
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
                        STBLNA_CHECKBOX.IsChecked = true;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = true;
                        break;
                    default: break;
                }
            }
        }

        private void CHECKBOX_Indeterminate(object sender, RoutedEventArgs e)
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
                CheckboxClickEvent?.Invoke(sender, e, cat, null);
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
                        STBLNA_CHECKBOX.IsChecked = null;
                        break;
                    case "LPM":
                        SETLPM_CHECKBOX.IsChecked = null;
                        break;
                    default: break;
                }
            }
        }

        private void ALL_DSA_CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            ALL_DSA_COMBOBOX.IsEnabled = true;
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID") && ((Grid)objBf).Name != "P16_GRID")
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DSA[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).IsEnabled = false;
                                    ((ComboBox)objChild).SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DSA_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            ALL_DSA_COMBOBOX.IsEnabled = false;
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID") && ((Grid)objBf).Name != "P16_GRID")
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DSA[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).IsEnabled = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DPS_CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            ALL_DPS_COMBOBOX.IsEnabled = true;
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID"))
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DPS[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).IsEnabled = false;
                                    ((ComboBox)objChild).SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DPS_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            ALL_DPS_COMBOBOX.IsEnabled = false;
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID"))
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DPS[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).IsEnabled = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DSA_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Port_GRID == null) { return; }
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID") && ((Grid)objBf).Name != "P16_GRID")
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DSA[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ALL_DPS_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Port_GRID == null) { return; }
            foreach (object objBf in Port_GRID.Children)
            {
                if (typeof(Grid) == objBf.GetType())
                {
                    if (Regex.IsMatch(((Grid)objBf).Name, "P[0-1][0-9]_GRID"))
                    {
                        foreach (object objChild in ((Grid)objBf).Children)
                        {
                            if (typeof(ComboBox) == objChild.GetType())
                            {
                                if (Regex.IsMatch(((ComboBox)objChild).Name, "DPS[0-1][0-9]_COMBOBOX"))
                                {
                                    ((ComboBox)objChild).SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
