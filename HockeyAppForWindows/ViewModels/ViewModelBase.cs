using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.AppLoader.ViewModels
{
    public class ViewModelBase:Screen
    {
        public ViewModelBase()
        {
            
        }

        public event EventHandler Closed;
        public void Close()
        {
            base.TryClose();
            if (this.Closed != null)
            {
                Closed(this.Closed, new EventArgs());
            }
        }
    }
}
