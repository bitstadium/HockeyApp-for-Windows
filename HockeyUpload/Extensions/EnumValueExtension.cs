using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace HockeyApp.AppLoader.Extensions
{
    [MarkupExtensionReturnType(typeof(IEnumerable))]

    public class EnumValuesExtension : MarkupExtension
    {
        public EnumValuesExtension()
        {
        }

        public EnumValuesExtension(Type enumType)
        {
            this.EnumType = enumType;
        }

        [ConstructorArgument("enumType")]
        public Type EnumType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {

            if (this.EnumType == null)
            {
                throw new ArgumentException("The enum type is not set");
            }
            List<Enum> list = Enum.GetValues(this.EnumType).OfType<Enum>().ToList();
            return list.OrderBy(p => p.GetOrder());
        }

    }
}
