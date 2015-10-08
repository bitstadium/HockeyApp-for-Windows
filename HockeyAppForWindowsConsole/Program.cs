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
using HockeyApp;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using System.Security.Cryptography;

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

#if DEBUG
            HockeyApp.HockeyClient.Current.Configure(HockeyApp.AppLoader.DemoConstants.AppId);
            ((HockeyClient)HockeyClient.Current).OnHockeySDKInternalException += (sender, a1) =>
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
            };
#else
            HockeyApp.HockeyClient.Current.Configure(HockeyApp.AppLoader.DemoConstants.AppId);
#endif


            var tCrashes = HockeyApp.HockeyClient.Current.SendCrashesAsync(true);

            // Create new Telemetry Client with proper iKey
            var AIClient = new TelemetryClient();
            AIClient.InstrumentationKey = "a53c58f3-f38a-4911-a806-c91371cc2aa5";

            // Set AnonUserId using SHA256 hash.
            SHA256 sha = SHA256Managed.Create();
            byte[] hashedBytes = sha.ComputeHash(Encoding.Unicode.GetBytes(Environment.UserName));
            StringBuilder anonID = new StringBuilder();
            foreach (byte b in hashedBytes)
            {
                anonID.AppendFormat("{0:X2}", b);
            }
            AIClient.Context.User.Id = anonID.ToString();

            // Log a PageView as a substitute for a session event
            AIClient.TrackPageView("WindowsUploadConsole App Start");

            // Flush so that the messages are sent before closing down
            AIClient.Flush();


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

            tCrashes.Wait();

            
            #if DEBUG
                Console.ReadLine();
            #endif   
            
        }

        
    }
}
