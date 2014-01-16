using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.PlatformStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;
using System.Threading;

namespace HockeyApp.AppLoader.ViewModels
{
    public class UploadDialogViewModel:ViewModelBase
    {
        private UserConfiguration _activeUserConfiguration = null;
        public UploadDialogViewModel(List<AppInfo> list, UserConfiguration uc)
        {
            this.ValidApps = new List<AppDialogViewModel>();

            foreach (AppInfo app in list)
            {
                AppDialogViewModel newVM = new AppDialogViewModel(app);
                newVM.PropertyChanged += (sender, p) =>
                {
                    if (p.PropertyName.Equals("CanUpload")) { this.NotifyOfPropertyChange(() => this.CanUpload); }
                };
                this.ValidApps.Add(newVM);
            }
            if (this.ValidApps.Count == 1)
            {
                this.SelectedApp = this.ValidApps[0];
            }
            else
            {
                this.SelectedApp = null;
            }
            this._activeUserConfiguration = uc;    
        }


        public List<AppDialogViewModel> ValidApps { get; set; }

        private AppDialogViewModel _selectedApp=null;
        public AppDialogViewModel SelectedApp
        {
            get { return this._selectedApp; }
            set
            {
                this._selectedApp = value;
                NotifyOfPropertyChange(() => this.SelectedApp);
                NotifyOfPropertyChange(() => this.CanUpload);
            }
        }

        public bool IsUploading
        {
            get { return this._cancelTokenSource != null; }
        }

        #region Commands
        public void Cancel()
        {
            if (this._cancelTokenSource != null && !this._cancelTokenSource.IsCancellationRequested)
            {
                this._cancelTokenSource.Cancel();
            }
            else
            {
                base.Close();
            }
        }
        public bool CanCancel
        {
            get
            {
                return !this.IsUploading || !this._cancelTokenSource.IsCancellationRequested;
            }
        }

        private CancellationTokenSource _cancelTokenSource = null;
        public CancellationTokenSource CancelTokenSource
        {
            get { return this._cancelTokenSource; }
            set
            {
                this._cancelTokenSource = value;
                NotifyOfPropertyChange(() => this.IsUploading);
                NotifyOfPropertyChange(() => this.CanUpload);
            }
        }
        public async Task Upload()
        {
            try
            {
                this.CancelTokenSource = new CancellationTokenSource();
                
                this.SelectedApp.PrepareUpload();
                AppInfo appInfo = this.SelectedApp.AppInfo;

                UploadStrategy uploadStrategy = UploadStrategy.GetStrategy(appInfo);

                await uploadStrategy.Upload(this.SelectedApp.FileToUpload, this._activeUserConfiguration, ReportProgress, _cancelTokenSource.Token);

                string publicWebSite = this._activeUserConfiguration.ApiBase;
                publicWebSite = publicWebSite.Substring(0, publicWebSite.Length - 6);
                System.Diagnostics.Process.Start(publicWebSite + "apps/" + appInfo.PublicID);
                
                this.Close();
            }
            catch (TaskCanceledException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                IWindowManager wm = IoC.Get<IWindowManager>();
                Task t = wm.ShowSimpleMessageAsync("Error", "Error while creating a new version:\n" + ex.Message);
            }
            finally
            {
                this.CancelTokenSource = null;
            }
        }

        public bool CanUpload { get { return this.SelectedApp != null && this.SelectedApp.CanUpload && !this.IsUploading; } }

        public double UploadPercentage { get; set; }
        private void ReportProgress(object sender, HttpProgressEventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher.CheckAccess())
            {
                double x, y;
                x = e.BytesTransferred;
                y = e.TotalBytes.GetValueOrDefault(1);
                double percentage = x / y * 100;
                this.UploadPercentage = percentage; //Math.Round(percentage,0);
            }
            NotifyOfPropertyChange(() => this.UploadPercentage);
        }

        #endregion





    }

}
