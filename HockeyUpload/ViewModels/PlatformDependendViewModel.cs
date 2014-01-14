using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.AppLoader.ViewModels
{
    public abstract class PlatformDependendViewModel:ViewModelBase
    {
        protected void Save(){
            ConfigurationStore.Instance.Save();
        }
    }
}
