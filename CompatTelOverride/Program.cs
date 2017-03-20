using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using CompatTelHelper;

namespace CompatTelOverride
{
    class Program
    {
        public static readonly string ThisFile = Assembly.GetEntryAssembly().Location;

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        private static bool Unblock(string fileName)
        {
            return DeleteFile(fileName + ":Zone.Identifier");
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "/uninstall")
            {
                Uninstall();
                return;
            }

            if (!string.Equals(ThisFile, Common.RemoteFile, StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(Common.WatchFile))
                {
                    if(Uninstall())
                        Install();
                    return;
                }
                else
                {
                    Install();
                    return;
                }
            }

            // Override mode
            Common.KillProcesses("CompatTelRunner");

            while (true)
            {
                Thread.Sleep(5000);
            }  
        }

        private static void Install()
        {
            string[] myFiles = {"CompatTelWatch.exe"};
            Directory.SetCurrentDirectory(Path.GetDirectoryName(ThisFile));

            if (!myFiles.Any(File.Exists))
            {
                MessageBox.Show("Files are missing! Please get a new copy of this software.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!IsAdministrator())
            {
                MessageBox.Show("Insufficient privileges, please restart this executable as Administrator.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("Would you like to install?", "CompatTelOverride v2 ~ViRb3", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            InstallWatch();
            InstallOverride();

            ServiceController sc = new ServiceController {ServiceName = "CompatTelWatch"};
            sc.Start();

            MessageBox.Show("Successfully installed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void InstallWatch()
        {
            if (File.Exists(Common.WatchFile))   
                UninstallWatch();

            File.Copy("CompatTelWatch.exe", Common.WatchFile);
            Unblock(Common.WatchFile);
            FileUtils.RestorePermissions(Common.WatchFile);

            var installer = Process.Start(Common.InstallUtil, Common.WatchFile);
            installer.WaitForExit();
        }

        private static void UninstallWatch()
        {
            Common.KillProcesses("CompatTelWatch");

            var uninstaller = Process.Start(Common.InstallUtil, $"/u {Common.WatchFile}");
            uninstaller.WaitForExit();

            FileUtils.TakeOwnership(Common.WatchFile);
            File.Delete(Common.WatchFile);
        }

        private static void InstallOverride()
        {
            if (File.Exists(Common.RemoteFileOverride))
                FileUtils.TakeOwnership(Common.RemoteFileOverride);

            File.Copy(ThisFile, Common.RemoteFileOverride, true);
            Unblock(Common.RemoteFileOverride);
            FileUtils.RestorePermissions(Common.RemoteFileOverride);
        }

        private static void UninstallOverride()
        {
            Common.KillProcesses("CompatTelRunner");

            if (File.Exists(Common.RemoteFileBackup))
            {
                FileUtils.TakeOwnership(Common.RemoteFile);
                FileUtils.TakeOwnership(Common.RemoteFileBackup);

                File.Delete(Common.RemoteFile);
                File.Move(Common.RemoteFileBackup, Common.RemoteFile);

                FileUtils.RestorePermissions(Common.RemoteFile);
            }

            if (File.Exists(Common.RemoteFileOverride))
            {
                FileUtils.TakeOwnership(Common.RemoteFileOverride);
                File.Delete(Common.RemoteFileOverride);
            }
        }

        private static bool Uninstall()
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("Insufficient privileges, please restart this executable as Administrator.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            var result = MessageBox.Show("Would you like to uninstall?", "CompatTelOverride v2 ~ViRb3", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return false;

            if (File.Exists(Common.WatchFile))
                UninstallWatch();
            if (File.Exists(Common.RemoteFile))
                UninstallOverride();

            MessageBox.Show("Successfully uninstalled!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
    }
}
