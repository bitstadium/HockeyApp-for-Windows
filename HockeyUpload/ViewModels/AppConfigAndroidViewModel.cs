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


        private bool _isMandatory = false;
        public bool IsMandatory
        {
            get
            {
                return this._isMandatory;
            }
            set
            {
                this._isMandatory = value;
                this.WasChanged = true;
                NotifyOfPropertyChange(() => this.IsMandatory);
            }
        }

        public override void Save()
        {
            this._app.DefaultIsMandatory = this.IsMandatory;
            this.WasChanged = false;
        }

        public override void Reset()
        {
            this.IsMandatory = this._app.DefaultIsMandatory;
            this.WasChanged = false;
        }
    }
}
