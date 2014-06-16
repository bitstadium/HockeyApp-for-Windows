using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Caliburn.Micro;

namespace HockeyApp.AppLoader.Model
{
    [DataContract]
    public class UserConfiguration
    {
        public static UserConfiguration CreateNew(string configurationName){
            ConfigurationStore config = IoC.Get<ConfigurationStore>();
            if (config.UserConfigurations.Any(p=>p.ConfigurationName.Equals(configurationName))){
                throw new Exception("Configuration name already in use!");
            }
            UserConfiguration newConfig = new UserConfiguration(configurationName);
            newConfig.ConfigurationId = Guid.NewGuid();
            config.UserConfigurations.Add(newConfig);
            return newConfig;
        }

        private ConfigurationStore _configuration = null;
        private ConfigurationStore configuration
        {
            get
            {
                if (this._configuration == null)
                {
                    this._configuration = IoC.Get<ConfigurationStore>();
                }
                return this._configuration;
            }
        }
        
        public UserConfiguration() {
        }

        private UserConfiguration(string configurationName) {
            this.ConfigurationName = configurationName;
        }

        [DataMember]
        public Guid ConfigurationId{get;private set;}

        [DataMember]
        public string ConfigurationName { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserToken { get; set; }
        [DataMember]
        private List<AppInfo> _appInfos = new List<AppInfo>();
        public List<AppInfo> AppInfos { get { return this._appInfos; } set { this._appInfos = value; } }

        [DataMember]
        public string ApiBase { get; set; }

        [DataMember]
        public string GravatarHash { get; set; }

        [DataMember]
        public bool IsInvalid { get; set; }


        public bool IsDefault
        {
            get {
                
                return this.configuration.DefaultUserConfiguration != null 
                && this.configuration.DefaultUserConfiguration.Equals(this); }
        }


        public void Delete()
        {
            this.configuration.UserConfigurations.Remove(this);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            //changing defaultconfig from configname to guid, because config name can be changed from now on
            if (this.ConfigurationId == Guid.Empty) { this.ConfigurationId = Guid.NewGuid(); }
        }
    }
}
