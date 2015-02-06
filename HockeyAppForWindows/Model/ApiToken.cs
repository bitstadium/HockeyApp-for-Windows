using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.AppLoader.Model
{
    [DataContract]
    public class ApiToken
    {

        [DataMember(Name = "token")]
        public virtual string Token{get;private set;}

        [DataMember(Name = "rights")]
        public string Rights{get;private set;}

        [DataMember(Name = "name")]
        public string Name { get; private set; }

        [DataMember(Name = "app_id")]
        public virtual string App_Id { get; private set; }

        public virtual string NameToDisplay
        {
            get {
                string retVal = "Unknown";
                if (!String.IsNullOrWhiteSpace(this.Name))
                {
                    retVal = this.Name;
                }
                return retVal;
            }
        }
        public virtual string RightsDisplay
        {
            get
            {
                string retVal = "unknown";
                if (this.Rights == "0")
                {
                    retVal = "Full-Access";
                }else if(this.Rights == "1"){
                    retVal = "Upload-Only";
                }else if(this.Rights == "2"){
                    retVal = "Read-Only";
                }
                return retVal;
            }
        }

    }
}
