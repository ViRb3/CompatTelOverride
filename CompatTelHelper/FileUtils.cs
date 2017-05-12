using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using ProcessPrivileges;

namespace CompatTelHelper
{
    public static class FileUtils
    {
        public static bool TakeOwnership(string file)
        {
            using (new PrivilegeEnabler(Process.GetCurrentProcess(), Privilege.TakeOwnership))
            {
                SecurityIdentifier si = WindowsIdentity.GetCurrent().User;
                if (si == null)
                {
                    return false;
                }

                var access = File.GetAccessControl(file);
                access.SetOwner(si);
                File.SetAccessControl(file, access);

                access.SetAccessRule(new FileSystemAccessRule(si, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(file, access);
            }

            return true;
        }

        public static bool RestorePermissions(string file)
        {
            using (new PrivilegeEnabler(Process.GetCurrentProcess(), Privilege.Restore))
            {
                SecurityIdentifier siTrustedInstaller = (SecurityIdentifier)new NTAccount("NT SERVICE\\TrustedInstaller").Translate(typeof(SecurityIdentifier));

                var systemAccess = File.GetAccessControl(Common.CmdFile);
                var access = File.GetAccessControl(file);

                access.SetAccessRuleProtection(true, false); // remove inherited rules

                foreach (FileSystemAccessRule rule in access.GetAccessRules(true, false, typeof(SecurityIdentifier)))
                    access.RemoveAccessRule(rule); // remove explicit rules

                foreach (FileSystemAccessRule rule in systemAccess.GetAccessRules(true, false, typeof(SecurityIdentifier)))
                    access.AddAccessRule(rule); // copy explicit rules from system file

                // deny TrustedInstaller write access
                //access.SetAccessRule(new FileSystemAccessRule(siTrustedInstaller, FileSystemRights.ReadAndExecute, AccessControlType.Allow));

                File.SetAccessControl(file, access);

                access.SetOwner(siTrustedInstaller);
                File.SetAccessControl(file, access);
            }

            return true;
        }
    }
}
