using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyAppLoader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public TelemetryClient AIClient;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create new Telemetry Client with proper iKey
            AIClient = new TelemetryClient();
            AIClient.InstrumentationKey = "ceb7f928-d67f-479d-930b-2afa7c42a386";

            // Set AnonUserId using SHA256 hash.
            SHA256 sha = SHA256Managed.Create();
            byte[] hashedBytes = sha.ComputeHash(Encoding.Unicode.GetBytes(Environment.UserName));
            StringBuilder anonID = new StringBuilder();
            foreach(byte b in hashedBytes)
            {
                anonID.AppendFormat("{0:X2}", b);
            }
            AIClient.Context.User.Id = anonID.ToString();

            // Log a PageView as a substitute for a session event
            AIClient.TrackPageView("App Start");

            // Flush so that the messages are sent before closing down
            AIClient.Flush();
            
        }
        public static string UserFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HockeyUpload");
            }
        }

    }
}
