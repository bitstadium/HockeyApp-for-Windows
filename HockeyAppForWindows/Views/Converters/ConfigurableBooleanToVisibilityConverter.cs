using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HockeyApp.AppLoader.Views.Converters
{
    public sealed class ConfigurableBooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public ConfigurableBooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed) { }
    }
}
