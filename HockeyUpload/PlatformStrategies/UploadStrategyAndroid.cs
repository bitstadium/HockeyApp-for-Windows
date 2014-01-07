using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Model;

namespace HockeyApp.AppLoader.PlatformStrategies
{
    internal class UploadStrategyAndroid:UploadStrategy
    {
        internal UploadStrategyAndroid(AppInfo appInfo):base(appInfo){ }

        public async override Task Upload(string filename, Model.UserConfiguration uc, EventHandler<HttpProgressEventArgs> progressHandler, CancellationToken cancelToken)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("notes", _appInfo.Notes);
            values.Add("notes_type", "0");
            values.Add("status", _appInfo.Status.ToString());
            values.Add("notify", _appInfo.Notify.ToString());
            values.Add("release_type", _appInfo.ReleaseType.ToString());
            values.Add("mandatory", _appInfo.Mandatory.ToString());


            FileStream fs = new FileStream(filename, FileMode.Open);

            HttpContent formContent = new FormUrlEncodedContent(values);
            HttpContent fileContent = new StreamContent(fs);


            MultipartFormDataContent multipartContent = new MultipartFormDataContent();
            multipartContent.Add(formContent);
            multipartContent.Add(fileContent, "ipa", Path.GetFileName(filename));


            ProgressMessageHandler progress = new ProgressMessageHandler();
            progress.HttpSendProgress += progressHandler;

            HttpClient client = HttpClientFactory.Create(progress);
            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.Add("X-HockeyAppToken", uc.UserToken);
            HttpResponseMessage response = null;

            response = await client.PostAsync(uc.ApiBase + "apps", multipartContent, cancelToken);

            if (response != null && !response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
            fs.Close();
        }
    }
}
