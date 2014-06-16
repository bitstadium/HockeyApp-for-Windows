using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HockeyApp.AppLoader.Extensions
{
  
    public class AppInfoPlatformsTypeConverter : TypeConverter
    {
        private static Dictionary<string, AppInfoPlatforms> map = new Dictionary<string,AppInfoPlatforms>();

        static AppInfoPlatformsTypeConverter()
        {
            foreach(AppInfoPlatforms c in Enum.GetValues(typeof(AppInfoPlatforms))){
                    map.Add(c.GetDescription(),c);
            }
        }
                
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;

            if (stringValue != null)
            {
                AppInfoPlatforms operation;
                if (map.TryGetValue(stringValue, out operation))
                {
                    return operation;
                }
                else
                {
                    throw new ArgumentException("Cannot convert '" + stringValue + "' to Operation");
                }
            }
            return null;
        }
    }


}
