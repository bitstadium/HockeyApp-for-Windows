using HockeyApp.AppLoader.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.ComponentModel.DataAnnotations;

namespace HockeyApp.AppLoader.Model
{

    public enum AppInfoStatusType
    {
        [Display(Name="created",Order=1)]
        created = 1,
        [Display(Name = "downloadable", Order = 1)]
        downloadable = 2
    }

    public enum AppInfoNotifyType
    {
        [Display(Name = "none", Order = 1)]
        none = 0,
        [Display(Name = "Email", Order = 2)]
        eMail = 1
    }

    public enum AppInfoReleaseType
    {
        [Display(Name = "alpha", Order = 1)]
        alpha = 2,
        [Display(Name = "beta", Order = 2)]
        beta = 0,
        [Display(Name = "live", Order = 3)]
        live = 1
    }

    
    public enum AppInfoPlatforms
    {
        [Display(Name = "Unknown", Order = 0)]
        None,
        [Display(Name = "Windows Phone", Order = 1)]
        WindowsPhone,
        [Display(Name = "Mac OS", Order = 2)]
        MacOS,
        [Display(Name = "Android", Order = 3)]
        Android,
        [Display(Name = "iOS", Order = 4)]
        iOS,
        [Display(Name = "Windows", Order = 5)]
        Windows,
        [Display(Name = "Custom", Order = 6)]
        Custom
    }


    [DataContract]
    public class AppInfo
    {   

        [DataMember(Name="id")]
        public string Id { get; set; }

        [DataMember(Name="title")]
        public string Title{get;set;}

        [DataMember(Name="bundle_identifier")]
        public string BundleID{get;set;}

        [DataMember(Name="public_identifier")]
        public string PublicID{get;set;}

        [DataMember(Name="platform")]
        private string platform{get;set;}

        [DataMember(Name="release_type")]
        public string ReleaseType { get; set; }

        [DataMember(Name="custom_release_type")]
        public string CustomReleaseType{get;set;}

        [DataMember(Name="role")]
        public string Role { get; set; }

        [DataMember(Name="owner")]
        public string Owner{get;set;}

        [DataMember(Name = "company")]
        public string Company { get; set; }

        [DataMember(Name = "status")]
        public int? Status { get; set; }

        [DataMember(Name = "notify")]
        public int? Notify { get; set; }

        [DataMember(Name = "device_family")]
        public string DeviceFamily { get; set; }

        [DataMember(Name = "minimum_os_version")]
        public string MinimumOsVersion { get; set; }
        
        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "shortversion")]
        public string Shortversion { get; set; }

        [DataMember(Name = "timestamp")]
        public string Timestamp { get; set; }

        [DataMember(Name = "app_size")]
        public Int64? AppSize { get; set; }

        [DataMember(Name = "mandatory")]
        public bool? Mandatory{get;set;}

        [DataMember(Name = "company_token")]
        public string CompanyToken { get; set; }

        [DataMember(Name = "notes")]
        public string Notes { get; set; }


        #region Wrapper
        public AppInfoPlatforms Platform
        {
            get {
                return (AppInfoPlatforms) (new AppInfoPlatformsTypeConverter()).ConvertFrom(this.platform);
            }
            set
            {
                this.platform = value.GetDescription();
            }
        }
        #endregion


        #region Local Data
        public void CompleteWithLocalData(AppInfo app)
        {
            this.RegularExpression = app.RegularExpression;
            this.DefaultNotifyType = app.DefaultNotifyType;
            this.DefaultStatusType = app.DefaultStatusType;
            this.DefaultReleaseType = app.DefaultReleaseType;
            this.DefaultIsMandatory = app.DefaultIsMandatory;
        }

        [DataMember]
        public string RegularExpression { get; set; }
        [DataMember]
        private int defaultStatusType = 1;
        public int DefaultStatusType
        {
            get
            {
                if (this.defaultStatusType == 0) { this.defaultStatusType = 1; }
                return this.defaultStatusType;
            }
            set { this.defaultStatusType = value; }
        }
        [DataMember]
        private int defaultNotifyType = 0;
        public int DefaultNotifyType { get { return this.defaultNotifyType; } set { this.defaultNotifyType = value; } }

        [DataMember]
        private int _defaultReleaseType = 0;
        public int DefaultReleaseType { get { return this._defaultReleaseType; } set { this._defaultReleaseType = value; } }

        [DataMember]
        private bool _defaultIsMandatory = false;
        public bool DefaultIsMandatory { get { return this._defaultIsMandatory; } set { this._defaultIsMandatory = value; } }

        #endregion

        #region Upload

       




        #endregion

    }
}
