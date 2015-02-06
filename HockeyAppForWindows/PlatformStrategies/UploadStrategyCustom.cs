using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using HockeyApp.AppLoader.Extensions;
using HockeyApp.AppLoader.Model;

namespace HockeyApp.AppLoader.PlatformStrategies
{
    internal class UploadStrategyCustom:UploadStrategy
    {
        private string _versionId = "0";
        internal UploadStrategyCustom(AppInfo appInfo) : base(appInfo) { }

        private async Task AddVersion(UserConfiguration uc, CancellationToken cancelToken)
        {
            Dictionary<string, string> formParms = new Dictionary<string, string>();
            formParms.Add("bundle_version", _appInfo.Version);
            formParms.Add("notes_type", "1");
            formParms.Add("status", _appInfo.Status.GetValueOrDefault(1) + "");
            formParms.Add("notes", _appInfo.Notes);

            FormUrlEncodedContent paramContent = new FormUrlEncodedContent(formParms);

            HttpClient client = HttpClientFactory.Create();
            client.DefaultRequestHeaders.Add("X-HockeyAppToken", uc.UserToken);
            HttpResponseMessage response = await client.PostAsync(uc.ApiBase + "apps/" + _appInfo.PublicID + "/app_versions/new", paramContent, cancelToken);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                StreamContent sr = response.Content as StreamContent;
                string result = await sr.ReadAsStringAsync();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                dynamic responseInfo = serializer.Deserialize<object>(result);
                _versionId = ((Dictionary<string, object>)responseInfo)["id"].ToString();
            }
            else
            {
                StreamContent sr = response.Content as StreamContent;
                string result = await sr.ReadAsStringAsync();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                dynamic responseInfo = serializer.Deserialize<object>(result);
                Dictionary<string, object> errors = responseInfo["errors"];
                StringBuilder message = new StringBuilder();
                message.AppendLine(response.ReasonPhrase);
                foreach (string key in errors.Keys)
                {
                    object[] values = (object[])errors[key];
                    string val = "";
                    foreach (object obj in values)
                    {
                        val += obj.ToString() + ", ";
                    }
                    if (val.Length > -2) { val = val.Substring(0, val.Length - 2); }
                    message.AppendLine(key + ":" + val);
                }

                throw new Exception(message.ToString());
            }
        }


        public override async Task Upload(string filename, Model.UserConfiguration uc, EventHandler<System.Net.Http.Handlers.HttpProgressEventArgs> progressHandler, System.Threading.CancellationToken cancelToken)
        {
            await this.AddVersion(uc, cancelToken);
            var multipartContent = base.GetInitalizedMultipartFormDataContent();
            
            var values = new Dictionary<string, string>()
            {
                {"notes", _appInfo.Notes + ""},
                {"notes_type", "1"},
                {"status", _appInfo.Status.ToString()},
                {"notify", _appInfo.Notify.ToString()},
                {"release_type", _appInfo.ReleaseType}
            };
            multipartContent.AddStringContents(values);


            var fs = new FileStream(filename, FileMode.Open);
            multipartContent.Add(new StreamContent(fs), "ipa", Path.GetFileName(filename));

            ProgressMessageHandler progress = new ProgressMessageHandler();
            progress.HttpSendProgress += progressHandler;

            HttpClient client = HttpClientFactory.Create(progress);
            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.Add("X-HockeyAppToken", uc.UserToken);
            HttpResponseMessage response = null;

            response = await client.PutAsync(uc.ApiBase + "apps/" + _appInfo.PublicID + "/app_versions/" + this._versionId, multipartContent, cancelToken);

            if (response != null && !response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
            fs.Close();
        }

        public override string UrlToShowAfterUpload
        {
            get { return "apps/" + _appInfo.PublicID + "/app_versions/" + this._versionId; }
        }
        
    }
}
