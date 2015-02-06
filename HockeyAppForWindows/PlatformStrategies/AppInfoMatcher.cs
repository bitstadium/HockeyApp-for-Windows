using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Deployment.WindowsInstaller.Linq;
using Microsoft.Deployment.WindowsInstaller;
using Caliburn.Micro;
using HockeyApp.AppLoader.Util;


namespace HockeyApp.AppLoader.PlatformStrategies
{
    public class AppInfoMatcher
    {
        private CommandLineArgs _args;
        
        private AppInfoEnvelope _envelope;
        private List<AppInfo> _matchingApps = new List<AppInfo>();

        public async Task<List<AppInfo>> GetMatchingApps(CommandLineArgs args)
        {
            ConfigurationStore configuration = ConfigurationStore.Instance;
            this._args=args;
            if (!String.IsNullOrWhiteSpace(args.Accountname))
            {
                ActiveUserConfiguration = configuration.UserConfigurations.FirstOrDefault(p => p.ConfigurationName.ToUpper().Equals(args.Accountname.ToUpper()));
            }
            else
            {
                ActiveUserConfiguration = configuration.DefaultUserConfiguration;
            }
            if (ActiveUserConfiguration == null)
            {
                throw new Exception("Wrong AccountName or no default account configured!");
            }

            this._envelope = await AppInfoEnvelope.Load(ActiveUserConfiguration);

            switch (args.Platform)
            {
                case AppInfoPlatforms.None:
                    throw new Exception("Platform not supported!");

                case AppInfoPlatforms.Android:
                    MatchAndroid();
                    break;
                case AppInfoPlatforms.Windows:
                    MatchWindows();
                    break;
                case AppInfoPlatforms.Custom:
                    MatchCustom();
                    break;
                case AppInfoPlatforms.WindowsPhone:
                    MatchWindowsPhone();
                    break;
            }

            if (_matchingApps.Count == 0)
            {
                this._matchingApps.AddRange(this._envelope.Apps.Where(p => p.Platform == args.Platform));
            }

            return _matchingApps;
        }

        public UserConfiguration ActiveUserConfiguration { get; private set; }

        private void MatchWindowsPhone()
        {
            List<AppInfo> apps = this._envelope.Apps.Where(p => p.Platform == AppInfoPlatforms.WindowsPhone).ToList();
            if(!this.MatchCommonSwitches(apps)){
                ZipArchive zip = null;
                if (_args.Package.ToUpper().EndsWith("APPXBUNDLE"))
                {
                    zip = GetInnerPackageFromBundle(_args.Package);
                }
                else
                {
                    zip = new ZipArchive(File.OpenRead(_args.Package), ZipArchiveMode.Read, false);
                }
                foreach (ZipArchiveEntry zipEntry in zip.Entries)
                {
                    if (zipEntry.Name.Equals("WMAppManifest.xml"))
                    {
                        if (String.IsNullOrWhiteSpace(this._args.Version))
                        {
                            this._args.Version = this.GetValueFromStream(zipEntry.Open(), "Version");
                        }
                    }
                    else if (zipEntry.Name.Equals("AppManifest.xaml"))
                    {
                        if (String.IsNullOrWhiteSpace(this._args.BundleID))
                        {
                            string entrypoint = this.GetValueFromStream(zipEntry.Open(), "EntryPointType");
                            if (!String.IsNullOrWhiteSpace(entrypoint)){
                                this._args.BundleID = entrypoint.Substring(0, entrypoint.LastIndexOf("."));
                            }
                        }
                    }
                    else if (zipEntry.Name.Equals("AppxManifest.xml"))
                    {
                        var appxManifest = AppxManifest.Create(zipEntry.Open());
                        if (appxManifest.Package != null)
                        {
                            if (appxManifest.Package.Identity + "" != "")
                            {
                                if (String.IsNullOrWhiteSpace(this._args.Version))
                                {
                                    this._args.Version = appxManifest.Package.Identity.Version + "";
                                }
                                if (String.IsNullOrWhiteSpace(this._args.BundleID))
                                {
                                    this._args.BundleID = appxManifest.Package.Identity.Name + "";
                                }
                            }
                        }
                    }
                }
                if (!String.IsNullOrWhiteSpace(this._args.BundleID))
                {
                    foreach (AppInfo app in apps)
                    {
                        if (app.BundleID.ToUpper().Equals(this._args.BundleID.ToUpper()))
                        {
                            this._matchingApps.Add(app);
                        }
                    }
                }
            }
        }

