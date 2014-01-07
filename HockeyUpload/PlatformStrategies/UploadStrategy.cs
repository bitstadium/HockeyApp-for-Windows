using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Model;

namespace HockeyApp.AppLoader.PlatformStrategies
{
    public abstract class UploadStrategy
    {
        public static UploadStrategy GetStrategy(AppInfo forApp)
        {
            switch (forApp.Platform)
            {
                case AppInfoPlatforms.WindowsPhone:
                    return new UploadStrategyCustom(forApp);
                    
                case AppInfoPlatforms.Android:
                    return new UploadStrategyAndroid(forApp);
                    
                case AppInfoPlatforms.Windows:
                    return new UploadStrategyCustom(forApp);
                    
                case AppInfoPlatforms.Custom:
                    return new UploadStrategyCustom(forApp);

            }
            return null;
        }

        protected AppInfo _appInfo;
        protected UploadStrategy(AppInfo appInfo)
        {
            this._appInfo = appInfo;
        }

        public abstract Task Upload(string filename, UserConfiguration uc, EventHandler<HttpProgressEventArgs> progressHandler, CancellationToken cancelToken);

    }
}
