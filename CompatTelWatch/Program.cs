using System.Reflection;
using System.ServiceProcess;

namespace CompatTelWatch
{
    internal static class Program
    {
        public static readonly string ThisFile = Assembly.GetEntryAssembly().Location;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] servicesToRun = {
                new WatchService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
