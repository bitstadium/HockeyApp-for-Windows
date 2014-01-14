using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp.AppLoader.Model
{
    [DataContract]
    public class AppInfoEnvelope
    {
        public async static Task<AppInfoEnvelope> Load(UserConfiguration uc)
        {
            HttpWebRequest webRequest = HttpWebRequest.CreateHttp(uc.ApiBase + "apps");

            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.UserAgent = "HockeyAppLoader";
            webRequest.Headers.Add("X-HockeyAppToken", uc.UserToken);

            WebResponse response = await webRequest.GetResponseAsync();
            Stream stream = response.GetResponseStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AppInfoEnvelope));

            AppInfoEnvelope envelope = serializer.ReadObject(stream) as AppInfoEnvelope;
            
            List<AppInfo> localAppData = uc.AppInfos;

            foreach (AppInfo app in envelope.Apps)
            {
                var local = localAppData.FirstOrDefault(p => p.Id.Equals(app.Id));
                if (local != null)
                {
                    app.CompleteWithLocalData(local);
                }
            }

            uc.AppInfos = envelope.Apps;

            return envelope;
        }


        public AppInfoEnvelope() {
        }

        [DataMember(Name="apps")]
        public List<AppInfo> Apps { get; private set; }

        public string status { get; set; }


        private static Stream GetTestStream()
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);


            sw.Write("{\"status\":\"success\"}");
            //sw.Write("{\"apps\":[{\"title\":\"MyTestApp\",\"bundle_identifier\":\"MyTestApp\",\"public_identifier\":\"5cc72cbd6f609d80ce33e42bcb88a886\",\"platform\":\"WindowsPhone\",\"release_type\":0,\"custom_release_type\":null,\"role\":3,\"id\":36562,\"owner\":\"AndreasMoosbrugger\"}],\"status\":\"success\"}");
            sw.Flush();
            ms.Position = 0;
            return ms;
        }
    }


    

}
