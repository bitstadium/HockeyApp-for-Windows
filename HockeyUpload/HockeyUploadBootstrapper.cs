using Caliburn.Micro;
using HockeyApp.AppLoader.Model;
using HockeyApp.AppLoader.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.AppLoader.Extensions;
using System.Reflection;
using Caliburn.Micro.Logging.NLog;
using HockeyApp.AppLoader.Util;
using System.Windows;
using HockeyApp.AppLoader.PlatformStrategies;
using System.IO;

namespace HockeyApp.AppLoader
{
    public class HockeyUploadBootstrapper:Bootstrapper<MainWindowViewModel>
    {
        public static EventAggregator Aggregator { get; set; }
        private CompositionContainer container;


        public static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static HockeyUploadBootstrapper()
        {
            LogManager.GetLog = type => new NLogLogger(type);
            HockeyApp.HockeyLogManager.GetLog = type => new HockeyAppLogger();
            
        }

        protected override void Configure()
        {
            this.container = new CompositionContainer(new AggregateCatalog(
                   AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
                   )
               );

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new MetroWindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue<ConfigurationStore>(ConfigurationStore.Instance);
            batch.AddExportedValue<MainWindowViewModel>(new MainWindowViewModel());
            
            batch.AddExportedValue(container);

            container.Compose(batch);
            
            
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            HockeyApp.HockeyClientWPF.Instance.Configure(
                HockeyApp.AppLoader.Properties.Settings.Default.AppID,
                version.ToString(),
                null,
                null,
                (ex) =>
                {
                    return "";
                });
            
            HockeyApp.HockeyClientWPF.Instance.SendCrashesNowAsync();
        }

        protected async override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            _logger.Info("OnStartup");

            base.OnStartup(sender, e);

            var shell = IoC.Get<MainWindowViewModel>();

            if (Environment.GetCommandLineArgs().Count() > 1)
            {
                CommandLineArgs cmdLineArgs = null;
                try
                {
                    if (Environment.CommandLine.ToUpper().Contains("/HELP"))
                    {
                        throw new Exception();
                    }

                    _logger.Info("Commandline Args=" + Environment.CommandLine);
                    IWindowManager wm = IoC.Get<IWindowManager>();

                    cmdLineArgs = Args.Configuration.Configure<HockeyApp.AppLoader.Model.CommandLineArgs>().CreateAndBind(Environment.GetCommandLineArgs());
                    string errMsg = "";
                    if (!cmdLineArgs.IsValid(out errMsg))
                    {
                        _logger.Warn("Command line args invalid: " + errMsg);
                        throw new Exception();
                    }
                }catch{
                    StringBuilder sb = new StringBuilder();
                    StringWriter tw = new StringWriter(sb);
                    CommandLineArgs.WriteHelp(tw, "HockeyUpload");
                    MessageBox.Show(sb.ToString(), "Usage");
                    Application.Shutdown();
                }

                this.container.ComposeExportedValue<CommandLineArgs>(cmdLineArgs);

                AppInfoMatcher matcher = new AppInfoMatcher();
                List<AppInfo> list = await matcher.GetMatchingApps(cmdLineArgs);
                if (list.Count == 0)
                {
                    MessageBox.Show("No matching application found. Please check the configuration information!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Shutdown(-1);
                }


                UploadDialogViewModel vm = new UploadDialogViewModel(list, matcher.ActiveUserConfiguration);
                vm.Closed += delegate(object s, EventArgs args)
                {
                    Application.Shutdown(0);
                };
                try
                {
                    shell.Init(vm);
                    //shell.IsDialog = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    MessageBox.Show("Error while loading applications!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                ConfigurationStore config = IoC.Get<ConfigurationStore>();
                if (config.UserConfigurations.Count == 0)
                {
                    InitialConfigurationViewModel initialVM = new InitialConfigurationViewModel();
                    shell.Init(initialVM);
                    //shell.IsDialog = false;
                    initialVM.Closed += (a, b) =>
                    {
                        if (config.UserConfigurations.Count > 0)
                        {
                            this.container.ComposeExportedValue<ConfigurationViewModel>(new ConfigurationViewModel());
                            this.container.ComposeExportedValue<FeedbackViewModel>(new FeedbackViewModel());
                            this.container.ComposeExportedValue<ApplicationsViewModel>(new ApplicationsViewModel());

                            shell.Init(new ConfigurationContentViewModel());
                        }
                        else
                        {
                            Application.Shutdown();
                        }
                    };
                }
                else
                {
                    this.container.ComposeExportedValue<ConfigurationViewModel>(new ConfigurationViewModel());
                    this.container.ComposeExportedValue<FeedbackViewModel>(new FeedbackViewModel());
                    this.container.ComposeExportedValue<ApplicationsViewModel>(new ApplicationsViewModel());

                    shell.Init(new ApplicationsViewModel());
                    //shell.IsDialog = false;
                }
            }
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);

            if (exports.Count() > 0)
            {
                return exports.First();
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }

    }
}
