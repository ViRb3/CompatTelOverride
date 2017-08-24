using System.ComponentModel;
using System.Configuration.Install;

namespace CompatTelWatch
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
