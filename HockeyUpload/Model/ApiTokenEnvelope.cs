using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.AppLoader.Model
{
    [DataContract]
    public class ApiTokenEnvelope
    {

        public async static Task<ApiTokenEnvelope> Load(string apiBase, string username, string password)
        {
            
            HttpWebRequest webRequest = HttpWebRequest.CreateHttp(apiBase + "auth_tokens");
            
            webRequest.PreAuthenticate = true;
            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.UserAgent = "HockeyAppLoader";

            String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
            webRequest.Headers.Add("Authorization", "Basic " + encoded);

            WebResponse response = await webRequest.GetResponseAsync();
            Stream stream = response.GetResponseStream();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ApiTokenEnvelope));
            ApiTokenEnvelope envelope = serializer.ReadObject(stream) as ApiTokenEnvelope;

            return envelope;
        }

        public async static Task<ApiToken> CreateNewToken(string apiBase, string username, string password)
        {

            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("rights", "0");
            
            HttpContent formContent = new FormUrlEncodedContent(values);

            HttpClient client = HttpClientFactory.Create();
            client.Timeout = TimeSpan.FromMinutes(10);
            HttpResponseMessage response = null;

            String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);


            response = await client.PostAsync(apiBase + "auth_tokens", formContent);

            if (response != null && !response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }

            Stream stream = await response.Content.ReadAsStreamAsync();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ApiTokenEnvelope));
            ApiTokenEnvelope envelope = serializer.ReadObject(stream) as ApiTokenEnvelope;

            if (!envelope.Status.Equals("success"))
            {
                throw new Exception("Error: " + envelope.Status);
            }

            return envelope.Tokens.FirstOrDefault();
        }

        [DataMember(Name = "tokens")]
        public List<ApiToken> Tokens { get; private set; }

        [DataMember(Name = "key")]
        public string Key{get;private set;}

        [DataMember(Name = "name")]
        public string Name{get;private set;}

        [DataMember(Name = "status")]
        public string Status{get;private set;}
        
    }
}