        private ZipArchive GetInnerPackageFromBundle(string filename)
        {
            ZipArchive retVal = null;
             ZipArchive zip = new ZipArchive(File.OpenRead(_args.Package),ZipArchiveMode.Read,false);
             List<string> applicationEntries = new List<string>();
             foreach (ZipArchiveEntry zipEntry in zip.Entries)
             {
                 if (zipEntry.Name.Equals("AppxBundleManifest.xml"))
                 {
                     using (Stream s = zipEntry.Open())
                     {
                         applicationEntries = AppxBundleManifest.GetApplicationEntries(s);
                     }

                 }
             }
             if (applicationEntries != null && applicationEntries.Count > 0)
             {
                 string f = applicationEntries[0];
                 foreach (ZipArchiveEntry zipEntry in zip.Entries)
                 {
                     if (zipEntry.Name.Equals(f))
                     {
                         retVal = new ZipArchive(zipEntry.Open());
                     }
                 }
             }

            return retVal;
        }

        private string GetValueFromStream(Stream stream, string key)
        {
            StreamReader reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                int begin = line.IndexOf(" " + key + "=\"", StringComparison.InvariantCulture);
                if (begin >= 0)
                {
                    int end = line.IndexOf("\"", begin + key.Length + 3, StringComparison.InvariantCulture);
                    if (end >= 0)
                    {
                        return line.Substring(begin + key.Length + 3, end - begin - key.Length - 3);
                    }
                }
            }
            return "";
        }

        private void MatchAndroid()
        {
            List<AppInfo> apps = this._envelope.Apps.Where(p => p.Platform == AppInfoPlatforms.Android).ToList();
            Dictionary<string, string> manifestInfos = GetManifestInfo();
            this._args.Version = manifestInfos["versionName"]; 

            if (!this.MatchCommonSwitches(apps)){

                foreach (AppInfo app in apps)
                {
                    if (app.BundleID.ToUpper().Equals(manifestInfos["name"].ToUpper()))
                    {
                        this._matchingApps.Add(app);
                    }
                }
            }
        }

        private void MatchWindows()
        {
            string productName = null;
            using (QDatabase database = new QDatabase(this._args.Package, DatabaseOpenMode.ReadOnly))
            {

                var properties = from p in database.Properties
                                 where p.Property == "ProductVersion" || p.Property == "ProductName"
                                 select p;

                foreach (var property in properties)
                {
                    if (property.Property.Equals("ProductVersion") && String.IsNullOrWhiteSpace(this._args.Version))
                    {
                        this._args.Version = property.Value;
                    }
                    else if (property.Property.Equals("ProductName"))
                    {
                        productName = property.Value;
                    }
                }
            }

            List<AppInfo> apps = this._envelope.Apps.Where(p => p.Platform == AppInfoPlatforms.Windows).ToList();
            if (!this.MatchCommonSwitches(apps) && productName != null)
            {
                foreach (AppInfo app in apps)
                {
                    if (app.Title.Contains(productName))
                    {
                        this._matchingApps.Add(app);
                    }
                }
            }
        }

        private void MatchCustom()
        {
            List<AppInfo> apps = this._envelope.Apps.Where(p => p.Platform == AppInfoPlatforms.Custom).ToList();
            if (!this.MatchCommonSwitches(apps))
            {
                foreach (AppInfo app in apps)
                {
                    if (!String.IsNullOrWhiteSpace(app.RegularExpression) && Regex.IsMatch(this._args.PackageWithoutExtension, app.RegularExpression))
                    {
                        this._matchingApps.Add(app);
                    }
                }
            }
        }

        private bool MatchCommonSwitches(List<AppInfo> apps)
        {
            if (!String.IsNullOrWhiteSpace(_args.Id))
            {
                var app = apps.FirstOrDefault(p => p.PublicID.Equals(_args.Id));
                if (app != null) { this._matchingApps.Add(app); }
                return true;
            }
            else if (!String.IsNullOrWhiteSpace(_args.BundleID))
            {
                foreach (AppInfo app in apps.Where(p => p.BundleID.ToUpper().Equals(this._args.BundleID.ToUpper())))
                {
                    this._matchingApps.Add(app);
                }
                return true;
            }
            return false;
        }


        private Dictionary<string, string> GetManifestInfo()
        {
            string aaptFile = HockeyApp.AppLoader.Properties.Settings.Default.AAPTPath;
            if (String.IsNullOrEmpty(aaptFile))
            {
                throw new Exception("No AAPT-Path configured. Check configuration!");
            }
            if (!File.Exists(aaptFile)){
                   throw new Exception("AAPT-Tool not found. Please check configuration!");
            }
            
            List<String> lines = new List<string>();
            Process proc = new Process(){
                StartInfo= new ProcessStartInfo()
                    {
                    FileName = aaptFile,
                    Arguments = @"d badging " + this._args.Package,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                    }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                lines.Add(proc.StandardOutput.ReadLine());
            }

            string versionInfo = lines[0];
            versionInfo=versionInfo.Substring(9); //without leading "package:"
            versionInfo=versionInfo.Replace("'",""); //delete '
            
            Dictionary<string,string> vInfo = new Dictionary<string,string>();
            foreach(string keyValue in versionInfo.Split(' ')){
                string[] arr = keyValue.Split('=');
                vInfo.Add(arr[0], arr[1]);
            }
            return vInfo;
        }


      
    }
}
