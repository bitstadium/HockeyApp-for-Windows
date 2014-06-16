using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.AppLoader.PlatformStrategies
{
    public static class PlatformDependendConfigurationStrategyFactory
    {
        public static PlatformDependendViewModel GetConfigurationViewModel(AppInfo appInfo)
        {
            PlatformDependendViewModel retVal = null;

            switch (appInfo.Platform)
            {
                case AppInfoPlatforms.WindowsPhone:
                    retVal = new AppConfigWPViewModel(appInfo);
                    break;
                case AppInfoPlatforms.MacOS:
                    retVal = new NotSupportedPlatformViewModel();
                    break;
                case AppInfoPlatforms.Android:
                    retVal = new AppConfigAndroidViewModel(appInfo);
                    break;
                case AppInfoPlatforms.iOS:
                    retVal = new NotSupportedPlatformViewModel();
                    break;
                case AppInfoPlatforms.Windows:
                    retVal = new AppConfigWindowsViewModel();
                    break;
                case AppInfoPlatforms.Custom:
                    retVal = new AppConfigCustomViewModel(appInfo);
                    break;
            }
            if (retVal == null)
            {
                retVal = new NotSupportedPlatformViewModel();
            }

            return retVal;
        }
    }
}
