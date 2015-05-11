using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace QlikConnector
{
    [RunInstaller(true)]
    public partial class InstallerClass : System.Configuration.Install.Installer
    {
        public InstallerClass()
        {
            InitializeComponent();

            
        }

        private void startProcess(string FileName, string Arguments, params string[] ArgumentsParams)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = FileName;
            startInfo.Arguments = string.Format(Arguments, ArgumentsParams);
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            this.startProcess("cmd.exe", "/C icacls \"{0}\\connections\" /grant {1}:(OI)(CI)M", this.Context.Parameters["path"], this.Context.Parameters["user"]);
            this.startProcess("cmd.exe", "/C del \"{0}\\verpatch.exe\"", this.Context.Parameters["path"]);
        }
    }
}
