using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;
using HockeyApp.AppLoader.Model;

namespace HockeyApp.AppLoader.PlatformStrategies
{
    internal class UploadStrategyAndroid:UploadStrategy
    {
        internal UploadStrategyAndroid(AppInfo appInfo):base(appInfo){ }

        public async override Task Upload(string filename, Model.UserConfiguration uc, EventHandler<HttpProgressEventArgs> progressHandler, CancellationToken cancelToken)
        {
            var multipartContent = base.GetInitalizedMultipartFormDataContent();

            var values = new Dictionary<string, string>
            {
                {"notes", _appInfo.Notes},
                {"notes_type", "0"},
                {"status", _appInfo.Status.ToString()},
                {"notify", _appInfo.Notify.ToString()},
                {"release_type", _appInfo.ReleaseType},
                {"mandatory", _appInfo.Mandatory.ToString()}
            };

            multipartContent.AddStringContents(values);

            FileStream fs = new FileStream(filename, FileMode.Open);
            HttpContent fileContent = new StreamContent(fs);
            multipartContent.Add(fileContent, "ipa", Path.GetFileName(filename));

            ProgressMessageHandler progress = new ProgressMessageHandler();
            progress.HttpSendProgress += progressHandler;

            HttpClient client = HttpClientFactory.Create(progress);
            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.Add("X-HockeyAppToken", uc.UserToken);
            HttpResponseMessage response = null;

            response = await client.PostAsync(uc.ApiBase + "apps/" + _appInfo.PublicID + "/app_versions/", multipartContent, cancelToken);

            if (response != null && !response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
            fs.Close();
        }

        public override string UrlToShowAfterUpload
        {
            get { return "apps/" + this._appInfo.PublicID; }
        }
    }
}
