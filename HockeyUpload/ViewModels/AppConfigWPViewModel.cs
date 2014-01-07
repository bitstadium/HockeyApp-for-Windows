using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.AppLoader.ViewModels
{
    public class AppConfigWPViewModel : PlatformDependendViewModel
    {
        public AppConfigWPViewModel(AppInfo app)
        {
            
        }

        public override void Save()
        {
            
        }

        public override bool CanSave
        {
            get { return true; }
        }

        public override void Reset()
        {
            
        }

        public override bool CanReset
        {
            get { return true; }
        }
    }
}
