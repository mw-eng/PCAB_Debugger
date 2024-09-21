using System;
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
        public enum ROTATE
        {
            ZERO = 0,
            RIGHT_TURN = 1,
            LEFT_TURN = 2,
            HALF_TURN = 3,
            MIRROR_ZERO = 4,
            MIRROR_RIGHT_TURN = 5,
            MIRROR_LEFT_TURN = 6,
            MIRROR_HALF_TURN = 7,
            MATRIX = 8
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
        public void SetDSA(uint number, int value)
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
                if (ALL_DSA_CHECKBOX.IsChecked == true) { ALL_DSA_CHECKBOX.IsChecked = false; }
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
        public void SetDPS(uint number, int value)
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
                if (ALL_DSA_CHECKBOX.IsChecked == true) { ALL_DSA_CHECKBOX.IsChecked = false; }
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
        public string SerialNumber { get; private set; }
        private ROTATE angle;
        public ROTATE TURN
        {
            get { return angle; }
            set
            {
                angle = value;
                switch (angle)
                {
                    case ROTATE.RIGHT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 3);
                        P02_GRID.SetValue(Grid.RowProperty, 3);
                        P03_GRID.SetValue(Grid.RowProperty, 3);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 2);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 2);
                        P08_GRID.SetValue(Grid.RowProperty, 2);
                        P09_GRID.SetValue(Grid.RowProperty, 1);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 1);
                        P12_GRID.SetValue(Grid.RowProperty, 1);
                        P13_GRID.SetValue(Grid.RowProperty, 0);
                        P14_GRID.SetValue(Grid.RowProperty, 0);
                        P15_GRID.SetValue(Grid.RowProperty, 0);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 1);
                        P03_GRID.SetValue(Grid.ColumnProperty, 2);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 3);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 1);
                        P08_GRID.SetValue(Grid.ColumnProperty, 0);
                        P09_GRID.SetValue(Grid.ColumnProperty, 0);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 3);
                        P13_GRID.SetValue(Grid.ColumnProperty, 3);
                        P14_GRID.SetValue(Grid.ColumnProperty, 2);
                        P15_GRID.SetValue(Grid.ColumnProperty, 1);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.LEFT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 0);
                        P03_GRID.SetValue(Grid.RowProperty, 0);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 1);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 1);
                        P09_GRID.SetValue(Grid.RowProperty, 2);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 2);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 3);
                        P15_GRID.SetValue(Grid.RowProperty, 3);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 2);
                        P03_GRID.SetValue(Grid.ColumnProperty, 1);
                        P04_GRID.SetValue(Grid.ColumnProperty, 0);
                        P05_GRID.SetValue(Grid.ColumnProperty, 0);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 3);
                        P09_GRID.SetValue(Grid.ColumnProperty, 3);
                        P10_GRID.SetValue(Grid.ColumnProperty, 2);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 0);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 1);
                        P15_GRID.SetValue(Grid.ColumnProperty, 2);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.HALF_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 1);
                        P03_GRID.SetValue(Grid.RowProperty, 2);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 3);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 0);
                        P09_GRID.SetValue(Grid.RowProperty, 0);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 3);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 2);
                        P15_GRID.SetValue(Grid.RowProperty, 1);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 0);
                        P03_GRID.SetValue(Grid.ColumnProperty, 0);
                        P04_GRID.SetValue(Grid.ColumnProperty, 0);
                        P05_GRID.SetValue(Grid.ColumnProperty, 1);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 1);
                        P08_GRID.SetValue(Grid.ColumnProperty, 1);
                        P09_GRID.SetValue(Grid.ColumnProperty, 2);
                        P10_GRID.SetValue(Grid.ColumnProperty, 2);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 2);
                        P13_GRID.SetValue(Grid.ColumnProperty, 3);
                        P14_GRID.SetValue(Grid.ColumnProperty, 3);
                        P15_GRID.SetValue(Grid.ColumnProperty, 3);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_RIGHT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 3);
                        P02_GRID.SetValue(Grid.RowProperty, 3);
                        P03_GRID.SetValue(Grid.RowProperty, 3);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 2);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 2);
                        P08_GRID.SetValue(Grid.RowProperty, 2);
                        P09_GRID.SetValue(Grid.RowProperty, 1);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 1);
                        P12_GRID.SetValue(Grid.RowProperty, 1);
                        P13_GRID.SetValue(Grid.RowProperty, 0);
                        P14_GRID.SetValue(Grid.RowProperty, 0);
                        P15_GRID.SetValue(Grid.RowProperty, 0);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 2);
                        P03_GRID.SetValue(Grid.ColumnProperty, 1);
                        P04_GRID.SetValue(Grid.ColumnProperty, 0);
                        P05_GRID.SetValue(Grid.ColumnProperty, 0);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 3);
                        P09_GRID.SetValue(Grid.ColumnProperty, 3);
                        P10_GRID.SetValue(Grid.ColumnProperty, 2);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 0);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 1);
                        P15_GRID.SetValue(Grid.ColumnProperty, 2);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    case ROTATE.MIRROR_LEFT_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 0);
                        P03_GRID.SetValue(Grid.RowProperty, 0);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 1);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 1);
                        P09_GRID.SetValue(Grid.RowProperty, 2);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 2);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 3);
                        P15_GRID.SetValue(Grid.RowProperty, 3);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 1);
                        P03_GRID.SetValue(Grid.ColumnProperty, 2);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 3);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 1);
                        P08_GRID.SetValue(Grid.ColumnProperty, 0);
                        P09_GRID.SetValue(Grid.ColumnProperty, 0);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 3);
                        P13_GRID.SetValue(Grid.ColumnProperty, 3);
                        P14_GRID.SetValue(Grid.ColumnProperty, 2);
                        P15_GRID.SetValue(Grid.ColumnProperty, 1);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.MIRROR_HALF_TURN:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 1);
                        P03_GRID.SetValue(Grid.RowProperty, 2);
                        P04_GRID.SetValue(Grid.RowProperty, 3);
                        P05_GRID.SetValue(Grid.RowProperty, 3);
                        P06_GRID.SetValue(Grid.RowProperty, 2);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 0);
                        P09_GRID.SetValue(Grid.RowProperty, 0);
                        P10_GRID.SetValue(Grid.RowProperty, 1);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 3);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 2);
                        P15_GRID.SetValue(Grid.RowProperty, 1);
                        P16_GRID.SetValue(Grid.RowProperty, 0);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 3);
                        P03_GRID.SetValue(Grid.ColumnProperty, 3);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 2);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 2);
                        P09_GRID.SetValue(Grid.ColumnProperty, 1);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 1);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 0);
                        P15_GRID.SetValue(Grid.ColumnProperty, 0);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                    case ROTATE.MATRIX:
                        P01_GRID.SetValue(Grid.RowProperty, 0);
                        P02_GRID.SetValue(Grid.RowProperty, 0);
                        P03_GRID.SetValue(Grid.RowProperty, 0);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 1);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 1);
                        P08_GRID.SetValue(Grid.RowProperty, 1);
                        P09_GRID.SetValue(Grid.RowProperty, 2);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 2);
                        P12_GRID.SetValue(Grid.RowProperty, 2);
                        P13_GRID.SetValue(Grid.RowProperty, 3);
                        P14_GRID.SetValue(Grid.RowProperty, 3);
                        P15_GRID.SetValue(Grid.RowProperty, 3);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 0);
                        P02_GRID.SetValue(Grid.ColumnProperty, 1);
                        P03_GRID.SetValue(Grid.ColumnProperty, 2);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 0);
                        P06_GRID.SetValue(Grid.ColumnProperty, 1);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 3);
                        P09_GRID.SetValue(Grid.ColumnProperty, 0);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 2);
                        P12_GRID.SetValue(Grid.ColumnProperty, 3);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 1);
                        P15_GRID.SetValue(Grid.ColumnProperty, 2);
                        P16_GRID.SetValue(Grid.ColumnProperty, 3);
                        break;
                    default:
                        P01_GRID.SetValue(Grid.RowProperty, 3);
                        P02_GRID.SetValue(Grid.RowProperty, 2);
                        P03_GRID.SetValue(Grid.RowProperty, 1);
                        P04_GRID.SetValue(Grid.RowProperty, 0);
                        P05_GRID.SetValue(Grid.RowProperty, 0);
                        P06_GRID.SetValue(Grid.RowProperty, 1);
                        P07_GRID.SetValue(Grid.RowProperty, 2);
                        P08_GRID.SetValue(Grid.RowProperty, 3);
                        P09_GRID.SetValue(Grid.RowProperty, 3);
                        P10_GRID.SetValue(Grid.RowProperty, 2);
                        P11_GRID.SetValue(Grid.RowProperty, 1);
                        P12_GRID.SetValue(Grid.RowProperty, 0);
                        P13_GRID.SetValue(Grid.RowProperty, 0);
                        P14_GRID.SetValue(Grid.RowProperty, 1);
                        P15_GRID.SetValue(Grid.RowProperty, 2);
                        P16_GRID.SetValue(Grid.RowProperty, 3);
                        P01_GRID.SetValue(Grid.ColumnProperty, 3);
                        P02_GRID.SetValue(Grid.ColumnProperty, 3);
                        P03_GRID.SetValue(Grid.ColumnProperty, 3);
                        P04_GRID.SetValue(Grid.ColumnProperty, 3);
                        P05_GRID.SetValue(Grid.ColumnProperty, 2);
                        P06_GRID.SetValue(Grid.ColumnProperty, 2);
                        P07_GRID.SetValue(Grid.ColumnProperty, 2);
                        P08_GRID.SetValue(Grid.ColumnProperty, 2);
                        P09_GRID.SetValue(Grid.ColumnProperty, 1);
                        P10_GRID.SetValue(Grid.ColumnProperty, 1);
                        P11_GRID.SetValue(Grid.ColumnProperty, 1);
                        P12_GRID.SetValue(Grid.ColumnProperty, 1);
                        P13_GRID.SetValue(Grid.ColumnProperty, 0);
                        P14_GRID.SetValue(Grid.ColumnProperty, 0);
                        P15_GRID.SetValue(Grid.ColumnProperty, 0);
                        P16_GRID.SetValue(Grid.ColumnProperty, 0);
                        break;
                }
            }
        }

        public cntConfigSettings() : this("SN", ROTATE.ZERO) { }
        public cntConfigSettings(string serialNumber, ROTATE _turn)
        {
            InitializeComponent();
            SerialNumber = serialNumber;
            TURN = _turn;
            VIEW_COMBOBOX.SelectedIndex = (int)angle;
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
                        STBLNA_CHECKBOX.IsChecked = true;
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
                        STBLNA_CHECKBOX.IsChecked = false;
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
            DSA01_COMBOBOX.IsEnabled = false;
            DSA02_COMBOBOX.IsEnabled = false;
            DSA03_COMBOBOX.IsEnabled = false;
            DSA04_COMBOBOX.IsEnabled = false;
            DSA05_COMBOBOX.IsEnabled = false;
            DSA06_COMBOBOX.IsEnabled = false;
            DSA07_COMBOBOX.IsEnabled = false;
            DSA08_COMBOBOX.IsEnabled = false;
            DSA09_COMBOBOX.IsEnabled = false;
            DSA10_COMBOBOX.IsEnabled = false;
            DSA11_COMBOBOX.IsEnabled = false;
            DSA12_COMBOBOX.IsEnabled = false;
            DSA13_COMBOBOX.IsEnabled = false;
            DSA14_COMBOBOX.IsEnabled = false;
            DSA15_COMBOBOX.IsEnabled = false;
            DSA01_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA02_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA03_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA04_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA05_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA06_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA07_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA08_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA09_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA10_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA11_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA12_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA13_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA14_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            DSA15_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
        }

        private void ALL_DSA_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            ALL_DSA_COMBOBOX.IsEnabled = false;

            DSA01_COMBOBOX.IsEnabled = true;
            DSA02_COMBOBOX.IsEnabled = true;
            DSA03_COMBOBOX.IsEnabled = true;
            DSA04_COMBOBOX.IsEnabled = true;
            DSA05_COMBOBOX.IsEnabled = true;
            DSA06_COMBOBOX.IsEnabled = true;
            DSA07_COMBOBOX.IsEnabled = true;
            DSA08_COMBOBOX.IsEnabled = true;
            DSA09_COMBOBOX.IsEnabled = true;
            DSA10_COMBOBOX.IsEnabled = true;
            DSA11_COMBOBOX.IsEnabled = true;
            DSA12_COMBOBOX.IsEnabled = true;
            DSA13_COMBOBOX.IsEnabled = true;
            DSA14_COMBOBOX.IsEnabled = true;
            DSA15_COMBOBOX.IsEnabled = true;
        }

        private void ALL_DPS_CHECKBOX_Checked(object sender, RoutedEventArgs e)
        {
            ALL_DPS_COMBOBOX.IsEnabled = true;
            DPS01_COMBOBOX.IsEnabled = false;
            DPS02_COMBOBOX.IsEnabled = false;
            DPS03_COMBOBOX.IsEnabled = false;
            DPS04_COMBOBOX.IsEnabled = false;
            DPS05_COMBOBOX.IsEnabled = false;
            DPS06_COMBOBOX.IsEnabled = false;
            DPS07_COMBOBOX.IsEnabled = false;
            DPS08_COMBOBOX.IsEnabled = false;
            DPS09_COMBOBOX.IsEnabled = false;
            DPS10_COMBOBOX.IsEnabled = false;
            DPS11_COMBOBOX.IsEnabled = false;
            DPS12_COMBOBOX.IsEnabled = false;
            DPS13_COMBOBOX.IsEnabled = false;
            DPS14_COMBOBOX.IsEnabled = false;
            DPS15_COMBOBOX.IsEnabled = false;
            DPS01_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS02_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS03_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS04_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS05_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS06_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS07_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS08_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS09_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS10_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS11_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS12_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS13_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS14_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            DPS15_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
        }

        private void ALL_DPS_CHECKBOX_Unchecked(object sender, RoutedEventArgs e)
        {
            ALL_DPS_COMBOBOX.IsEnabled = false;
            DPS01_COMBOBOX.IsEnabled = true;
            DPS02_COMBOBOX.IsEnabled = true;
            DPS03_COMBOBOX.IsEnabled = true;
            DPS04_COMBOBOX.IsEnabled = true;
            DPS05_COMBOBOX.IsEnabled = true;
            DPS06_COMBOBOX.IsEnabled = true;
            DPS07_COMBOBOX.IsEnabled = true;
            DPS08_COMBOBOX.IsEnabled = true;
            DPS09_COMBOBOX.IsEnabled = true;
            DPS10_COMBOBOX.IsEnabled = true;
            DPS11_COMBOBOX.IsEnabled = true;
            DPS12_COMBOBOX.IsEnabled = true;
            DPS13_COMBOBOX.IsEnabled = true;
            DPS14_COMBOBOX.IsEnabled = true;
            DPS15_COMBOBOX.IsEnabled = true;
        }

        private void ALL_DSA_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DSA01_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA02_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA03_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA04_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA05_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA06_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA07_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA08_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA09_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA10_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA11_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA12_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA13_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA14_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
                DSA15_COMBOBOX.SelectedIndex = ALL_DSA_COMBOBOX.SelectedIndex;
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2147467261) { throw; }
            }
        }

        private void ALL_DPS_COMBOBOX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DPS01_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS02_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS03_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS04_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS05_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS06_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS07_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS08_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS09_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS10_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS11_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS12_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS13_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS14_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
                DPS15_COMBOBOX.SelectedIndex = ALL_DPS_COMBOBOX.SelectedIndex;
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2147467261) { throw; }
            }
        }

        private void VIEW_COMBOBOX_DropDownClosed(object sender, EventArgs e)
        {
            if(VIEW_COMBOBOX.SelectedIndex != (int)angle) { TURN = (ROTATE)VIEW_COMBOBOX.SelectedIndex; }
        }
    }
}
