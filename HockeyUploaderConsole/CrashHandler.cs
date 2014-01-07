using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HockeyApp;
using HockeyApp.Model;

namespace HockeyUploaderConsole
{
    public class CrashHandler
    {
        private readonly string _crashFilePrefix = "crashinfo_";
        private readonly string _appID = "1c6db542f926e5cac5df8c23f7bb2ba5";
        private readonly string crashFilePath;
        public CrashHandler()
        {

            HockeyApp.HockeyClient.Configure(this._appID, Assembly.GetExecutingAssembly().GetName().Version.ToString());

            this.crashFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.crashFilePath = Path.Combine(this.crashFilePath, "Hoch");

            AppDomain.CurrentDomain.UnhandledException += (a, b) =>
            {
                string crashID = Guid.NewGuid().ToString();
                String filename = String.Format("{0}{1}.log", _crashFilePrefix, crashID);

                CrashLogInformation logInfo = new CrashLogInformation()
                {
                    PackageName = "HockeyUploaderConsole",
                    Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    OperatingSystem = Environment.OSVersion.ToString(),
                };

                ICrashData crash = HockeyClient.Instance.CreateCrashData(b.ExceptionObject as Exception, logInfo);
                FileStream stream = File.Create(Path.Combine(this._crashFilePrefix, filename));
                crash.Serialize(stream);
                stream.Flush();
                stream.Close();
            };

            if (Directory.Exists(this.crashFilePath))
            {
                foreach (string filename in Directory.GetFiles(this.crashFilePath, _crashFilePrefix + "*.log"))
                {
                    try
                    {
                        FileStream fs = File.OpenRead(filename);
                        ICrashData cd = HockeyClient.Instance.Deserialize(fs);
                        fs.Close();
                        cd.SendDataAsync().Wait();
                        File.Delete(filename);
                    }
                    catch (Exception ex)
                    {
                        Program.LogToConsole("Error sending crash-information: " + ex.Message);
                    }
                }
            }
        }
    }
}
