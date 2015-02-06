using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace HockeyApp.AppLoader.Model
{
    [DataContract]
    public class Error
    {
        [DataMember(Name="errors")]
        public Dictionary<string, List<string>> Errors{get;set;}
    }
}
