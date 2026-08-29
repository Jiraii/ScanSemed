using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDSender
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (sender, args) => 
            {
                System.IO.File.WriteAllText("error.txt", "ThreadException: " + args.Exception.ToString());
                MessageBox.Show(args.Exception.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => 
            {
                System.IO.File.WriteAllText("error_domain.txt", "UnhandledException: " + args.ExceptionObject.ToString());
                MessageBox.Show(args.ExceptionObject.ToString(), "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                        try {
                string tempPath = System.IO.Path.GetTempPath();
                string[] tempFiles = System.IO.Directory.GetFiles(tempPath, "~*.*");
                foreach (string f in tempFiles) { try { System.IO.File.Delete(f); } catch { } }
                string[] tempFiles2 = System.IO.Directory.GetFiles(tempPath, "*.tmp");
                foreach (string f in tempFiles2) { try { System.IO.File.Delete(f); } catch { } }
                string[] tempFiles3 = System.IO.Directory.GetFiles(tempPath, "*.rpt");
                foreach (string f in tempFiles3) { try { System.IO.File.Delete(f); } catch { } }
            } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try {
                Application.Run(new frm_main());
            } catch (Exception ex) {
                System.IO.File.WriteAllText("error_main.txt", "Main Exception: " + ex.ToString());
                MessageBox.Show(ex.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

