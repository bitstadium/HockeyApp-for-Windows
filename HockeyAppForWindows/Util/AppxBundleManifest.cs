using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace HockeyApp.AppLoader.Util
{
    public static class AppxBundleManifest
    {
        public static List<string> GetApplicationEntries(Stream xmlStream)
        {
            XElement root = XElement.Load(XmlReader.Create(xmlStream));
            List<string> retVal = new List<string>();
            var packages = root.Elements(XName.Get("Packages", root.GetDefaultNamespace().NamespaceName));
            if(packages != null){
                foreach(var package in packages.Elements(XName.Get("Package", root.GetDefaultNamespace().NamespaceName))){
                    var t = package.Attribute("Type");
                    if (t != null && t.Value.ToUpper().Equals("APPLICATION"))
                    {
                        var app = package.Attribute("FileName");
                        if (app.Value != null) { retVal.Add(app.Value); }
                    }
                }
            }
            
            return retVal;
        }
    }
}
