using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using ProcessPrivileges;

namespace CompatTelOverride
{
    internal static class FileHandler
    {
        internal static bool Install()
        {
            if (File.Exists(Program.RemoteFile))
            {
                if (!TakeOwnership())
                    return false;

                HandleExistingFile();
            }

            File.Copy(Program.ThisFile, Program.RemoteFile);

            SetStartup();
            RestorePermissions();
            RestoreBackupPermissions();

            return true;
        }

        private static void HandleExistingFile()
        {
            Process[] processes = Process.GetProcessesByName("CompatTelRunner");

            foreach (Process process in processes)
            {
                if (string.Equals(process.MainModule.FileName, Program.RemoteFile, StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }

            if (!File.Exists(Program.RemoteFileBackup))
                File.Move(Program.RemoteFile, Program.RemoteFileBackup);
            else
                File.Delete(Program.RemoteFile);
        }

        private static bool SetStartup()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (registryKey == null)
            {
                MessageBox.Show("Could not add entry to start-up!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            registryKey.SetValue("CompatTelOverride", Program.RemoteFile);
            return true;
        }

        private static bool TakeOwnership()
        {
            using (new PrivilegeEnabler(Process.GetCurrentProcess(), Privilege.TakeOwnership))
            {
                SecurityIdentifier si = WindowsIdentity.GetCurrent().User;
                if (si == null)
                {
                    MessageBox.Show("Could not get current user ID!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var access = File.GetAccessControl(Program.RemoteFile);
                access.SetOwner(si);
                File.SetAccessControl(Program.RemoteFile, access);

                access.SetAccessRule(new FileSystemAccessRule(si, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(Program.RemoteFile, access);
            }

            return true;
        }

        private static bool RestorePermissions()
        {
            using (new PrivilegeEnabler(Process.GetCurrentProcess(), Privilege.Restore))
            {
                SecurityIdentifier siTrustedInstaller = (SecurityIdentifier)new NTAccount("NT SERVICE\\TrustedInstaller").Translate(typeof(SecurityIdentifier));

                var systemAccess = File.GetAccessControl(Program.CmdFile);
                var access = File.GetAccessControl(Program.RemoteFile);

                access.SetAccessRuleProtection(true, false); // remove inherited rules

                foreach (FileSystemAccessRule rule in access.GetAccessRules(true, false, typeof(SecurityIdentifier)))
                        access.RemoveAccessRule(rule); // remove explict rules

                foreach (FileSystemAccessRule rule in systemAccess.GetAccessRules(true, false, typeof(SecurityIdentifier)))
                    access.AddAccessRule(rule); // copy explict rules from system file

                // deny TrustedInstaller write access
                access.SetAccessRule(new FileSystemAccessRule(siTrustedInstaller, FileSystemRights.ReadAndExecute, AccessControlType.Allow));

                File.SetAccessControl(Program.RemoteFile, access);

                access.SetOwner(siTrustedInstaller);
                File.SetAccessControl(Program.RemoteFile, access);
            }

            return true;
        }

        private static bool RestoreBackupPermissions()
        {
            using (new PrivilegeEnabler(Process.GetCurrentProcess(), Privilege.Restore))
            {
                SecurityIdentifier si = WindowsIdentity.GetCurrent().User;
                if (si == null)
                {
                    MessageBox.Show("Could not get current user ID!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var access = File.GetAccessControl(Program.RemoteFileBackup);
                SecurityIdentifier siTrustedInstaller = (SecurityIdentifier) new NTAccount("NT SERVICE\\TrustedInstaller").Translate(typeof(SecurityIdentifier));

                access.RemoveAccessRule(new FileSystemAccessRule(si, FileSystemRights.FullControl, AccessControlType.Allow));
                access.SetOwner(siTrustedInstaller);
                File.SetAccessControl(Program.RemoteFileBackup, access);
            }

            return true;
        }
    }
}
