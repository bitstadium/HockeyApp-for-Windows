using Caliburn.Micro;
using HockeyApp.AppLoader.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;

namespace HockeyApp.AppLoader.ViewModels
{
    public class FeedbackViewModel:ViewModelBase
    {
        public FeedbackViewModel()
        {
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
                    fbThread.DeletedOnServer += delegate(object sender, EventArgs args)
                    {
                        IWindowManager wm= IoC.Get<IWindowManager>();
                        wm.ShowMetroMessageBox("Feedback-Thread was deleted on the server!", "Deleted", System.Windows.MessageBoxButton.OK);
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
                //if (this._addNewThreadRequest) { return; }
                /*
                if (value is AddFeedbackThreadViewModel)
                {
                    this._addNewThreadRequest = true;
                    FeedbackThreadViewModel vm = new FeedbackThreadViewModel(null);
                    this.FeedbackThreadList.Insert(this.FeedbackThreadList.IndexOf(value) , vm);
                    this._selectedFeedbackThread = vm;
                    this._addNewThreadRequest = false;
                }
                else
                {*/
                    this._selectedFeedbackThread = value;
                //}
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

        public void CloseThread()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            if (wm.ShowMetroMessageBox("Thread will be closed and cannot be opened later. Continue anyway?", "Close thread", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
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

        public bool CanCloseThread { get { return this.SelectedFeedbackThread != null; } }

        public void RefreshThread()
        {
            this.SelectedFeedbackThread.RefreshFeedbackThread();
        }

        public bool CanRefreshThread { get { return this.SelectedFeedbackThread != null; } }
        #endregion
    }

}
