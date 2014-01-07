using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.PlatformStrategies;
using HockeyApp.AppLoader.Util;

namespace HockeyUploaderConsole
{
    class Program
    {
        private static CommandLineArgs _args = null;

        public static void LogToConsole(string message, bool withNewLine = true){
            if(_args != null && (_args.Verbose == null || _args.Verbose.ToUpper() != "FALSE")){
                if (withNewLine)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    Console.Write(message);
                }
            }
        }

        

        static void Main(string[] args)
        {

            new CrashHandler();

            string exeFile = System.Reflection.Assembly.GetEntryAssembly().Location;
            string exePath = Path.GetDirectoryName(exeFile);
            var tmp = System.Configuration.ConfigurationManager.OpenExeConfiguration(exePath + @"\HockeyUpload.exe");
            System.Configuration.KeyValueConfigurationCollection col = tmp.AppSettings.Settings;


            HockeyApp.AppLoader.Model.Configuration c = HockeyApp.AppLoader.Model.Configuration.Instance;
            if (Environment.CommandLine.ToUpper().Contains("/HELP"))
            {
                HockeyApp.AppLoader.Model.CommandLineArgs.WriteHelp(Console.Out, "HOCH");
            }
            else if(Environment.GetCommandLineArgs().Count() > 1)
            {
                try
                {
                    _args = Args.Configuration.Configure<HockeyApp.AppLoader.Model.CommandLineArgs>().CreateAndBind(Environment.GetCommandLineArgs());
                }
                catch
                {
                    HockeyApp.AppLoader.Model.CommandLineArgs.WriteHelp(Console.Out, "HOCH");
                    return;
                }

                string errMsg = "";
                if (!_args.IsValid(out errMsg))
                {
                    LogToConsole("Wrong parameter: " + errMsg);
                    return;
                }
                try
                {
                    AppInfoMatcher matcher = new AppInfoMatcher();
                    Task<List<AppInfo>> t = matcher.GetMatchingApps(_args);
                    t.Wait();

                    List<AppInfo> list = t.Result;
                    if (list.Count == 0)
                    {
                        LogToConsole("No matching application found. Please check the configuration information!");
                        return;
                    }
                    else if (list.Count > 1)
                    {
                        LogToConsole("More than one apps are matching. Please change parameter!");
                        LogToConsole("Matching apps: " + list.Select(p => p.Title).Aggregate((a, b) => a + "," + b));
                        return;
                    }

                    UploadStrategy uploader = UploadStrategy.GetStrategy(list.First());
                    LogToConsole("");
                    DateTime start = DateTime.Now;
                    Task task = uploader.Upload(_args.Package, matcher.ActiveUserConfiguration, ProgressHandler, CancellationToken.None);
                    task.Wait();
                    DateTime end = DateTime.Now;
                    TimeSpan sp = end.Subtract(start);
                    FileInfo fi = new FileInfo(_args.Package);
                    long length =  fi.Length;
                    length = length / 8;
                    LogToConsole("");
                    LogToConsole(string.Format("Uploaded {0}KB in {1}sec", length.ToString("###,###,###"), sp.Seconds.ToString("d")));
                }
                catch (Exception ex)
                {
                    LogToConsole("Error: " + ex.Message);
                }
            }
            else
            {
                HockeyApp.AppLoader.Model.CommandLineArgs.WriteHelp(Console.Out, "HOCH");
            }
        }

        public static void ProgressHandler(object sender, HttpProgressEventArgs e){
            LogToConsole("#", false);
        }
    }
}
