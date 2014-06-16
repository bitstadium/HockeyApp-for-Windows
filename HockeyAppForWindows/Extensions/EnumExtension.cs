using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace HockeyApp.AppLoader.Extensions
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            object[] attribs = field.GetCustomAttributes(typeof(DisplayAttribute), true);
            if (attribs.Length > 0)
            {
                return ((DisplayAttribute)attribs[0]).Name;
            }
            
            return string.Empty;
        }

        public static int GetOrder(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            object[] attribs = field.GetCustomAttributes(typeof(DisplayAttribute), true);
            if (attribs.Length > 0)
            {
                return ((DisplayAttribute)attribs[0]).Order;
            }

            return -1;
        }

    }
}
