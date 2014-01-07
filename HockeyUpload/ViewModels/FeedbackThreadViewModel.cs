﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Util;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HockeyApp.AppLoader.Extensions;
namespace HockeyApp.AppLoader.ViewModels
{
    public class FeedbackThreadViewModel:Screen
    {
        
        public FeedbackThreadViewModel(IFeedbackThread newFeedbackThread)
        {
            this.FeedbackThread = newFeedbackThread;
            this.FeedbackMessages = new ObservableCollection<FeedbackMessageViewModel>();
            this.FeedbackMessages.Add(new NewFeedbackMessage(this));
            
        }

        public event EventHandler DeletedOnServer;
        private void raiseDeletedOnServer()
        {
            if(this.DeletedOnServer != null){
                EventArgs args = new EventArgs();
                DeletedOnServer(this, args);
            }
        }

        public async Task<bool> LoadThread(string token)
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            wm.ShowBusyView("Loading Feedbacks...");
            try
            {
                this.FeedbackThread = await HockeyClientWPF.Instance.OpenFeedbackThreadAsync(token);
                this.FeedbackMessages.Clear();

                if (this.FeedbackThread != null)
                {
                    this.FeedbackThread.Messages.ForEach(p => FeedbackMessages.Add(new FeedbackMessageViewModel(p)));
                    this.FeedbackMessages.Add(new NewFeedbackMessage(this));
                }
                else
                {
                    FeedbackToken.DeleteToken(token);
                    raiseDeletedOnServer();
                }
            }
            catch (Exception ex)
            {
                wm.ShowBusyView("An error ocurred:\n" + ex.Message);
            }
            finally
            {
                wm.HideBusyView();
            }
            return this.FeedbackThread != null;
        }
        
        #region props
        public IFeedbackThread FeedbackThread { get; set; }
        public ObservableCollection<FeedbackMessageViewModel> FeedbackMessages { get; private set; }
        public virtual object Subject
        {
            get { return this.FeedbackMessages != null && this.FeedbackMessages.Count > 0 ? this.FeedbackMessages.First().Subject : ""; }
        }
        public bool IsNewThread { get { return this.FeedbackThread != null ? this.FeedbackThread.IsNewThread : true; } }

        #endregion

        #region Commands
       
        public async void RefreshFeedbackThread()
        {
            
            await this.LoadThread(this.FeedbackThread.Token);
        }

        public bool CanRefresh { get { return this.FeedbackThread != null && !this.FeedbackThread.IsNewThread; } }
        
        #endregion

    }

    public class FeedbackMessageViewModel:Screen
    {
        private IFeedbackMessage _message;
        public FeedbackMessageViewModel(IFeedbackMessage message)
        {
            this._message = message;
            if (this._message != null && !String.IsNullOrWhiteSpace(this._message.GravatarHash))
            {
                Task t = this.LoadGravatar(this._message.GravatarHash);
            }
            else
            {
                this.Gravatar = GravatarHelper.DefaultGravatar;
            }
        }

        private ImageSource _gravatar = null;
        public ImageSource Gravatar
        {
            get
            {
                return this._gravatar;
            }
            protected set
            {
                this._gravatar = value;
                NotifyOfPropertyChange(() => this.Gravatar);
            }
        }

        protected async Task LoadGravatar(string hash)
        {
            this.Gravatar = await GravatarHelper.LoadAvatar(hash);
            //this.Gravatar = GravatarHelper.DefaultGravatar;
        }

        #region props
        public virtual string MessageFrom { get { return this._message.Name; } }
        public string CreatedAndVia { get { return "Posted on " + this._message.Created.ToString("dd MMMM yyyy, HH:mm") + " via " + this._message.ViaAsString; } }
        public virtual string Message { get { return this._message.CleanText; } set { } }
        public virtual string Subject { get { return this._message.Subject; } set { } }
        public virtual string EMail{get{return this._message.Email;}set{}}
        public virtual string Username { get { return this._message.Name; } set { } }
        #endregion
    }

    public class NewFeedbackMessage : FeedbackMessageViewModel
    {
        private FeedbackThreadViewModel _fbThreadVM = null;
        public NewFeedbackMessage(FeedbackThreadViewModel fbThreadVM):base(null){
            this._fbThreadVM = fbThreadVM;

            this.Username = HockeyApp.AppLoader.Properties.Settings.Default.LastFeedbackUserName;
            this.EMail = HockeyApp.AppLoader.Properties.Settings.Default.LastFeedbackUserEMail;

            if (this.IsNewThread)
            {
                this.Subject = "New Subject";
            }
            else
            {
                this.Subject = _fbThreadVM.FeedbackMessages.Last().Subject;
            }
        }

        public override string MessageFrom { get { return HockeyApp.HockeyClient.Instance.UserID; } }

        private string _messageText = "";
        public override string Message
        {
            get { return this._messageText; }
            set
            {
                this._messageText = value;
                NotifyOfPropertyChange(() => this.Message);
                NotifyOfPropertyChange(() => this.CanSubmit);
            }
        }
        private string _subject = "";
        public override string Subject
        {
            get { return this._subject; }
            set
            {
                this._subject = value;
                NotifyOfPropertyChange(() => this.Subject);
                NotifyOfPropertyChange(() => this.CanSubmit);
            }
        }

        private string _eMail = "";
        public override string EMail
        {
            get
            {
                return this._eMail;
            }
            set
            {
                this._eMail = value;    
                Task t = base.LoadGravatar(GravatarHelper.CreateHash(this.EMail));
            }
        }

        private string _username = "";
        public override string Username
        {
            get { return this._username; }
            set
            {
                this._username = value;
            }
        }

        public bool IsNewThread { get { return this._fbThreadVM.IsNewThread; } }
        public bool IsNotNewThread { get { return !IsNewThread; } }

        public async void Submit(){
            bool wasNewThread = this._fbThreadVM.IsNewThread;
            IFeedbackThread fbThread = this._fbThreadVM.FeedbackThread;
            IWindowManager wm = IoC.Get<IWindowManager>();
            wm.ShowBusyView("Submitting...");
            try
            {
                IFeedbackMessage msg = await fbThread.PostFeedbackMessageAsync(this.Message, this.EMail, this.Subject, this.Username);
                HockeyApp.AppLoader.Properties.Settings.Default.LastFeedbackUserName = this.Username;
                HockeyApp.AppLoader.Properties.Settings.Default.LastFeedbackUserEMail = this.EMail;
                HockeyApp.AppLoader.Properties.Settings.Default.Save();

                if (msg != null)
                {
                    if (wasNewThread)
                    {
                        FeedbackToken.AddToken(fbThread.Token);
                    }
                    this._fbThreadVM.FeedbackMessages.Insert(this._fbThreadVM.FeedbackMessages.Count - 1, new FeedbackMessageViewModel(msg));
                    this._fbThreadVM.NotifyOfPropertyChange(() => this._fbThreadVM.Subject);
                    this.NotifyOfPropertyChange(() => this.IsNewThread);
                    this.Message = "";
                }
            }
            catch (Exception ex)
            {
                wm.ShowMetroMessageBox("An error ocurred:\n" + ex.Message);
            }
            finally
            {
                wm.HideBusyView();
            }
            
        }

        public bool CanSubmit
        {
            get { return !String.IsNullOrWhiteSpace(this.Message) && !String.IsNullOrWhiteSpace(this.Subject); }
        }

        public void Cancel()
        {
            this.EMail = "";
            this.Username = "";
            this.Message = "";
            if (this.IsNewThread)
            {
                this.Subject = "";
            }
            NotifyOfPropertyChange(""); 
        }
    }
}
