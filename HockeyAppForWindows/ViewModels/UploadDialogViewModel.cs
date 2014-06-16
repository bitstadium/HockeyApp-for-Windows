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
using MahApps.Metro.Controls.Dialogs;

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


        #region Commands
       
        public async Task Upload()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            ProgressDialogController pdc = await wm.ShowProgressAsync("Please wait...", "Uploading your package to " + this.SelectedApp.Title);
            pdc.SetCancelable(true);
            Exception exThrown = null;
            try
            {

                this.SelectedApp.PrepareUpload();
                AppInfo appInfo = this.SelectedApp.AppInfo;
                CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
                UploadStrategy uploadStrategy = UploadStrategy.GetStrategy(appInfo);


                await uploadStrategy.Upload(this.SelectedApp.FileToUpload, this._activeUserConfiguration, (a, b) => {
                    SetProgress(a, b, pdc, _cancelTokenSource);
                }, _cancelTokenSource.Token);

                string publicWebSite = this._activeUserConfiguration.ApiBase;
                publicWebSite = publicWebSite.Substring(0, publicWebSite.Length - 6);
                System.Diagnostics.Process.Start(publicWebSite + uploadStrategy.UrlToShowAfterUpload);
                
                this.Close();
            }
            catch (TaskCanceledException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                exThrown = ex;
            }

            await pdc.CloseAsync();
            if (exThrown != null)
            {
                await wm.ShowSimpleMessageAsync("Error", "Error while creating a new version:\n" + exThrown.Message);
            }
            
        }

        public bool CanUpload { get { return this.SelectedApp != null && this.SelectedApp.CanUpload; } }

        private void SetProgress(object sender, HttpProgressEventArgs e, ProgressDialogController pdc, CancellationTokenSource _cancelToken)
        {


            double x, y;
            x = e.BytesTransferred;
            y = e.TotalBytes.GetValueOrDefault(1);
            double percentage = x / y;
            
            pdc.SetProgress(percentage);
            if (pdc.IsCanceled) { _cancelToken.Cancel(); }

        }

        #endregion





    }

}
