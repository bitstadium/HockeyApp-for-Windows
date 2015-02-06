using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HockeyApp.AppLoader.Util
{
    public class GravatarHelper
    {
        private static Dictionary<string, BitmapImage> _gravatarCache = new Dictionary<string, BitmapImage>();

        public static string CreateHash(string input){
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static async Task<BitmapImage> LoadGravatar(string hash)
        {
            if (hash == null) { hash = ""; }
            
            if (_gravatarCache.ContainsKey(hash))
            {
                return _gravatarCache[hash];
            }
            
            string url = "http://s.gravatar.com/avatar/" + hash;

            BitmapImage bi;


            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                StreamContent sc = response.Content as StreamContent;
                bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                //Debug:
                //MemoryStream ms = new MemoryStream(await sc.ReadAsByteArrayAsync());
                //bi.StreamSource = ms;

                bi.StreamSource = await sc.ReadAsStreamAsync();
                bi.EndInit();
                bi.Freeze();

                //because of async loading of all gravatar infos...
                Monitor.Enter(_gravatarCache);
                if (!_gravatarCache.ContainsKey(hash))
                {
                    _gravatarCache.Add(hash, bi);
                }
                Monitor.Exit(_gravatarCache);
            }
            else
            {
                bi = GravatarHelper.DefaultGravatar;
            }

            return bi;
        }

        private static BitmapImage _defaultGravatar = null;
        public static BitmapImage DefaultGravatar
        {
            get
            {
                if (_defaultGravatar == null)
                {
                    Uri uri = new Uri("pack://application:,,,/Resources/DefaultGravatar.jpg");
                    _defaultGravatar = new BitmapImage(uri);
                    _defaultGravatar.Freeze();
                }
                return _defaultGravatar;
            }
        }

    
        //Debug
        public static void SaveToFile(MemoryStream ms)
        {
            
            Guid photoID = System.Guid.NewGuid();
            String photolocation = photoID.ToString() + ".jpg";  //file name 


            
            using (var filestream = new FileStream("c:\\tmp\\photos\\" + photolocation, FileMode.Create))
            {
                ms.WriteTo(filestream);
            }
        }
    }
}
