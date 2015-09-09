using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Ex360E
{
    class Log
    {
        public static bool writeToFile;

        public static void LogError(string cause, string text)
        {
            if (writeToFile == true)
            {
                File.WriteAllText(".\\ex360e.log", "[E] {" + cause + "} " + text);
            }
        }

        public static void LogFatalError(string cause, string text)
        {
            if (writeToFile == true)
            {
                File.WriteAllText(".\\ex360e.log", "[FatalError] {" + cause + "} " + text);
            }
            DialogResult i = MessageBox.Show("Ex360e : Fatal Error!", "[FatalError] {" + cause + "} " + text, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            if (i == DialogResult.Abort)
            {
                Application.Exit();
            }
            else if (i == DialogResult.Retry)
            {
                Application.Exit();
            }
            else if (i == DialogResult.Ignore)
            {
                Application.Exit();
            }
        }

        public static void LogWarning(string cause, string text)
        {
            if (writeToFile == true)
            {
                File.WriteAllText(".\\ex360e.log", "[W] {" + cause + "} " + text);
            }
        }

        public static void LogInfo(string cause,string text)
        {
            if (writeToFile == true)
            {
                File.WriteAllText(".\\ex360e.log", "[!] {" + cause + "} " + text);
            }
        }
    }
}
