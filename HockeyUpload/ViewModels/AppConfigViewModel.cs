using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.PlatformStrategies;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using HockeyApp.AppLoader.Extensions;
using System.Threading.Tasks;
using System.Net;

namespace HockeyApp.AppLoader.ViewModels
{

    public class AppConfigViewModel:ViewModelBase
    {

        private AppInfo _app;
        private ConfigurationStore _configuration;
        private UserConfiguration _userConfiguration = null;
        public AppConfigViewModel(AppInfo app, UserConfiguration uc)
        {
            this._configuration = IoC.Get<ConfigurationStore>();
            this._userConfiguration = uc;
            this._app = app;
            Task t = this._app.LoadAppIcon(this._userConfiguration);
            t.ContinueWith(p => { this.NotifyOfPropertyChange(()=>this.AppImage); });
        }


        public string AppId { get { return this._app.Id; } }
        public string PublicAppID { get { return this._app.PublicID; } }
        public string Title { get { return this._app.Title; } }
        public string Platform { get { return this._app.Platform.GetDescription(); } }
        public string BundleId { get { return this._app.BundleID; } }
        public AppInfoReleaseType ReleaseType { get { return (AppInfoReleaseType)Int32.Parse(this._app.ReleaseType); } }

        
        public string AppImage { get {
            string retVal = "";
            if (string.IsNullOrWhiteSpace(this._app.AppImage))
            {
                retVal = "pack://application:,,,/Resources/NoIcon.png";
            }
            else
            {
                retVal = this._app.AppImage;
            }
            return retVal;
        } }

        public string PlatformIcon
        {
            get
            {
                string retVal = "";
                switch (this._app.Platform)
                {
                    case AppInfoPlatforms.None:
                        break;
                    case AppInfoPlatforms.WindowsPhone:
                        retVal = "pack://application:,,,/Resources/windowsphone.png";
                        break;
                    case AppInfoPlatforms.MacOS:
                        retVal = "pack://application:,,,/Resources/apple.png";
                        break;
                    case AppInfoPlatforms.Android:
                        retVal = "pack://application:,,,/Resources/android.png";
                        break;
                    case AppInfoPlatforms.iOS:
                        retVal = "pack://application:,,,/Resources/ios.png";
                        break;
                    case AppInfoPlatforms.Windows:
                        retVal = "pack://application:,,,/Resources/windows.png";
                        break;
                    case AppInfoPlatforms.Custom:
                        retVal = "pack://application:,,,/Resources/CustomPlatform.png";
                        break;
                    default:
                        break;
                }
                return retVal;
            }
        }

        public double CornerRadius
        {
            get
            {
                if (this._app.Platform == AppInfoPlatforms.iOS)
                {
                    return 7.5;
                }
                return 0;
            }
        }

        public AppInfoStatusType SelectedStatus
        {
            get { return (AppInfoStatusType)this._app.DefaultStatusType; }
            set
            {
                this._app.DefaultStatusType = (int)value;
                this.SaveAppConfig();
            }
        }
        
        public AppInfoNotifyType SelectedNotify
        {
            get { return (AppInfoNotifyType)this._app.DefaultNotifyType; }
            set
            {
                this._app.DefaultNotifyType = (int)value;
                this.SaveAppConfig();
            }
        }

        private PlatformDependendViewModel _platformDependendData = null;
        public PlatformDependendViewModel PlatformDependendData
        {
            get
            {
                if (this._platformDependendData == null)
                {
                    this._platformDependendData = PlatformDependendConfigurationStrategyFactory.GetConfigurationViewModel(this._app);
                }
                return this._platformDependendData;
            }
        }

        public bool IsPlatformSupported
        {
            get { return !(this.PlatformDependendData is NotSupportedPlatformViewModel); }
        }

        protected void SaveAppConfig()
        {
            ConfigurationStore.Instance.Save();
        }

    }
}
