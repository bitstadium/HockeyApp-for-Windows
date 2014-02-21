using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HockeyApp.AppLoader.Extensions;
using Caliburn.Micro;
using System.Net.Http.Handlers;
using System.Threading;

namespace HockeyApp.AppLoader.ViewModels
{
    public class AppDialogViewModel:ViewModelBase
    {
        
        public AppDialogViewModel(AppInfo appInfo){
            this.AppInfo = appInfo;
            this.ResetToDefaults();
        }

        public AppInfo AppInfo { get; private set; }

        public string Title { get { return this.AppInfo.Title; } }
        public string AppId { get { return this.AppInfo.Id; } }
        public string PublicAppId { get { return this.AppInfo.PublicID; } }
        public string Platform { get { return this.AppInfo.Platform.GetDescription(); } }
        public string BundleId { get { return this.AppInfo.BundleID; } }
        public AppInfoReleaseType ReleaseType { get { return (AppInfoReleaseType)Int32.Parse(this.AppInfo.ReleaseType); } }

        public bool IsVersionEditable{get;set;}

        private string _version = "";
        public string Version
        {
            get { return this._version; }
            set
            {
                this._version = value;
                NotifyOfPropertyChange(() => this.Version);
                NotifyOfPropertyChange(() => this.CanUpload);
            }
        }
        public string FileToUpload { get; private set; }

        private string _notes = "";

        public string Notes
        {
            get
            {
                return this._notes;
            }
            set
            {
                this._notes = value;
                NotifyOfPropertyChange(()=>this.Notes);
            }
        }

        public AppInfoStatusType SelectedStatus { get; set; }
        public AppInfoNotifyType SelectedNotify { get; set; }

        public bool IsMandatoryEnabled { get { return this.AppInfo.Platform == AppInfoPlatforms.Android; } }
        public bool IsMandatory{get;set; }

        public void ResetToDefaults()
        {
            var args = IoC.Get<CommandLineArgs>();

            this.FileToUpload = args.Package;

            if (!String.IsNullOrWhiteSpace(args.Version))
            {
                this.Version = args.Version;
            }

            if (!String.IsNullOrWhiteSpace(args.Status))
            {
                this.SelectedStatus = args.StatusInternal;
            }
            else
            {
                this.SelectedStatus =(AppInfoStatusType)  this.AppInfo.DefaultStatusType;
            }

            if (!String.IsNullOrWhiteSpace(args.Notify))
            {
                this.SelectedNotify = args.NotifyTypeInternal;
            }
            else
            {
                this.SelectedNotify =(AppInfoNotifyType) this.AppInfo.DefaultNotifyType;
            }

            if (!String.IsNullOrWhiteSpace(args.IsMandatory))
            {
                this.IsMandatory = args.IsMandatoryInternal;
            }
            else
            {
                this.IsMandatory = this.AppInfo.DefaultIsMandatory;
            }

            if (!string.IsNullOrWhiteSpace(args.Notes))
            {
                this.Notes = args.Notes;
            }

            NotifyOfPropertyChange(() => this.CanUpload);
        }


        public bool CanUpload { get {
            return this.Version != null && this.Version.Length > 0;
        } }

        public void PrepareUpload()
        {
            this.AppInfo.Version = this.Version;
            this.AppInfo.Mandatory = this.IsMandatory;
            this.AppInfo.Status = (int)this.SelectedStatus;
            this.AppInfo.Notify = (int)this.SelectedNotify;
            this.AppInfo.Notes = this.Notes;
        }

    }
    
}
