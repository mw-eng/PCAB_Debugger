using MWComLibCS.FileFormats;
using System.Collections.Generic;
using MWComLibCS;
using MWComLibCS.CoordinateSystem;
using System;
using System.IO;

namespace PCAB_Debugger_ACS
{
    public static class clsReadDAT
    {
        public static bool ReadPortDAT(string filePath,
            out List<PANEL.PORT> PTU11, out List<PANEL.PORT> PTU12, out List<PANEL.PORT> PTU13,
            out List<PANEL.PORT> PTU21, out List<PANEL.PORT> PTU22, out List<PANEL.PORT> PTU23,
            out List<PANEL.PORT> PTU31, out List<PANEL.PORT> PTU32, out List<PANEL.PORT> PTU33)
        {
            PTU11 = new List<PANEL.PORT>();
            PTU12 = new List<PANEL.PORT>();
            PTU13 = new List<PANEL.PORT>();
            PTU21 = new List<PANEL.PORT>();
            PTU22 = new List<PANEL.PORT>();
            PTU23 = new List<PANEL.PORT>();
            PTU31 = new List<PANEL.PORT>();
            PTU32 = new List<PANEL.PORT>();
            PTU33 = new List<PANEL.PORT>();
            using(FileStream fs = new FileStream(filePath,FileMode.Open,FileAccess.Read, FileShare.ReadWrite))
            using (CSVformatStreamReader reader = new CSVformatStreamReader(fs))
            {
                string[] strLine = reader.ReadLine();
                while (strLine?.Length > 0 )
                {
                    if (strLine.Length == 7)
                    {
                        if(!string.IsNullOrWhiteSpace(strLine[0])&& !string.IsNullOrWhiteSpace(strLine[1])&&
                            !string.IsNullOrWhiteSpace(strLine[2])&& !string.IsNullOrWhiteSpace(strLine[3])&&
                            !string.IsNullOrWhiteSpace(strLine[4]) && !string.IsNullOrWhiteSpace(strLine[5]) && 
                            !string.IsNullOrWhiteSpace(strLine[6]))
                            {
                            if (strLine[0].Trim().Substring(0, 1) != "#")
                            {
                                uint unitNum;
                                uint portNum;
                                double x;
                                double y;
                                double z;
                                double att;
                                double pha;
                                if (uint.TryParse(strLine[0], out unitNum) &&
                                    uint.TryParse(strLine[1], out portNum) &&
                                    double.TryParse(strLine[2], out x) &&
                                    double.TryParse(strLine[3], out y) &&
                                    double.TryParse(strLine[4], out z) &&
                                    double.TryParse(strLine[5], out att) &&
                                    double.TryParse(strLine[6], out pha))
                                {
                                    PANEL.PORT portBF = new PANEL.PORT(portNum, new CoordinateSystem3D(x * Math.Pow(10, (double)SIunit.SIprefix.m), y * Math.Pow(10, (double)SIunit.SIprefix.m), z * Math.Pow(10, (double)SIunit.SIprefix.m)),
                                        new ComplexAngle(20.0, 10, att, new Angle(pha, false)));
                                    switch (unitNum)
                                    {
                                        case 11:
                                            PTU11.Add(portBF);
                                            break;
                                        case 12:
                                            PTU12.Add(portBF);
                                            break;
                                        case 13:
                                            PTU13.Add(portBF);
                                            break;
                                        case 21:
                                            PTU21.Add(portBF);
                                            break;
                                        case 22:
                                            PTU22.Add(portBF);
                                            break;
                                        case 23:
                                            PTU23.Add(portBF);
                                            break;
                                        case 31:
                                            PTU31.Add(portBF);
                                            break;
                                        case 32:
                                            PTU32.Add(portBF);
                                            break;
                                        case 33:
                                            PTU33.Add(portBF);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    strLine = reader.ReadLine();
                }
            }
            if( PTU11.Count == 15 && PTU12.Count == 15 && PTU13.Count == 15&&
                PTU21.Count == 15 && PTU22.Count == 15 && PTU23.Count == 15&&
                PTU31.Count == 15 && PTU32.Count == 15 && PTU33.Count == 15)
            { return true; }
            else { return false; }
        }
    }
}
