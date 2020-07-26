using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SteamInteropTest.Logging;
using SteamVent.InterProc;
using SteamVent.InterProc.Attributes;
using SteamVent.InterProc.Interfaces;

namespace SteamInteropTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<string> _logOutput;

        private bool IsSteamInitialized { get; set; }

        private ISteamClient017 SteamClient017 { get; set; }

        private Int32 Pipe { get; set; }

        private Int32 User { get; set; }

        private ISteamApps006 SteamApps006 { get; set; }

        public MainWindow()
        {
            SteamVent.Common.Logging.Logger.Log = new Logger();
            InitializeComponent();
            _logOutput = new List<string>();
            lstBoxLogOutput.DataContext = _logOutput;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cleanup();
        }

        private void InitSteam()
        {
            if (IsSteamInitialized)
            {
                WriteToWindowLog("Steam is already initialized!");
                return;
            }

            if (!Steam.Load())
            {
                WriteToWindowLog("Steam NOT successfully initialized!");
                return;
            }

            WriteToWindowLog("Steam successfully initialized!");
            IsSteamInitialized = true;
        }

        private void InitSteamClient017()
        {
            if (!IsSteamInitialized)
            {
                WriteToWindowLog("Steam must be initialized prior to ISteamClient017.");
                return;
            }

            //     - For steam.dll interfaces use 'Steam.CreateSteamInterface'
            //     - For steamclient.dll/steamclient64.dll interfaces use 'Steam.CreateInterface'

            SteamClient017 = Steam.CreateInterface<ISteamClient017>();

            if (SteamClient017 == null)
            {
                WriteToWindowLog("Failed to initialize ISteamClient017!");
                return;
            }

            WriteToWindowLog("Successfully initialized ISteamClient017!");

            Pipe = SteamClient017.CreateSteamPipe();
            User = SteamClient017.ConnectToGlobalUser(Pipe);
        }

        private void InitSteamApps006()
        {
            if (SteamClient017 == null)
            {
                WriteToWindowLog("ISteamClient017 must be initialized prior to ISteamApps006.");
                return;
            }

            //var interfaceVersionString = ((InterfaceVersion)typeof(ISteamApps006).GetCustomAttribute(typeof(InterfaceVersion))).Version;
            //var steamApps006InterfacePtr = SteamClient017.GetISteamApps(User, Pipe, interfaceVersionString);
            //if (steamApps006InterfacePtr == IntPtr.Zero)
            //{
            //    WriteToWindowLog("Failed to initialize ISteamApps006!");
            //    return;
            //}
            //
            //WriteToWindowLog("Successfully initialized ISteamApps006!");

            //SteamApps006InterfacePtr = steamApps006InterfacePtr;
            //SteamApps006 = new ISteamApps006(SteamApps006InterfacePtr);

            SteamApps006 = SteamClient017.GetISteamApps<ISteamApps006>(User, Pipe);
            if (SteamApps006 == null)
            {
                WriteToWindowLog("Failed to initialize ISteamApps006!");
                return;
            }
            WriteToWindowLog("Successfully initialized ISteamApps006!");
        }

        private void GetCurrentGameLanguage()
        {
            if (SteamApps006 == null)
            {
                WriteToWindowLog("ISteamApps006 is not yet initialized.");
                return;
            }

            var currentGameLang = SteamApps006.GetCurrentGameLanguage();
            WriteToWindowLog($"Current Game Language: {currentGameLang}");
        }

        private void GetAvailableGameLanguages()
        {
            if (SteamApps006 == null)
            {
                WriteToWindowLog("ISteamApps006 is not yet initialized.");
                return;
            }

            var availableGameLanguages = SteamApps006.GetAvailableGameLanguages();
            WriteToWindowLog($"Available Game Languages: {availableGameLanguages}");
        }

        private void GetIsAppInstalled()
        {
            if (SteamApps006 == null)
            {
                WriteToWindowLog("ISteamApps006 is not yet initialized.");
                return;
            }

            if (!UInt32.TryParse(txtIsAppInstalledAppId.Text, out var appId))
            {
                WriteToWindowLog("Invalid AppID!");
                return;
            }

            var isAppInstalled = SteamApps006.BIsAppInstalled(appId);
            //var isDlcInstalled = SteamApps006.BIsDlcInstalled(appId);
            WriteToWindowLog($"The AppID '{appId}' is {(isAppInstalled ? "" : "NOT ")}installed.");
            //WriteToWindowLog($"The DLC '{appId}' is {(isDlcInstalled > 0 ? "" : "NOT ")}installed. {isDlcInstalled}");
        }

        private void Cleanup()
        {
            if (SteamClient017 == null)
                return;

            SteamClient017.ReleaseUser(Pipe, User);
            SteamClient017.BReleaseSteamPipe(Pipe);
        }

        private void WriteToWindowLog(string msg)
        {
            _logOutput.Add(msg);
            lstBoxLogOutput.Items.Refresh();
        }

        private void BtnInitSteam_Click(object sender, RoutedEventArgs e)
        {
            InitSteam();
        }

        private void BtnInitISteamClient017_Click(object sender, RoutedEventArgs e)
        {
            InitSteamClient017();
        }

        private void BtnInitISteamApps006_Click(object sender, RoutedEventArgs e)
        {
            InitSteamApps006();
        }

        private void BtnGetCurrentGameLanguage_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentGameLanguage();
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            // Test case just to show that creating a Steam interface works (not SteamClient)
            // Just uncomment it out if you want to try it

            //var interfacePtr = Steam.CreateSteamInterface("Steam006");
            //if (interfacePtr == IntPtr.Zero)
            //{
            //    WriteToWindowLog("Failed!");
            //    return;
            //}

            //WriteToWindowLog($"Success! 0x{interfacePtr.ToInt64():X}");

            //if (!UInt32.TryParse(txtIsAppInstalledAppId.Text, out var appId))
            //{
            //    WriteToWindowLog("Invalid AppID!");
            //    return;
            //}

            //UInt32 unix = SteamApps006.GetEarliestPurchaseUnixTime(appId);
            //WriteToWindowLog($"GetEarliestPurchaseUnixTime {unix} {DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime()}");
        }

        private void BtnGetAvailableGameLanguages_Click(object sender, RoutedEventArgs e)
        {
            GetAvailableGameLanguages();
        }

        private void BtnGetIsAppInstalled_Click(object sender, RoutedEventArgs e)
        {
            GetIsAppInstalled();
        }
    }
}
