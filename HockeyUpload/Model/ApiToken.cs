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
        public string Token{get;private set;}

        [DataMember(Name = "rights")]
        public string Rights{get;private set;}
    }
}
