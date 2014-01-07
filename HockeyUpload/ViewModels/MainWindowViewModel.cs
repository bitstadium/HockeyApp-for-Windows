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


        #region MessageBox / BusyView
        private int _overlayDependencies;
        public void ShowOverlay()
        {
            _overlayDependencies++;
            NotifyOfPropertyChange(() => IsOverlayVisible);
        }

        public void HideOverlay()
        {
            _overlayDependencies--;
            NotifyOfPropertyChange(() => IsOverlayVisible);
        }

        public bool IsOverlayVisible
        {
            get { return _overlayDependencies > 0; }
        }


        private bool _viewIsBusy = false;
        public bool ViewIsBusy
        {
            get { return _viewIsBusy; }
            set
            {
                _viewIsBusy = value;
                NotifyOfPropertyChange(() => ViewIsBusy);
            }
        }



        private string _busyMessage = "";
        public string BusyMessage
        {
            get { return this._busyMessage; }
            set
            {
                this._busyMessage = value;
                NotifyOfPropertyChange(() => BusyMessage);
            }
        }       
        #endregion

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

        #region Appearance
        private bool _isDialog = false;
        public bool IsDialog
        {
            get { return this._isDialog; }
            set
            {
                this._isDialog = value;

                if (this.IsDialog)
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                    this.WindowWidth = 600;
                    this.WindowHeight = 500;
                }
                else
                {
                    
                    this.WindowWidth = 1024;
                    this.WindowHeight = 768;
                }

                NotifyOfPropertyChange("");
            }
        }

        public double WindowWidth { get; set; }
        public double WindowHeight{get;set;}
        public WindowState WindowState{get;set;}

        #endregion

        #region Commands
        public void OpenSettings(){
            ConfigurationViewModel vm = IoC.Get<ConfigurationViewModel>();
            vm.Closed += delegate(object sender, EventArgs args)
            {
                this.SetDefaultView();
            };
            this.ActiveContent = vm;
        }

        public void OpenFeedbackView()
        {
            FeedbackViewModel vm = new FeedbackViewModel();
            vm.Closed += delegate(object sender, EventArgs args)
            {
                this.SetDefaultView();
            };
            this.ActiveContent = vm;
        }
        #endregion
    }
}
