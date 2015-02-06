using System.Diagnostics;
using System.IO;
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
using HockeyApp.Model;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using Microsoft.Win32;
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

        public bool WasDeletedOnServer { get; private set; }
        public async Task<bool> LoadThread(string token)
        {
            this.FeedbackThread = await HockeyClient.Current.OpenFeedbackThreadAsync(token);
            this.FeedbackMessages.Clear();

            if (this.FeedbackThread != null)
            {
                this.FeedbackThread.Messages.ForEach(p => FeedbackMessages.Add(new FeedbackMessageViewModel(p)));
                Add_NewFeedbackMessage();
            }
            else
            {
                FeedbackToken.DeleteToken(token);
                this.WasDeletedOnServer = true;
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
       
        public async Task<bool> RefreshFeedbackThread()
        {
            
            return await this.LoadThread(this.FeedbackThread.Token);
        }

        public bool CanRefresh { get { return this.FeedbackThread != null && !this.FeedbackThread.IsNewThread; } }
        
        #endregion

    }

    public class FeedbackMessageViewModel:Screen
    {
        private IFeedbackMessage _message;

        public FeedbackMessageViewModel(IFeedbackMessage message)
        {
            this.Attachments = new ObservableCollection<FeedbackAttachmentViewModel>();
            this._message = message;
            if (message != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    this.Attachments.Add(new FeedbackAttachmentViewModel(attachment, this));
                }
            }

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

        public ObservableCollection<FeedbackAttachmentViewModel> Attachments { get; private set; } 

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

        public virtual string UserMessage { get { return this._message.CleanText; } set { } }
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

        public override string MessageFrom { get { return (HockeyApp.HockeyClient.Current as HockeyClient).UserID; } }

        private string _messageText = "";
        public override string UserMessage
        {
            get { return this._messageText; }
            set
            {
                this._messageText = value;
                NotifyOfPropertyChange(() => this.UserMessage);
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
                IFeedbackMessage msg = await fbThread.PostFeedbackMessageAsync(this.UserMessage, this.EMail, this.Subject, this.Username,this.Attachments.Select(p=>p.Attachment));
                this.Attachments.Clear();
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
                    this.UserMessage = "";
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
            get { return !String.IsNullOrWhiteSpace(this.UserMessage) && !String.IsNullOrWhiteSpace(this.Subject); }
        }

        public void Cancel()
        {
            this.EMail = "";
            this.Username = "";
            this.UserMessage = "";
            if (this.IsNewThread)
            {
                this.Subject = "";
            }
            NotifyOfPropertyChange(""); 
        }

        public void AddAttachment()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                this.AddAttachmentBasic(ofd.FileName);
            }
        }

        private void AddAttachmentBasic(string filename)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            var filenameOnly = Path.GetFileName(filename);
            var fa = new FeedbackAttachment(filenameOnly, bytes, "");
            this.Attachments.Add(new FeedbackAttachmentViewModel(fa, this));
        }

        
        #region DnD

        private bool _isDropAllowed = false;
        public Boolean IsDropAllowed
        {
            get { return this._isDropAllowed; }
            set
            {
                this._isDropAllowed = value;
                NotifyOfPropertyChange(() => this.IsDropAllowed);
            }
        }

        private bool _dnDSourceIsOK = false;
        public bool DnDSourceIsOK
        {
            get
            {
                return this._dnDSourceIsOK;
            }
            set
            {
                this._dnDSourceIsOK = value;
                NotifyOfPropertyChange(() => this.DnDSourceIsOK);
            }
        }

        private bool _isInDragDropAction = false;
        public bool IsInDragDropAction
        {
            get { return this._isInDragDropAction; }
            set
            {
                this._isInDragDropAction = value;
                NotifyOfPropertyChange(() => this.IsInDragDropAction);
            }
        }

        private bool AcceptDrop(System.Windows.IDataObject dataObject)
        {
            bool retVal = false;
            if (dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                var filenames = dataObject.GetData(System.Windows.DataFormats.FileDrop) as string[];
                if (filenames != null && filenames.Length == 1)
                {
                    string filename = filenames[0];
                    if (File.Exists(filename))
                    {
                        DropText = "Drop anywhere to upload";
                        DnDSourceIsOK = true;
                        retVal = true;
                    }
                    else if (Directory.Exists(filename))
                    {
                        DnDSourceIsOK = false;
                        DropText = "Directories are not supported";
                    }
                }
                else
                {
                    DnDSourceIsOK = false;
                    DropText = "Multiple items are not supported";
                }
            }
            else
            {
                DnDSourceIsOK = false;
                DropText = "No data found?";
            }
            return retVal;
        }

        public void OnDragEnterAndOver(System.Windows.DragEventArgs e)
        {
            this.IsInDragDropAction = true;
            e.Effects = AcceptDrop(e.Data) ? System.Windows.DragDropEffects.Move : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        public void OnDragLeave(System.Windows.DragEventArgs e)
        {
            this.IsInDragDropAction = false;
            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        public void OnDrop(System.Windows.DragEventArgs e)
        {
            this.IsInDragDropAction = false;
            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
            
            if (AcceptDrop(e.Data))
            {
                string filename = (e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[])[0];
                this.AddAttachmentBasic(filename);
            }
        }

        private string _dropText = "";
        public string DropText
        {
            get
            {
                return this._dropText;
            }
            set
            {
                this._dropText = value;
                NotifyOfPropertyChange(() => this.DropText);
            }
        }


        #endregion
    }

    public class FeedbackAttachmentViewModel
    {
        private FeedbackMessageViewModel _msgVM;
        private string _localFileName = "";
        public FeedbackAttachmentViewModel(IFeedbackAttachment attachment, FeedbackMessageViewModel msgVM)
        {
            this.Attachment = attachment;
            this._msgVM = msgVM;
        }
        public string Name {
            get { return this.Attachment.FileName; }
        }

        public IFeedbackAttachment Attachment { get; private set; }
    
        public async void OpenAttachment()
        {
            IWindowManager wm = IoC.Get<IWindowManager>();
            if(string.IsNullOrWhiteSpace(this._localFileName))
            {
                ProgressDialogController pdc = null;
                if (this.Attachment.DataBytes == null)
                {
                    pdc = await wm.ShowProgressAsync("Please wait", "Loading data from server", false);
                    if (!await this.Attachment.LoadAttachmentFromServer())
                    {
                        await pdc.CloseAsync();
                        return;
                    }
                }
                
                var tmpFileName = Path.GetTempFileName();
                var tmpFileWoExt = Path.GetFileNameWithoutExtension(tmpFileName);
                var attachmentExt = Path.GetExtension(this.Attachment.FileName);
                var pathOfTmpFile = Path.GetDirectoryName(tmpFileName);
                var newFileName = Path.Combine(pathOfTmpFile, tmpFileWoExt +  attachmentExt);
                File.Move(tmpFileName, newFileName);

                FileStream fs = File.OpenWrite(newFileName);
                fs.Write(this.Attachment.DataBytes, 0, this.Attachment.DataBytes.Length);
                fs.Flush();
                fs.Close();

                System.Diagnostics.Process.Start(newFileName);
                if (pdc != null && pdc.IsOpen)
                {
                    await pdc.CloseAsync();
                }
            }
        }

        public void RemoveAttachment()
        {
            this._msgVM.Attachments.Remove(this);
        }
    }
}
