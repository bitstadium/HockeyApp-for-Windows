using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using HockeyApp.AppLoader.Model;
using System.Collections.ObjectModel;
using System.Windows;
using MahApps.Metro.Controls;

namespace HockeyApp.AppLoader.ViewModels
{
    [Export(typeof(MainWindowViewModel))]
    public class MainWindowViewModel:Screen
    {
        private Screen _defaultContent = null;

        public MainWindowViewModel()
        {
            base.DisplayName = "HockeyApp";   
        }

        public void Init(ViewModelBase defaultView)
        {
            this._defaultContent = defaultView;
            this.SetDefaultView();
        }

        private void SetDefaultView()
        {
            this.ActiveContent = _defaultContent;
        }

        private Screen _activeContent = null;
        public Screen ActiveContent
        {
            get { return this._activeContent; }
            set
            {
                this._activeContent = value;
                NotifyOfPropertyChange(() => this.ActiveContent);
            }
        }


        #region Commands

        private bool _isFlyoutOpen = false;
        public bool IsFlyoutOpen
        {
            get
            {
                return this._isFlyoutOpen;
            }
            set
            {
                this._isFlyoutOpen = value;
                NotifyOfPropertyChange(() => this.IsFlyoutOpen);
            }
        }
        private ViewModelBase _activeFlyoutContent = null;
        public ViewModelBase ActiveFlyoutContent
        {
            get
            {
                return this._activeFlyoutContent;
            }
            set
            {
                this._activeFlyoutContent = value;
                NotifyOfPropertyChange(() => this.ActiveFlyoutContent);
            }
        }
        public void ShowFeedbackFlyout(){
            FeedbackViewModel vm = new FeedbackViewModel();
            this.ActiveFlyoutContent = vm;
            this.IsFlyoutOpen = true;
        }

        
        public void ShowAboutFlyout()
        {
            AboutViewModel vm = new AboutViewModel();
            this.ActiveFlyoutContent = vm;
            this.IsFlyoutOpen = true;
        }

        public void ShowGeneralConfigurationFlyout()
        {
            GeneralConfigurationViewModel vm = new GeneralConfigurationViewModel();
            this.ActiveFlyoutContent = vm;
            this.IsFlyoutOpen = true;
        }

        public void ShowAddUserConfigurationFlyout()
        {
            AddUserConfigurationViewModel vm = new AddUserConfigurationViewModel();
            this.ActiveFlyoutContent = vm;
            this.IsFlyoutOpen = true;

            vm.Closed += (a, b) =>
            {
                this.IsFlyoutOpen = false;
                if (vm.NewUserConfiguration != null)
                {
                    ApplicationsViewModel appVM = this.ActiveContent as ApplicationsViewModel;
                    if (appVM != null)
                    {
                        appVM = new ApplicationsViewModel(vm.NewUserConfiguration);
                        this.ActiveContent = appVM;
                    }
                }
            };
        }

        
        #endregion

        #region appearance
        
        private double _preferredWidth = 100;
        public double PreferredWidth{get{return this._preferredWidth;}
            set{this._preferredWidth = value;
            NotifyOfPropertyChange(()=>this.PreferredWidth);}
        }
        private double _preferrendHeight = 100;
        public double PreferredHeight
        {
            get
            {
                return this._preferrendHeight;
            }
            set
            {
                this._preferrendHeight = value;
                NotifyOfPropertyChange(() => this._preferrendHeight);
            }
        }
        #endregion
    }
}
