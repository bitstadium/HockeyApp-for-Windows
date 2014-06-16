using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.AppLoader.Extensions
{
    public static class MultipartFormDataContentExtension
    {
        public static void AddStringContents(this MultipartFormDataContent @this, Dictionary<string, string> values)
        {
            foreach (var keyValue in values)
            {
                @this.Add(new StringContent(keyValue.Value), "\"" + keyValue.Key + "\"");
            }
        }
    }
}
