using Caliburn.Micro;
using HockeyApp.AppLoader.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;
using MahApps.Metro.Controls.Dialogs;

namespace HockeyApp.AppLoader.ViewModels
{
    public class FeedbackViewModel:ViewModelBase
    {
        public FeedbackViewModel()
        {
            base.DisplayName = "Feedback";
            this.FeedbackThreadList = new ObservableCollection<FeedbackThreadViewModel>();
            loadFeedbackThreads();
        }

        private async void loadFeedbackThreads()
        {
            
            foreach (string token in FeedbackToken.Get())
            {
                FeedbackThreadViewModel fbThread = new FeedbackThreadViewModel(null);
                if (await fbThread.LoadThread(token))
                {
                    fbThread.DeletedOnServer += async delegate(object sender, EventArgs args)
                    {
                        IWindowManager wm = IoC.Get<IWindowManager>();
                        await wm.ShowSimpleMessageAsync("Deleted", "Feedback-Thread was deleted on the server!");
                        this.FeedbackThreadList.Remove(fbThread);
                    };
                    FeedbackThreadList.Add(fbThread);
                }
            }

            if (this.FeedbackThreadList.Count == 0)
            {
                this.NewThread();
            }

            //this.FeedbackThreadList.Add(new AddFeedbackThreadViewModel());

            this.SelectedFeedbackThread = this.FeedbackThreadList.FirstOrDefault();
            
        }

        #region props
        
        public ObservableCollection<FeedbackThreadViewModel> FeedbackThreadList { get; private set; }

        private FeedbackThreadViewModel _selectedFeedbackThread = null;
        //private bool _addNewThreadRequest = false;
        public FeedbackThreadViewModel SelectedFeedbackThread
        {
            get { return this._selectedFeedbackThread; }
            set
            {
            
                this._selectedFeedbackThread = value;
                if (this._selectedFeedbackThread != null)
                {
                    this._selectedFeedbackThread.PropertyChanged += (a, b) =>
                    {
                        if (b.PropertyName.Equals("IsNewThread"))
                        {
                            this.NotifyOfPropertyChange(() => this.CanCloseThread);
                            this.NotifyOfPropertyChange(() => this.CanRefreshThread);
                        }
                    };
                }
                NotifyOfPropertyChange(() => SelectedFeedbackThread);
                NotifyOfPropertyChange(() => this.CanCloseThread);
                NotifyOfPropertyChange(() => this.CanRefreshThread);
            }
        }

        #endregion


        #region Commands
        public void NewThread()
        {
            FeedbackThreadViewModel newThread = new FeedbackThreadViewModel(HockeyApp.HockeyClientWPF.Instance.CreateFeedbackThread());
            this.FeedbackThreadList.Add(newThread);
            this.SelectedFeedbackThread = newThread;
        }

        public async void CloseThread()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            MetroDialogSettings settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Close",
                NegativeButtonText = "Cancel"
            };

            if (await wm.ShowMessageAsync("Close thread?", "This thread will be closed and can not be reopened from the app. ",MessageDialogStyle.AffirmativeAndNegative, settings) 
                == MessageDialogResult.Affirmative)
            {
                FeedbackToken.DeleteToken(this.SelectedFeedbackThread.FeedbackThread.Token);
                int pos = this.FeedbackThreadList.IndexOf(this.SelectedFeedbackThread);
                this.FeedbackThreadList.Remove(this.SelectedFeedbackThread);
                if (this.FeedbackThreadList.Count >= pos + 1)
                {
                    this.SelectedFeedbackThread = this.FeedbackThreadList[pos];
                }
                else if (this.FeedbackThreadList.Count > 0)
                {
                    this.SelectedFeedbackThread = this.FeedbackThreadList[this.FeedbackThreadList.Count - 1];
                }
                else {
                    this.NewThread();
                }
            }
        }

        public bool CanCloseThread { get { return this.SelectedFeedbackThread != null && !this.SelectedFeedbackThread.IsNewThread; } }

        public void RefreshThread()
        {
            this.SelectedFeedbackThread.RefreshFeedbackThread();
        }

        public bool CanRefreshThread { get { return this.SelectedFeedbackThread != null && !this.SelectedFeedbackThread.IsNewThread; } }
        #endregion
    }

}
