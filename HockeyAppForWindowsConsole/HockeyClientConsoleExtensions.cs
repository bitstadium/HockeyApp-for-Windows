using HockeyApp;
using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace HockeyAppForWindows.Hoch
{
    public static class HockeyClientConsoleExtensions
    {
        internal static IHockeyClientInternal AsInternal(this IHockeyClient @this)
        {
            return (IHockeyClientInternal)@this;
        }

        #region Configure

        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier)
        {
            @this.AsInternal().AppIdentifier = identifier;
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperConsole();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        

            return (IHockeyClientConfigurable)@this;
        }

        private static Action<UnhandledExceptionEventArgs> customUnhandledExceptionAction;
        

        /// <summary>
        /// This will run after HockeyApp has written the crash-log to disk.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customAction"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Action<UnhandledExceptionEventArgs> customAction)
        {
            customUnhandledExceptionAction = customAction;
            return @this;
        }


        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HockeyClient.Current.AsInternal().HandleException((Exception)e.ExceptionObject);
            if (customUnhandledExceptionAction != null)
            {
                customUnhandledExceptionAction(e);
            }
        }


        #endregion



        #region CrashHandling

        /// <summary>
        /// Send crashes to the HockeyApp server. If crashes are available a messagebox will popoup to ask the user if he wants to send crashes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="sendAutomatically">if true crashes will be sent without asking</param>
        /// <returns></returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this, Boolean sendAutomatically = false)
        {
            @this.AsInternal().CheckForInitialization();

            if (sendAutomatically)
            {
                return await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
            }
            else
            {
                if (await @this.AsInternal().AnyCrashesAvailableAsync())
                {
                    return await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
                }
                return false;
            }
        }

        #endregion

        #region Helper

        private static string _appIdHash = null;
        public static string AppIdHash
        {
            get {
                if (_appIdHash == null)
                {
                    _appIdHash = GetMD5Hash(HockeyClient.Current.AsInternal().AppIdentifier);
                }
                return _appIdHash; }
        }

        internal static string GetMD5Hash(string sourceString)
        {
            if (String.IsNullOrEmpty(sourceString)) { return string.Empty; }
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] sourceBytes = Encoding.Default.GetBytes(sourceString);
            byte[] result = md5.ComputeHash(sourceBytes);
            return System.BitConverter.ToString(result);
        } 

        #endregion
    }
}
 
