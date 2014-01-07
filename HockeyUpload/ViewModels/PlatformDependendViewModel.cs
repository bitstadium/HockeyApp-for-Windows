using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.AppLoader.ViewModels
{
    public abstract class PlatformDependendViewModel:ViewModelBase
    {
        private bool _wasChanged = false;
        protected bool WasChanged
        {
            get { return this._wasChanged; }
            set
            {
                this._wasChanged = value;
                NotifyOfPropertyChange(() => this.WasChanged);
            }
        }

        public abstract void Save();
        public virtual bool CanSave { get { return this.WasChanged; } }
        public abstract void Reset();
        public virtual bool CanReset { get { return this.WasChanged; } }
    }
}
