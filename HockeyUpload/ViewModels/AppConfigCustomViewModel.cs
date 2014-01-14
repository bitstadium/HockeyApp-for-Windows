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


        public string RegularExpression
        {
            get { return this._app.RegularExpression; }
            set
            {
                this._app.RegularExpression = value;
                base.Save();
            }
        }

    }
}
