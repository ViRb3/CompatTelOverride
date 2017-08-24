using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using CompatTelHelper;

namespace CompatTelOverride
{
    internal static class Program
    {
        public static readonly string ThisFile = Assembly.GetEntryAssembly().Location;

        private static void Main(string[] args)
        {
            if (args?.Length > 0 && args[0] == "/uninstall")
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
            string binPath = Path.Combine(Path.GetDirectoryName(ThisFile), "bin");

            if (myFiles.Any(f => !File.Exists(Path.Combine(binPath, f))))
            {
                MessageBox.Show("Files are missing! Please get a new copy of this software.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Directory.SetCurrentDirectory(binPath);

            if (!Common.IsAdministrator())
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
            NativeImports.Unblock(Common.WatchFile);
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
            NativeImports.Unblock(Common.RemoteFileOverride);
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
            if (!Common.IsAdministrator())
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
