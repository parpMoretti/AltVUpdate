using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json;

namespace AltVUpdate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConfigItem_OnClick(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();

            settingsWindow.Show();
        }

        private void CloseItem_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateConfigButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Setting settings = Setting.FetchSettings();

                Update currentUpdate = null;

                long currentVersion = 0;

                if (File.Exists($"{settings.Directory}/update.json"))
                {
                    using (StreamReader currentStreamReader = new StreamReader($"{settings.Directory}/update.json"))
                    {
                        var currentUpdateString = currentStreamReader.ReadToEnd();

                        currentUpdate = JsonConvert.DeserializeObject<Update>(currentUpdateString);

                        if (currentUpdate != null)
                        {
                            currentVersion = currentUpdate.LatestBuildNumber;
                        }

                        currentStreamReader.Dispose();
                    }
                }

                string downloadString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", settings.Branch.ToLower());

                using (WebClient webClient = new WebClient())
                {
                    if (File.Exists($"{settings.Directory}/altv-server.exe"))
                    {
                        if (File.Exists($"{settings.Directory}/altv-server.exe.{currentVersion}"))
                        {
                            File.Delete($"{settings.Directory}/altv-server.exe.{currentVersion}");
                        }

                        File.Move($"{settings.Directory}/altv-server.exe", $"{settings.Directory}/altv-server.exe.{currentVersion}");
                    }

                    webClient.DownloadFile($"{downloadString}altv-server.exe", $"{settings.Directory}/altv-server.exe");

                    if (Directory.Exists($"{settings.Directory}/data"))
                    {
                        if (File.Exists($"{settings.Directory}/data/vehmods.bin"))
                        {
                            if (File.Exists($"{settings.Directory}/data/vehmods.bin.{currentVersion}"))
                            {
                                File.Delete($"{settings.Directory}/data/vehmods.bin.{currentVersion}");
                            }

                            File.Move($"{settings.Directory}/data/vehmods.bin", $"{settings.Directory}/data/vehmods.bin.{currentVersion}");
                        }

                        if (File.Exists($"{settings.Directory}/data/vehmodels.bin"))
                        {
                            if (File.Exists($"{settings.Directory}/data/vehmodels.bin.{currentVersion}"))
                            {
                                File.Delete($"{settings.Directory}/data/vehmodels.bin.{currentVersion}");
                            }

                            File.Move($"{settings.Directory}/data/vehmodels.bin", $"{settings.Directory}/data/vehmodels.bin.{currentVersion}");
                        }
                        webClient.DownloadFile($"{downloadString}data/vehmods.bin", $"{settings.Directory}/data/vehmods.bin");
                        webClient.DownloadFile($"{downloadString}data/vehmodels.bin", $"{settings.Directory}/data/vehmodels.bin");
                    }
                    else
                    {
                        MessageBox.Show($"Unable to download .bin files! Missing /data directory!", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (settings.CSharp == true || settings.Node == true)
                    {
                        if (Directory.Exists($"{settings.Directory}/modules"))
                        {
                            if (settings.CSharp == true)
                            {
                                if (File.Exists($"{settings.Directory}/modules/csharp-module.dll"))
                                {
                                    if (File.Exists($"{settings.Directory}/modules/csharp-module.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{settings.Directory}/modules/csharp-module.dll.{currentVersion}");
                                    }

                                    File.Move($"{settings.Directory}/modules/csharp-module.dll", $"{settings.Directory}/modules/csharp-module.dll.{currentVersion}");
                                }
                                webClient.DownloadFile("https://alt-cdn.s3.nl-ams.scw.cloud/coreclr-module/beta/x64_win32/csharp-module.dll", $"{settings.Directory}/modules/csharp-module.dll");
                            }

                            if (settings.Node == true)
                            {
                                if (File.Exists($"{settings.Directory}/modules/node-module.dll"))
                                {
                                    if (File.Exists($"{settings.Directory}/modules/node-module.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{settings.Directory}/modules/node-module.dll.{currentVersion}");
                                    }

                                    File.Move($"{settings.Directory}/modules/node-module.dll", $"{settings.Directory}/modules/node-module.dll.{currentVersion}");
                                }
                                webClient.DownloadFile("https://alt-cdn.s3.nl-ams.scw.cloud/node-module/beta/x64_win32/node-module.dll", $"{settings.Directory}/modules/node-module.dll");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Unable to download .dll files! Missing /modules directory!", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    if (File.Exists($"{settings.Directory}/update.json"))
                    {
                        File.Delete($"{settings.Directory}/update.json");
                    }

                    webClient.DownloadFile($"{downloadString}update.json", $"{settings.Directory}/update.json");

                    webClient.Dispose();
                }

                using (StreamReader streamReader = new StreamReader($"{settings.Directory}/update.json"))
                {
                    var updateString = streamReader.ReadToEnd();

                    Update updateInfo = JsonConvert.DeserializeObject<Update>(updateString);

                    MessageBox.Show(
                        $"Updated AltV Server to {settings.Branch} Branch - Build #{updateInfo.LatestBuildNumber}");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}");
                File.WriteAllText("crash.log", $"Message: {exception.Message}\nStack: {exception.StackTrace}\nSource: {exception.Source}");
                this.Close();
            }
        }

        private void StopServerButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process altVProcess = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.Contains("altv"));

            if (altVProcess != null)
            {
                altVProcess.Kill();
                MessageBox.Show($"Stopping Alt:V Server!\n", "Stopping", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            MessageBox.Show("Unable to find the Alt:V Process!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void StartServerButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process altVProcess = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.Contains("altv"));

            if (altVProcess != null)
            {
                MessageBox.Show($"The server is already running!", "Error!", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Setting currentSettings = Setting.FetchSettings();

            ProcessStartInfo altVStartProcess = new ProcessStartInfo($"{currentSettings.Directory}/altv-server.exe");
            altVStartProcess.WorkingDirectory = currentSettings.Directory;

            altVProcess = Process.Start(altVStartProcess);

            if (altVProcess == null || !altVProcess.Responding)
            {
                MessageBox.Show($"Error starting the server!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (WebClient wc = new WebClient())
            {
                string downloadString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", currentSettings.Branch.ToLower());

                var updateJson = wc.DownloadString($"{downloadString}update.json");

                wc.Dispose();

                Update updateInfo = JsonConvert.DeserializeObject<Update>(updateJson);

                if (File.Exists($"{currentSettings.Directory}/update.json"))
                {
                    using (StreamReader streamReader = new StreamReader($"{currentSettings.Directory}/update.json"))
                    {
                        var updateString = streamReader.ReadToEnd();

                        streamReader.Dispose();

                        Update currentUpdateInfo = JsonConvert.DeserializeObject<Update>(updateString);

                        if (updateInfo.LatestBuildNumber > currentUpdateInfo.LatestBuildNumber)
                        {
                            MessageBox.Show($"Server Started!\nNew build available for branch: {currentSettings.Branch}\nCurrent Build: {currentUpdateInfo.LatestBuildNumber}\nLatest Build: {updateInfo.LatestBuildNumber}",
                                "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }

            MessageBox.Show($"Server Started!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}