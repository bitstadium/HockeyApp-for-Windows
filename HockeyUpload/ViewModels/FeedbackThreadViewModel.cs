using Caliburn.Micro;
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
using MahApps.Metro.Controls.Dialogs;
namespace HockeyApp.AppLoader.ViewModels
{
    public class FeedbackThreadViewModel:Screen
    {
        
        public FeedbackThreadViewModel(IFeedbackThread newFeedbackThread)
        {
            this.FeedbackThread = newFeedbackThread;
            this.FeedbackMessages = new ObservableCollection<FeedbackMessageViewModel>();
            this.Add_NewFeedbackMessage();
            
        }

        private void Add_NewFeedbackMessage()
        {
            NewFeedbackMessage newMsg = new NewFeedbackMessage(this);
            this.FeedbackMessages.Add(newMsg);
            newMsg.PropertyChanged += (a, b) => {
                if (b.PropertyName.Equals("Subject"))
                {
                    this.NotifyOfPropertyChange(() => this.Subject);
                }else if (b.PropertyName.Equals("IsNewThread")){
                    this.NotifyOfPropertyChange(() => this.IsNewThread);
                }
            };
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
            Exception exThrown = null;
            ProgressDialogController pdc = await wm.ShowProgressAsync("Please wait...", "Sending feedback");
            try
            {
                this.FeedbackThread = await HockeyClientWPF.Instance.OpenFeedbackThreadAsync(token);
                this.FeedbackMessages.Clear();

                if (this.FeedbackThread != null)
                {
                    this.FeedbackThread.Messages.ForEach(p => FeedbackMessages.Add(new FeedbackMessageViewModel(p)));
                    Add_NewFeedbackMessage();
                }
                else
                {
                    FeedbackToken.DeleteToken(token);
                    raiseDeletedOnServer();
                }
            }
            catch (Exception ex)
            {
                exThrown = ex;
            }
            await pdc.CloseAsync();
            if (exThrown != null)
            {
                await wm.ShowSimpleMessageAsync("Error", "An error occurred while loading feedbacks:\n" + exThrown.Message);
            }
            return this.FeedbackThread != null;
        }
        
        #region props
        public IFeedbackThread FeedbackThread { get; set; }
        public ObservableCollection<FeedbackMessageViewModel> FeedbackMessages { get; private set; }
        public virtual string Subject
        {
            get{ return this.FeedbackMessages != null && this.FeedbackMessages.Count > 0 ? this.FeedbackMessages.First().Subject : ""; }
            set
            {
                this.FeedbackMessages[0].Subject = value;
            }
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
            this.Gravatar = await GravatarHelper.LoadGravatar(hash);
            //this.Gravatar = GravatarHelper.DefaultGravatar;
        }

        #region props
        public virtual string MessageFrom { get { return this._message.Name; } }
        public string CreatedAndVia { get { return "Posted on " + this._message.Created.ToString("dd MMMM yyyy, HH:mm") + " via " + this._message.ViaAsString; } }
        public string Via { get { return "via " + this._message.ViaAsString; } }
        public string Created { get { return this._message.Created.ToString("dd MMMM yyyy, HH:mm"); } }

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
                this.Subject = null;
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
            Exception exThrown = null;
            ProgressDialogController pdc = await wm.ShowProgressAsync("Submitting...", "We are submitting your feedback...");
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
                exThrown = ex;   
            }
            await pdc.CloseAsync();
            if (exThrown != null)
            {
                await wm.ShowSimpleMessageAsync("Error", "An error ocurred while submitting your feedback:\n" + exThrown.Message);
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
