using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.PlatformStrategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HockeyUploaderConsole
{
    public class AsyncUploader
    {
        public async Task<int> MainAsync(CommandLineArgs _args)
        {
            try
            {
                AppInfoMatcher matcher = new AppInfoMatcher();
                List<AppInfo> list = await matcher.GetMatchingApps(_args);

                if (list.Count == 0)
                {
                    Program.LogToConsole("No matching application found. Please check the configuration information!");
                    return -1;
                }
                else if (list.Count > 1)
                {
                    Program.LogToConsole("More than one apps are matching. Please change parameter!");
                    Program.LogToConsole("Matching apps: " + list.Select(p => p.Title).Aggregate((a, b) => a + "," + b));
                    return -1;
                }

                AppInfo app = list.First();
                app.Version = _args.Version;
                app.Status = !String.IsNullOrWhiteSpace(_args.Status)?(int)_args.StatusInternal: (int)app.DefaultStatusType;
                app.Notify = !String.IsNullOrWhiteSpace(_args.Notify) ? (int)_args.NotifyTypeInternal: (int)app.DefaultNotifyType;
                

                UploadStrategy uploader = UploadStrategy.GetStrategy(list.First());
                Program.LogToConsole("");
                DateTime start = DateTime.Now;

                await uploader.Upload(_args.Package, matcher.ActiveUserConfiguration, ProgressHandler, CancellationToken.None);
                
                DateTime end = DateTime.Now;
                TimeSpan sp = end.Subtract(start);
                FileInfo fi = new FileInfo(_args.Package);
                long length = fi.Length;
                length = length / 8;
                Program.LogToConsole("");
                Program.LogToConsole(string.Format("Uploaded {0}KB in {1}sec", length.ToString("###,###,###"), sp.Seconds.ToString("d")));
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    Program.LogToConsole("Error: " + ((AggregateException)ex).InnerException.Message);
                }
                else
                {
                    Program.LogToConsole("Error: " + ex.Message);
                }
                return -1;
            }
            return 0;
        }

        public void ProgressHandler(object sender, HttpProgressEventArgs e)
        {
            Program.LogToConsole("#", false);
        }
    }
}
