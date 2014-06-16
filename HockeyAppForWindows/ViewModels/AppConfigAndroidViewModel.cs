using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Model;

namespace HockeyApp.AppLoader.ViewModels
{
    public class AppConfigAndroidViewModel:PlatformDependendViewModel
    {

        private AppInfo _app;
        public AppConfigAndroidViewModel(AppInfo appInfo)
        {
            this._app = appInfo;
        }

        public bool IsMandatory
        {
            get
            {
                return this._app.DefaultIsMandatory;
            }
            set
            {
                this._app.DefaultIsMandatory = value;
                base.Save();
            }
        }

    }
}
