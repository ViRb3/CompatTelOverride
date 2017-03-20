using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Threading.Tasks;
using CompatTelHelper;

namespace CompatTelWatch
{
    public partial class Service1 : ServiceBase
    {
        private FileStream _lock;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(DoStuff);
        }

        private void DoStuff()
        {
            if (!File.Exists(Common.RemoteFileOverride))
                return;

            if (!File.Exists(Common.RemoteFile))
            {
                Override();
                return;
            }

            using (SHA256Managed crypto = new SHA256Managed())
            {
                using (var thisStream = File.OpenRead(Common.RemoteFileOverride))
                using (var remoteStream = File.OpenRead(Common.RemoteFile))
                {
                    var thisHash = crypto.ComputeHash(thisStream);
                    var remoteFileHash = crypto.ComputeHash(remoteStream);

                    if (thisHash.SequenceEqual(remoteFileHash))
                    {
                        //Process.Start(Common.RemoteFile);
                        _lock = new FileStream(Common.RemoteFile, FileMode.Open, FileAccess.Read, FileShare.None);
                        return;
                    }
                }
            }

            Override();
        }

        private void Override()
        {
            Common.KillProcesses("CompatTelRunner");

            if (File.Exists(Common.RemoteFile))
            {
                FileUtils.TakeOwnership(Common.RemoteFile);

                if (!File.Exists(Common.RemoteFileBackup))
                {
                    File.Move(Common.RemoteFile, Common.RemoteFileBackup);
                    FileUtils.RestorePermissions(Common.RemoteFileBackup);
                }
            }
            
            File.Copy(Common.RemoteFileOverride, Common.RemoteFile, true);
            FileUtils.RestorePermissions(Common.RemoteFile);

            //Process.Start(Common.RemoteFile);
            _lock = new FileStream(Common.RemoteFile, FileMode.Open, FileAccess.Read, FileShare.None);
        }

        protected override void OnStop()
        {
            _lock.Close();
        }
    }
}
