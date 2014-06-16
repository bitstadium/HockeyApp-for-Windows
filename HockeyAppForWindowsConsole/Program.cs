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

namespace HockeyAppForWindows.Hoch
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

            HockeyApp.AppLoader.Model.ConfigurationStore c = HockeyApp.AppLoader.Model.ConfigurationStore.Instance;
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
                AsyncUploader bs = new AsyncUploader();
                Task<int> t = bs.MainAsync(_args);
                t.Wait();
                Environment.ExitCode = t.Result;
            }
            else
            {
                HockeyApp.AppLoader.Model.CommandLineArgs.WriteHelp(Console.Out, "HOCH");
            }
            Console.ReadLine();
        }

        
    }
}
