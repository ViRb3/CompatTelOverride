using System;
using System.Diagnostics;
using System.IO;

namespace CompatTelHelper
{
    public static class Common
    {
        public static readonly string RemoteFile = Path.Combine(Environment.SystemDirectory, "CompatTelRunner.exe");
        public static readonly string RemoteFileOverride = Path.Combine(Environment.SystemDirectory, "CompatTelRunner.dll");
        public static readonly string RemoteFileBackup = Path.Combine(Environment.SystemDirectory, "CompatTelRunner.exe.BAK");

        public static readonly string WatchFile = Path.Combine(Environment.SystemDirectory, "CompatTelWatch.exe");
        public static readonly string HelperFile = Path.Combine(Environment.SystemDirectory, "CompatTelHelper.dll");
        public static readonly string CmdFile = Path.Combine(Environment.SystemDirectory, "cmd.exe");

        public static readonly string InstallUtil = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "installutil.exe");

        public static void KillProcesses(string name)
        {
            Process[] processes = Process.GetProcessesByName(name);
            Process thisProcess = Process.GetCurrentProcess();

            foreach (Process process in processes)
            {
                if (string.Equals(process.MainModule.FileName, RemoteFile, StringComparison.OrdinalIgnoreCase)
                    && process.Id != thisProcess.Id)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
        }
    }
}
