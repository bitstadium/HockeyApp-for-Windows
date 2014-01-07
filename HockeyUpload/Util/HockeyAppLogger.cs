using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.AppLoader.Util
{
    public class HockeyAppLogger:ILog
    {
        private static NLog.Logger _logger;
        static HockeyAppLogger(){
            _logger = NLog.LogManager.GetLogger("HockeyApp-Logger");
        }

            
        public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);            
        }

        public void Warn(string format, params object[] args)
        {
            _logger.Warn(format, args);
        }

        public void Error(Exception exception)
        {
            _logger.Error(exception);
        }
    }
}
