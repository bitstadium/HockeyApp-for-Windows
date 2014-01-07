using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.AppLoader.Util
{
    public static class FeedbackToken
    {
        private static Dictionary<string, string> _tokens = null;

        private static void refreshTokens(){
            _tokens = new Dictionary<string, string>();

            string tmp = HockeyApp.AppLoader.Properties.Settings.Default.ThreadTokens;
            if (tmp != null && tmp.Length > 0)
            {
                string[] arr = tmp.Split('|');
                foreach(string token in arr){
                    _tokens.Add(token,token);
                }
            }
        }

        private static void saveTokens()
        {
            string tmp  = "";
            if (_tokens.Count > 0){
                tmp = _tokens.Keys.Aggregate((a, b) => a + "|" + b);
            }
            
            HockeyApp.AppLoader.Properties.Settings.Default.ThreadTokens = tmp;
            HockeyApp.AppLoader.Properties.Settings.Default.Save();
        }

        public static string[] Get(){
            if (_tokens == null)
            {
                refreshTokens();
            }
            return _tokens.Keys.ToArray<string>();
        }

        public static void AddToken(string token)
        {
            _tokens.Add(token, token);
            saveTokens();
        }

        public static void DeleteToken(string token){
            if (_tokens.ContainsKey(token)){
                _tokens.Remove(token);
            }
            saveTokens();
        }

    }
}
