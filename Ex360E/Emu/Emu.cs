using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ex360E;
namespace Ex360E.Emu
{
    class Emu
    {
        /// <summary>
        /// Emumode 1 - interpreter
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="emumode"></param>
        public static void Start(string filename,int emumode)
        {
            byte[] xexFile = File.ReadAllBytes(filename);

            if (xexFile[0] == 0x58 &&
                xexFile[1] == 0x45 &&
                xexFile[2] == 0x58 &&
                xexFile[3] == 0x32)
            {
                System.Diagnostics.ProcessStartInfo xexTool = new System.Diagnostics.ProcessStartInfo(".\\xextool.exe");
                xexTool.Arguments = "-b \"" + filename.Replace(".xex",".exe") + "\" \"" + filename + "\""; 
                System.Diagnostics.Process.Start(xexTool);
            }
            else
            {
                Log.LogFatalError("XEXFile", "Error! not a valid XEX file!");
            }
        }
    }
}
