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

        public ICollectionView Apps { get; set; }
        private string _filterString = "";
        public string FilterString
        {
            get
            {
                return this._filterString;
            }
            set
            {
                this._filterString = value;
                NotifyOfPropertyChange(() => this.FilterString);
                this.Apps.Refresh();
            }
        }
        
        public ObservableCollection<UserConfigurationViewModel> UserConfigurations
        {
            get;
            private set;
        }

        public bool HasConfigurations { get { return this.UserConfigurations != null && this.UserConfigurations.Count > 0; } }

        private bool _cboChangedByUser = true;
        public async void SelectedUserConfiguration_OnSelectionChanged()
        {
            if (this._cboChangedByUser)
            {

                if (this.SelectedUserConfiguration == null)
                {
                    this.Apps = null;                    
                }
                else
                {
                    try
                    {
                        await this.RefreshApps();
                    }
                    catch 
                    {
                        this._cboChangedByUser = false;
                        this.SelectedUserConfiguration = this._oldSelectedUserConfiguration;
                    }
                }
            }
            else
            {
                this._cboChangedByUser = true;
            }
        }

        private UserConfigurationViewModel _oldSelectedUserConfiguration = null;
        private UserConfigurationViewModel _selectedUserConfiguration = null;
        public UserConfigurationViewModel SelectedUserConfiguration
        {
            get { return this._selectedUserConfiguration; }
            set
            {
                this._oldSelectedUserConfiguration = this._selectedUserConfiguration;
                this._selectedUserConfiguration = value;
               
                NotifyOfPropertyChange(() => this.SelectedUserConfiguration);
                this.SelectedUserConfiguration_OnSelectionChanged();
            }
        }

        public async Task RefreshApps()
        {
            Exception exThrown = null;
            var wm = Caliburn.Micro.IoC.Get<IWindowManager>();
            ProgressDialogController pdc = await  wm.ShowProgressAsync("Please wait...", "Loading apps and settings");
            try
            {
                AppInfoEnvelope envelope = await AppInfoEnvelope.Load(this._selectedUserConfiguration.UserConfiguration);
                
                List<AppConfigViewModel> list = new List<AppConfigViewModel>();
                if (envelope != null && envelope.Apps != null)
                {
                    foreach (AppInfo current in envelope.Apps)
                    {
                        list.Add(new AppConfigViewModel(current, this.SelectedUserConfiguration.UserConfiguration));
                    }
                }

                this.Apps = CollectionViewSource.GetDefaultView(list);
                this.Apps.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                this.Apps.SortDescriptions.Add(new SortDescription("ReleaseType", ListSortDirection.Ascending));

                this.Apps.Filter = (x) => { return (x as AppConfigViewModel).Title.ToUpper().Contains(this.FilterString.ToUpper())
                    || (x as AppConfigViewModel).Platform.ToUpper().Contains(this.FilterString.ToUpper());
                };

                this.Apps.MoveCurrentToFirst();
                NotifyOfPropertyChange(() => this.Apps);
            }
            catch (Exception ex)
            {
                exThrown = ex;
            }
            await pdc.CloseAsync();

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK",
            };
            if (exThrown != null)
            {
                await wm.ShowMessageAsync("Error", "An error occurred:\n" + exThrown.Message, MessageDialogStyle.Affirmative, mySettings);
                throw exThrown;
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


        public void SetDefaultConfiguration()
        {
            ConfigurationStore.Instance.SetDefaultUserConfiguration(this.SelectedUserConfiguration.UserConfiguration);
            ConfigurationStore.Instance.Save();
            this.UserConfigurations.ToList().ForEach(p => p.NotifyOfPropertyChange(() => p.IsDefaultConfiguration));
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
                
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location,filename);
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