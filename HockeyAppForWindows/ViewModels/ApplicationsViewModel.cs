using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;

namespace HockeyApp.AppLoader.ViewModels
{
    public class ApplicationsViewModel : ViewModelBase
    {
        public ApplicationsViewModel(UserConfiguration configurationToSelect = null)
        {
            UserConfigurationViewModel toSelect = null;
            
            this.UserConfigurations = new ObservableCollection<UserConfigurationViewModel>();
            this.UserConfigurations.CollectionChanged += (a, b) => {
                this.IsDropAllowed = this.HasConfigurations;
                NotifyOfPropertyChange(() => this.HasConfigurations);
            };
            foreach (UserConfiguration current in ConfigurationStore.Instance.UserConfigurations)
            {
                UserConfigurationViewModel newUCVM = new UserConfigurationViewModel(current);
                this.UserConfigurations.Add(newUCVM);
                if (current.Equals(configurationToSelect)) { toSelect = newUCVM; }

                newUCVM.PropertyChanged += (a, b) => {
                    if (b.PropertyName.Equals("IsDefaultConfiguration"))
                    {
                        this.UserConfigurations.Where(p=>p!=a).ToList().ForEach(p => p.NotifyOfPropertyChange(""));
                    }
                };
            }

            if (toSelect == null)
            {
                toSelect = this.UserConfigurations.FirstOrDefault(p => p.IsDefaultConfiguration);
            }
            if (toSelect == null && this.UserConfigurations.Count > 0)
            {
                toSelect = this.UserConfigurations[0];
            }
            if (toSelect != null) { 
                this.SelectedUserConfiguration = toSelect ;
            }
        }
        
        public ObservableCollection<UserConfigurationViewModel> UserConfigurations
        {
            get;
            private set;
        }

        public bool HasConfigurations { get { return this.UserConfigurations != null && this.UserConfigurations.Count > 0; } }


        private UserConfigurationViewModel _selectedUserConfiguration = null;
        public UserConfigurationViewModel SelectedUserConfiguration
        {
            get { return this._selectedUserConfiguration; }
            set
            {
                
                if (value == null && this.UserConfigurations.Count == 0) {
                    this._selectedUserConfiguration = null;

                }
                else if (value == null && this.UserConfigurations.Count > 0)
                {
                    this._selectedUserConfiguration = this.UserConfigurations[0];    
                }else{
                    this._selectedUserConfiguration = value;
                }

                if(this._selectedUserConfiguration != null && !this._selectedUserConfiguration.AppsLoaded)
                {
                    Task t = this._selectedUserConfiguration.RefreshApps();
                }
                NotifyOfPropertyChange(() => this.SelectedUserConfiguration);
            }
        }

       

        public async void DeleteConfiguration()
        {
            MetroDialogSettings settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "delete",
                NegativeButtonText = "cancel"
            };
            IWindowManager wm = IoC.Get<IWindowManager>();
            if (await wm.ShowMessageAsync("Delete configuration?", "This will remove your account from the app and delete all locally stored settings.",
                MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative)
            {
                ConfigurationStore.Instance.UserConfigurations.Remove(this.SelectedUserConfiguration.UserConfiguration);
                ConfigurationStore.Instance.Save();
                this.UserConfigurations.Remove(this.SelectedUserConfiguration);
                if (this.UserConfigurations.Count > 0)
                {
                    this.SelectedUserConfiguration = this.UserConfigurations[0];
                }
                else
                {
                    this.SelectedUserConfiguration = null;
                }
            }
        }


        public async void RefreshConfiguration()
        {
            this.SelectedUserConfiguration.IsInvalid = false;
            await this.SelectedUserConfiguration.RefreshApps();
        }
        public void SetDefaultConfiguration()
        {
            this.SelectedUserConfiguration.IsDefaultConfiguration = true;
        }

        public bool CanSetDefaultConfiguration
        {
            get
            {
                return this.SelectedUserConfiguration != null && !this.SelectedUserConfiguration.IsDefaultConfiguration;
            }
        }

        public void ShowFeedbackFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowFeedbackFlyout();
        }


        public void ShowAboutFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowAboutFlyout();
        }

        public void ShowGeneralConfigurationFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowGeneralConfigurationFlyout();
        }

        public void ShowAddUserConfigurationFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowAddUserConfigurationFlyout();
        }

        public void ShowEditUserConfigurationFlyout()
        {
            IoC.Get<MainWindowViewModel>().ShowEditUserConfigurationFlyout(this.SelectedUserConfiguration);
        }

        #region Drag&Drop
        private bool _isDropAllowed = false;
        public Boolean IsDropAllowed
        {
            get { return this._isDropAllowed;}
            set{
                this._isDropAllowed = value;
                NotifyOfPropertyChange(() => this.IsDropAllowed);
            }
        }

        private bool _dnDSourceIsOK = false;
        public bool DDnDSourceIsOK
        {
            get
            {
                return this._dnDSourceIsOK;
            }
            set { 
                this._dnDSourceIsOK = value;
                NotifyOfPropertyChange(() => this.DDnDSourceIsOK);
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
                string[] filenames = dataObject.GetData(System.Windows.DataFormats.FileDrop) as string[];
                if (filenames.Length == 1)
                {
                    string filename = filenames[0];
                    if(File.Exists(filename)){

                        if (!String.IsNullOrWhiteSpace(Path.GetExtension(filename))
                            && CommandLineArgs.SupportedFileExtensions.Any(p => Path.GetExtension(filename).ToUpper().Equals(p)))
                        {
                            DropText = "Drop anywhere to upload";
                            DDnDSourceIsOK = true;
                            retVal = true;
                        }
                        else
                        {
                            DDnDSourceIsOK = false;
                            DropText = "File format not supported";
                        }
                    }
                    else if (Directory.Exists(filename))
                    {
                        DDnDSourceIsOK = false;
                        DropText = "Directories are not supported";
                    }
                }
                else
                {
                    DDnDSourceIsOK = false;
                    DropText = "Multiple items are not supported";
                }
            }
            else
            {
                DDnDSourceIsOK = false;
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

        public void OnDragLeave(System.Windows.DragEventArgs e) { 
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
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, "\"" + filename + "\"");
              
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
}