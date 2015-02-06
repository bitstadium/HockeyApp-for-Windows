using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.AppLoader.Views.Converters
{
    public class BooleanNotConverter:BooleanConverter<bool>
    {
        public BooleanNotConverter()
            : base(false, true)
        { }
    }
}
