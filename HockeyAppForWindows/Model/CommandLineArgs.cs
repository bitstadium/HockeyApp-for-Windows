using Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using HockeyApp.AppLoader.Util;

namespace HockeyApp.AppLoader.Model
{





    public class CommandLineArgs
    {
        public static string[] SupportedFileExtensions = new string[]
        {
            ".APK",
            ".APPX",
            ".APPXBUNDLE",
            ".MSI",
            ".ZIP",
            ".XAP"
        };


        [ArgsMemberSwitch(0)]
        public string ExeFile { get; set; }

        [ArgsMemberSwitch(1)]   
        [Description("Fully qualified package filename to upload")]
        public string Package { get; set; }

        [ArgsMemberSwitch("version")]
        [Description("Version information for new version")]
        public string Version  {get;set;}

        [ArgsMemberSwitch("configuration")]
        [Description("Account name")]
        public string Accountname { get; set; }

        [ArgsMemberSwitch("app_id")]
        [Description("HockeyApp Id")]
        public string Id { get; set; }

        [ArgsMemberSwitch("bundle_id")]
        [Description("Bundle identifier")]
        public string BundleID { get; set; }

        [ArgsMemberSwitch("status")]
        [Description("Status [created, downloadable]")]
        public string Status { get; set; }

        [ArgsMemberSwitch("notify")]
        [Description("Notify [none, eMail]")]
        public string Notify { get; set; }

        [ArgsMemberSwitch("mandatory")]
        [Description("Mandatory [true, false]")]
        public string IsMandatory { get; set; }

        [ArgsMemberSwitch("notes")]
        [Description("Notes")]
        public string Notes { get; set; }

        [ArgsMemberSwitch("notes_file")]
        [Description("Textfile with release notes (use when using multiple lines). Overwrites /notes.")]
        public string NotesFile { get; set; }


        [ArgsMemberSwitch("verbose")]
        [Description("Verbose [true,false]")]
        public string Verbose { get; set; }

        [ArgsMemberSwitch("help")]
        [Description("Prints the usage")]
        public string Help { get; set; }

        #region internal


        public static void WriteHelp(TextWriter outStream, string commandName)
        {
            var definition = Args.Configuration.Configure<HockeyApp.AppLoader.Model.CommandLineArgs>();
            var formatter = new ConsoleHelpFormatterCustom(80, 1, 5, commandName);
            var help = new Args.Help.HelpProvider().GenerateModelHelp(definition);
            formatter.WriteHelp(help, outStream);
        }


        private AppInfoStatusType _statusInternal;
        public AppInfoStatusType StatusInternal
        {
            get
            {
                return this._statusInternal;
            }
        }

        private AppInfoNotifyType _notifyTypeInternal;
        public AppInfoNotifyType NotifyTypeInternal
        {
            get
            {
                return this._notifyTypeInternal;
            }
        }

        private bool _isMandatoryInternal = false;
        public bool IsMandatoryInternal
        {
            get
            {
                return this._isMandatoryInternal;
            }
        }

        public AppInfoPlatforms Platform
        {
            get {
                AppInfoPlatforms retVal = AppInfoPlatforms.None;
                if (!String.IsNullOrWhiteSpace(this.Package))
                {
                    string ext = Path.GetExtension(this.Package);
                    ext = ext.ToUpper();
                    switch (ext)
                    {
                        case ".APK":
                            retVal = AppInfoPlatforms.Android;
                            break;
                        case ".APPX":
                            retVal = AppInfoPlatforms.WindowsPhone;
                            break;
                        case ".APPXBUNDLE":
                            retVal = AppInfoPlatforms.WindowsPhone;
                            break;
                        case ".MSI":
                            retVal = AppInfoPlatforms.Windows;
                            break;
                        case ".XAP":
                            retVal = AppInfoPlatforms.WindowsPhone;
                            break;
                        case ".ZIP":
                            retVal = AppInfoPlatforms.Custom;
                            break;
                    }
                }
                return retVal;
            }
        }

        public string PackageWithoutExtension
        {
            get
            {
                string retVal = "";
                if (!string.IsNullOrWhiteSpace(this.Package))
                {
                    string ext = Path.GetExtension(this.Package);
                    retVal = this.Package.Substring(0, this.Package.Length - ext.Length);
                }
                return retVal;
            }
        }
        #endregion



        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";

            if (!File.Exists(this.Package))
            {
                errorMessage = "File does not exist!";
                return false;
            }

            if(!String.IsNullOrWhiteSpace(this.Status)){
                if (!Enum.TryParse<AppInfoStatusType>(this.Status, true, out this._statusInternal))
                {
                    errorMessage = "Wrong value for parameter <Status>. Allowed valus: [created, downloadable].";
                    return false;
                }
            }
            if(!String.IsNullOrWhiteSpace(this.Notify)){
                if (!Enum.TryParse<AppInfoNotifyType>(this.Notify, true, out this._notifyTypeInternal))
                {
                    errorMessage = "Wrong value for parameter <Notify>. Allowed values: [none, email].";
                    return false;
                }
            }
            if(!String.IsNullOrWhiteSpace(this.IsMandatory)){
                if (!String.IsNullOrWhiteSpace(this.IsMandatory))
                {
                    if (!Boolean.TryParse(this.IsMandatory, out this._isMandatoryInternal)){
                        errorMessage = "Wrong value for parameter <IsMandatory>. Allowed values: [true, false].";
                    }
                }
            }
            
            return true; ;
        }


    }
}
