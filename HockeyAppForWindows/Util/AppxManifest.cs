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
    public class AppxManifest
    {
        private AppxManifest() { }
        public static AppxManifest Create(Stream manifestStream)
        {
            var root = XElement.Load(XmlReader.Create(manifestStream));
            var retVal = new AppxManifest();
            retVal.Package = new InternalPackage(root);
            return retVal;
        }

        public IPackage Package { get; private set; }

        private class InternalPackage :IPackage
        {
            public InternalPackage(XElement element)
            {
                if (element.Element(XName.Get("Identity", element.GetDefaultNamespace().NamespaceName)) != null)
                {
                    this.Identity = new InternalIdentity(element.Element(XName.Get("Identity", element.GetDefaultNamespace().NamespaceName)));
                }

                this.Properties = new Dictionary<string, string>();
                var xProps = element.Element(XName.Get("Properties", element.GetDefaultNamespace().NamespaceName));
                if (xProps != null)
                {
                    foreach (XElement current in xProps.Elements())
                    {
                        this.Properties.Add(current.Name.LocalName, current.Value);
                    }
                }

            }
            public IIdentity Identity { get; private set; }
            public Dictionary<string, string> Properties { get; private set; }
            
        }

        private class InternalIdentity : IIdentity
        {
            public InternalIdentity(XElement element)
            {
                if (element != null)
                {
                    this.Name = element.Attribute("Name").Value;
                    this.Publisher = element.Attribute("Publisher").Value;
                    this.Version = element.Attribute("Version").Value;
                    this.ProcessorArchitecture = element.Attribute("ProcessorArchitecture").Value;
                }
            }
            public string Name { get; private set; }
            public string Publisher { get; private set; }
            public string Version { get; private set; }
            public string ProcessorArchitecture { get; private set; }

        }
    
    }

    public interface IPackage
    {
        IIdentity Identity { get; }
        Dictionary<string, string> Properties { get; }
    }

    public interface IIdentity
    {
        string Name { get; }
        string ProcessorArchitecture { get; }
        string Publisher { get; }
        string Version { get; }
    }
    
}
