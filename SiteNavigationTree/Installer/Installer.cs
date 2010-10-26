using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace KWizCom.Sharepoint.WebParts.SiteNavigationTree
{
    [RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        string targetDir = string.Empty;

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            if (!string.IsNullOrEmpty(base.Context.Parameters["TARGETDIR"].ToString()))
                stateSaver.Add("TARGETDIR", base.Context.Parameters["TARGETDIR"].ToString());
        }
        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
            ReadSavedState(savedState, ref targetDir);
            Execute(targetDir, "installwp.bat");
        }
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);
            ReadSavedState(savedState, ref targetDir);
            Execute(targetDir, "uninstallwp.bat");
        }
        public override void Rollback(System.Collections.IDictionary savedState)
        {
            base.Rollback(savedState);
            ReadSavedState(savedState, ref targetDir);
            Execute(targetDir, "uninstallwp.bat");
        }

        private void ReadSavedState(System.Collections.IDictionary savedState, ref string targetDir)
        {
            try
            {
                if (savedState != null)
                {
                    if (savedState.Contains("TARGETDIR") && !string.IsNullOrEmpty(savedState["TARGETDIR"].ToString()))
                    {
                        targetDir = savedState["TARGETDIR"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteEventLog("KWizCom", "ReadSavedState: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }

        }

        private void Execute(string directory, string filename)
        {
            try
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    if (directory.EndsWith(@"\\"))
                        directory = directory.Substring(0, directory.Length - 1);
                    //write information to the event log
                    WriteEventLog("KWizCom", "Executing " + directory + filename, EventLogEntryType.Information);

                    System.Diagnostics.ProcessStartInfo i = new System.Diagnostics.ProcessStartInfo(filename);
                    i.CreateNoWindow = true;
                    i.UseShellExecute = true;
                    i.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    i.WorkingDirectory = directory;
                    System.Diagnostics.Process.Start(i);
                }
            }
            catch(Exception ex)
            {
                WriteEventLog("KWizCom", ex.Message, EventLogEntryType.Error);
            }
        }
        /// <summary>
        /// write message to event log
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        private void WriteEventLog(string source, string message, System.Diagnostics.EventLogEntryType logType)
        {
            try
            {
                System.Diagnostics.EventLog.WriteEntry(source, message, logType);
            }
            catch //skip error
            {
            }
        }

    }
}
