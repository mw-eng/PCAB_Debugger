using System;
using System.Windows.Media;

namespace PCAB_Debugger_ComLib
{
    public class ColorChart
    {
        public double MaxValue
        {
            get { return maxVAL; }
            set { if (value > minVAL) maxVAL = value; }
        }
        public double MinValue
        {
            get { return minVAL; }
            set { if (maxVAL > value) minVAL = value; }
        }

        private double maxVAL = double.MaxValue;
        private double minVAL = double.MinValue;
        public ColorChart() : this(double.MinValue, double.MaxValue) { }
        public ColorChart(double minVAL, double maxVAL)
        {
            if (maxVAL > minVAL)
            {
                this.maxVAL = maxVAL;
                this.minVAL = minVAL;
            }
            else
            {
                this.maxVAL = double.MaxValue;
                this.minVAL = double.MinValue;
            }
        }

        public Color getColor(double value) { return getColor(255, value); }

        public Color getColor(byte alpha,double value)
        {
            if(value == double.NaN) { return Color.FromArgb(alpha, 0, 0, 0); }
            double norm = (value - minVAL) / (maxVAL - minVAL);
            if (norm < 0.0) { return Color.FromArgb(alpha, 0, 0, 0xFF); }
            else if (0.0 <= norm && norm < 0.25) { return Color.FromArgb(alpha, 0, (byte)Math.Round((norm / 0.25 - 0) * 255, 0), 0xFF); }
            else if (0.25 <= norm && norm < 0.5) { return Color.FromArgb(alpha, 0, 0xFF, (byte)(255 - Math.Round((norm / 0.25 - 1) * 255, 0))); }
            else if (0.5 <= norm && norm < 0.75) { return Color.FromArgb(alpha, (byte)Math.Round((norm / 0.25 - 2) * 255, 0), 0xFF, 0); }
            else if (0.75 <= norm && norm < 1) { return Color.FromArgb(alpha, 0xFF, (byte)(255 - Math.Round((norm / 0.25 - 1) * 255, 0)), 0); }
            else { return Color.FromArgb(alpha, 0xFF, 0, 0); }
        }
    }
    public class NormalizedColorChart
    {
        public double MaxValue
        {
            get { return maxVAL; }
            set { if (value > minVAL) maxVAL = value; }
        }
        public double MinValue
        {
            get { return minVAL; }
            set { if (maxVAL > value) minVAL = value; }
        }

        private double maxVAL = 180.0;
        private double minVAL = -180.0;
        public NormalizedColorChart() : this(double.MinValue, double.MaxValue) { }
        public NormalizedColorChart(double minVAL, double maxVAL)
        {
            if (maxVAL > minVAL)
            {
                this.maxVAL = maxVAL;
                this.minVAL = minVAL;
            }
            else
            {
                this.maxVAL = double.MaxValue;
                this.minVAL = double.MinValue;
            }
        }

        public Color getColor(double value) { return getColor(255, value); }

        public Color getColor(byte alpha, double value)
        {
            if (value == double.NaN) { return Color.FromArgb(alpha, 0, 0, 0); }
            double norm = (value + maxVAL) % (maxVAL - minVAL) + minVAL;
            if (norm < minVAL) { norm += (maxVAL - minVAL); }

            norm = (norm - minVAL) / (maxVAL - minVAL);
            if (norm < 0) 
            { return Color.FromArgb(alpha, 0xFF, 0, 0); }
            else if (0.0 <= norm && norm < 1.0 * 0.5 / 3.0) 
            { return Color.FromArgb(alpha, 0xFF, 0, (byte)Math.Round((norm * 3.0 / 0.5 - 0) * 255, 0)); }
            else if (1.0 * 0.5 / 3.0 <= norm && norm < 2.0 * 0.5 / 3.0) 
            { return Color.FromArgb(alpha, (byte)(255 - Math.Round((norm * 3.0 / 0.5 - 1) * 255, 0)), 0, 0xFF); }
            else if (2.0 * 0.5 / 3.0 <= norm && norm < 0.5)
            { return Color.FromArgb(alpha, 0, (byte)Math.Round((norm * 3.0 / 0.5 - 1) * 255, 0), 0xFF); }
            else if (0.5 <= norm && norm < 4.0 * 0.5 / 3.0)
            { return Color.FromArgb(alpha, 0, 0xFF, (byte)(255 - Math.Round((norm * 3.0 / 0.5 - 3) * 255, 0))); }
            else if (4.0 * 0.5 / 3.0 <= norm && norm < 5.0 * 0.5 / 3.0)
            { return Color.FromArgb(alpha, (byte)Math.Round((norm * 3.0 / 0.5 - 4) * 255, 0), 0xFF, 0); }
            else if (5.0 * 0.5 / 3.0 <= norm && norm < 1)
            { return Color.FromArgb(alpha, 0xFF, (byte)(255 - Math.Round((norm * 3.0 / 0.5 - 5) * 255, 0)), 0); }
            else { return Color.FromArgb(alpha, 0xFF, 0, 0); }
        }
    }
    public static class ColorConversion
    {
        static Color ComplementaryColor(Color color)
        {
            byte maxVAL = color.R;
            byte minVAL = color.R;
            if (color.G > maxVAL) { maxVAL = color.G; }
            if (color.B > maxVAL) { maxVAL = color.B; }
            if (color.G < minVAL) { minVAL = color.G; }
            if (color.B < minVAL) { minVAL = color.B; }
            return Color.FromArgb(color.A, (byte)(maxVAL + minVAL - color.R), (byte)(maxVAL + minVAL - color.G), (byte)(maxVAL + minVAL - color.B));
        }

        static Color OppositeColor(Color color)
        {
            return Color.FromArgb(color.A, (byte)(0xFF - color.R), (byte)(0xFF - color.G), (byte)(0xFF - color.B));
        }
    }
}
