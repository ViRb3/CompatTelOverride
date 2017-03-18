using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace CompatTelOverride
{
    class Program
    {
        public static readonly string RemoteFile = Path.Combine(Environment.SystemDirectory, "CompatTelRunner.exe");
        public static readonly string RemoteFileBackup = Path.Combine(Environment.SystemDirectory, "CompatTelRunner.exe.BAK");
        public static readonly string ThisFile = Assembly.GetEntryAssembly().Location;
        public static readonly string CmdFile = Path.Combine(Environment.SystemDirectory, "cmd.exe");

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void Main(string[] args)
        {
            if (!string.Equals(ThisFile, RemoteFile, StringComparison.OrdinalIgnoreCase))
            {
                HandleInstall();
                return;
            }

            Process[] processes = Process.GetProcessesByName("CompatTelRunner");
            Process thisProcess = Process.GetCurrentProcess();

            foreach (Process process in processes)
            {
                if (string.Equals(process.MainModule.FileName, RemoteFile, StringComparison.OrdinalIgnoreCase) && process.Id != thisProcess.Id)
                    process.Kill();
            }

            while (true)
            {
                Thread.Sleep(5000);
            }
        }

        private static void HandleInstall()
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("Insufficient privileges, please restart this executable as Administrator.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("Would you like to install?", "CompatTelOverride v1.0 ~ViRb3", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            if (FileHandler.Install())
            {
                Process.Start(RemoteFile);
                MessageBox.Show("Successfully installed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
