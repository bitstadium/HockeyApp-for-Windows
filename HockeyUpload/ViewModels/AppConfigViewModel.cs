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

namespace HockeyApp.AppLoader.ViewModels
{

    public class AppConfigViewModel:ViewModelBase
    {
        private AppInfo _app;
        private Configuration _configuration;
        public AppConfigViewModel(AppInfo app)
        {
            this._configuration = IoC.Get<Configuration>();
            this._app = app;
            this.ResetValues();
        }

        private bool _wasChanged=false;
        private bool WasChanged
        {
            get { return this._wasChanged; }
            set
            {
                this._wasChanged = value;
                NotifyOfPropertyChange(() => this.WasChanged);
                NotifyOfPropertyChange(() => this.CanSave);
                NotifyOfPropertyChange(() => this.CanResetValues);
            }
        }

        public string AppId { get { return this._app.Id; } }
        public string PublicAppID { get { return this._app.PublicID; } }
        public string Title { get { return this._app.Title; } }
        public string Platform { get { return this._app.Platform.GetDescription(); } }
        public string BundleId { get { return this._app.BundleID; } }
        public AppInfoReleaseType ReleaseType { get { return (AppInfoReleaseType)Int32.Parse(this._app.ReleaseType); } }

        /*
        private AppInfoReleaseType _selectedReleaseType;
        public AppInfoReleaseType SelectedReleaseType
        {
            get { return this._selectedReleaseType; }
            set
            {
                this._selectedReleaseType = value;
                this.WasChanged = true;
                NotifyOfPropertyChange(() => this.SelectedReleaseType);
            }
        }
         */

        private AppInfoStatusType _selectedStatus;
        public AppInfoStatusType SelectedStatus
        {
            get { return this._selectedStatus; }
            set
            {
                this._selectedStatus = value;
                this.WasChanged = true;
                NotifyOfPropertyChange(() => this.SelectedStatus);
            }
        }
        private AppInfoNotifyType _selectedNotify;
        public AppInfoNotifyType SelectedNotify
        {
            get { return this._selectedNotify; }
            set
            {
                this._selectedNotify = value;
                this.WasChanged = true;
                NotifyOfPropertyChange(() => this.SelectedNotify);
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
                    this._platformDependendData.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                    {
                        this.NotifyOfPropertyChange("");
                    };
                }
                return this._platformDependendData;
            }
        }

        #region Commands
        public void Save()
        {
            //this._app.DefaultReleaseType = (int)this.SelectedReleaseType;
            this._app.DefaultStatusType = (int)this.SelectedStatus;
            this._app.DefaultNotifyType = (int)this.SelectedNotify;
            this.PlatformDependendData.Save();
            this._configuration.Save();
            this.WasChanged = false;
        }

        public bool CanSave { get { return this.WasChanged || this.PlatformDependendData.CanSave; } }

        public void ResetValues()
        {
            //this.SelectedReleaseType = (AppInfoReleaseType)this._app.DefaultReleaseType;
            this.SelectedStatus = (AppInfoStatusType) this._app.DefaultStatusType;
            this.SelectedNotify = (AppInfoNotifyType)this._app.DefaultNotifyType;
            this.PlatformDependendData.Reset();
            this.WasChanged = false;
        }

        public bool CanResetValues { get { return this.WasChanged || this.PlatformDependendData.CanSave; } }
        #endregion

    }
}
