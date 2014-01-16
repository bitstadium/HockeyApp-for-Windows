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
using MahApps.Metro.Controls.Dialogs;

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
            
            //TODO
            //HockeyApp.HockeyClientWPF.Instance.SendCrashesNowAsync();
        }


        private async Task ShowHelp(IWindowManager wm)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter tw = new StringWriter(sb);
            CommandLineArgs.WriteHelp(tw, "HockeyUpload");
            await wm.ShowSimpleMessageAsync("Usage", sb.ToString());
        }
        protected async override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            _logger.Info("OnStartup");

            var shell = IoC.Get<MainWindowViewModel>();

            if (Environment.GetCommandLineArgs().Count() > 1)
            {
                shell.PreferredHeight = 495;
                shell.PreferredWidth = 642;
                base.OnStartup(sender, e);
                IWindowManager wm = IoC.Get<IWindowManager>();

                if (Environment.CommandLine.ToUpper().Contains("/HELP"))
                {
                    await ShowHelp(wm);
                    Application.Shutdown();
                }


                ProgressDialogController pdc = await wm.ShowProgressAsync("Matching apps...", "Please wait - we are looking for your app-configuration");
                
                CommandLineArgs cmdLineArgs = null;
                Exception exThrown = null;
                try
                {
                    _logger.Info("Commandline Args=" + Environment.CommandLine);
                    cmdLineArgs = Args.Configuration.Configure<HockeyApp.AppLoader.Model.CommandLineArgs>().CreateAndBind(Environment.GetCommandLineArgs());
                    string errMsg = "";
                    if (!cmdLineArgs.IsValid(out errMsg))
                    {
                        _logger.Warn("Command line args invalid: " + errMsg);
                        throw new Exception(errMsg);
                    }
                    this.container.ComposeExportedValue<CommandLineArgs>(cmdLineArgs);

                    AppInfoMatcher matcher = new AppInfoMatcher();
                    List<AppInfo> list = await matcher.GetMatchingApps(cmdLineArgs);
                    if (list.Count == 0)
                    {
                        await pdc.CloseAsync();
                        await wm.ShowSimpleMessageAsync("Could not find matching apps", "No matching application found. Please check the configuration information!");
                        Application.Current.Shutdown(-1);
                        return;
                    }


                    UploadDialogViewModel vm = new UploadDialogViewModel(list, matcher.ActiveUserConfiguration);
                    vm.Closed += delegate(object s, EventArgs args)
                    {
                        Application.Shutdown(0);
                    };
                    await pdc.CloseAsync();
                    shell.Init(vm);
                   
                }catch(Exception ex){
                    exThrown = ex;
                }
                if (exThrown != null)
                {
                    
                    await pdc.CloseAsync();
                    await wm.ShowSimpleMessageAsync("Error", "An exception was thrown:\n" + exThrown.Message);
                    Application.Shutdown(-1);
                }
            }
            else
            {
                shell.PreferredWidth = 1280;
                shell.PreferredHeight = 800;
                base.OnStartup(sender, e);

                ConfigurationStore config = IoC.Get<ConfigurationStore>();
                ApplicationsViewModel avm = new ApplicationsViewModel();
                
                
                shell.Init(avm);
                
                if (config.UserConfigurations.Count == 0){
                    shell.ShowAddUserConfigurationFlyout();
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
