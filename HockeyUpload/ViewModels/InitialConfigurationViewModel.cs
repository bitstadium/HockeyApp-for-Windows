using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.AppLoader.ViewModels
{
    public class InitialConfigurationViewModel:ViewModelBase
    {
        public InitialConfigurationViewModel()
        {
            this.NewConfiguration = new UserConfigurationViewModel(null);
            this.NewConfiguration.IsDefaultConfiguration = true;
            this.NewConfiguration.Closed += (a, b) =>
            {
                this.Close();
            };
        }

        public UserConfigurationViewModel NewConfiguration
        {
            private set;
            get;
        }

    }
}
