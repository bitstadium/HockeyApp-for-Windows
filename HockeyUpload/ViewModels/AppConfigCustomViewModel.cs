using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HockeyApp.AppLoader.ViewModels
{
    public class AppConfigCustomViewModel : PlatformDependendViewModel
    {
        private AppInfo _app;
        public AppConfigCustomViewModel(AppInfo appInfo)
        {
            this._app = appInfo;
        }


        private string _regularExpression = "";
        public string RegularExpression
        {
            get { return this._regularExpression; }
            set
            {
                this._regularExpression = value;
                this.WasChanged = true;
                NotifyOfPropertyChange(() => this.RegularExpression);
            }
        }

        public override void Save()
        {
            this._app.RegularExpression = this.RegularExpression;
            this.WasChanged = false;
        }

        public override void Reset()
        {
            this.RegularExpression = this._app.RegularExpression;
            this.WasChanged = false;
        }

      
    }
}
