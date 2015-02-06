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
            if (ConfigurationStore.Instance.UserConfigurations.Count == 0)
            {
                vm.IsDefault = true;
            }
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

        public void ShowEditUserConfigurationFlyout(UserConfigurationViewModel uc)
        {
            var vm = new EditUserConfigurationViewModel(uc);
            this.ActiveFlyoutContent = vm;
            vm.Closed += (a, b) => {
                this.IsFlyoutOpen = false;
                var appVM = this.ActiveContent as ApplicationsViewModel;
                if (appVM != null)
                {
                    appVM.RefreshConfiguration();
                }
            };
            this.IsFlyoutOpen = true;

        }

        #endregion

        #region appearance
        
        private double _preferredWidth = 1200;
        public double PreferredWidth{get{return this._preferredWidth;}
            set{this._preferredWidth = value;
            NotifyOfPropertyChange(()=>this.PreferredWidth);}
        }
        private double _preferrendHeight = 800;
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
        private double _minWidth = 0;
        public double MinWidth
        {
            get { return this._minWidth; }
            set
            {
                this._minWidth = value;
                NotifyOfPropertyChange(() => this.MinWidth);
            }
        }

        private double _minHeight = 0;
        public double MinHeight
        {
            get { return this._minHeight; }
            set
            {
                this._minHeight = value;
                NotifyOfPropertyChange(() => this.MinHeight);
            }
        }

        private double _x = 0;
        public double X
        {
            get { return _x; }
            set
            {
                this._x = value;
                NotifyOfPropertyChange(() => this.X);
            }
        }
        private double _y = 0;
        public double Y
        {
            get { return _y; }
            set
            {
                this._y = value;
                NotifyOfPropertyChange(() => this.Y);
            }
        }


        private bool _IsDialog = false;
        public bool IsDialog
        {
            get { return _IsDialog; }
            set
            {
                this._IsDialog = value;
                NotifyOfPropertyChange(() => this.IsDialog);
            }
        }


        

        public void AdjustWindow(bool isDialog)
        {
            double height = 0;
            double width = 0;
            
            
            if (isDialog)
            {
                height = 460;
                width = 820;
                
            }
            else
            {
                width = Math.Min(SystemParameters.WorkArea.Width, 1280);
                height = Math.Min(SystemParameters.WorkArea.Height, 690);
            }

            this.PreferredHeight = height;
            this.PreferredWidth = width;
            this.MinHeight = height;
            this.MinWidth = width;
            this.CenterWindow();
        }


        private void CenterWindow()
        {
            double x, y;

            //x = System.Windows.SystemParameters.MaximizedPrimaryScreenWidth / 2 - this.PreferredWidth / 2;
            //y = System.Windows.SystemParameters.MaximizedPrimaryScreenHeight / 2 - this.PreferredHeight / 2;

            x = System.Windows.SystemParameters.WorkArea.Width / 2 - this.PreferredWidth / 2;
            y = System.Windows.SystemParameters.WorkArea.Height / 2 - this.PreferredHeight / 2;
            this.X = Math.Max(x, 0);
            this.Y = Math.Max(y, 0);
        }

        #endregion
    }
}
