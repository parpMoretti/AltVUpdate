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
            TextBox.IsReadOnly = true;
            TextBox.AppendText($"\n[{DateTime.Now}] Started AltVUpdater - Version 2.0");

            User userSettings = User.Default;

            if (userSettings.UpdateRequired)
            {
                User.Default.Upgrade();
                User.Default.UpdateRequired = false;
                User.Default.Save();
            }

            if (string.IsNullOrEmpty(userSettings.Directory))
            {
                TextBox.AppendText($"\n[{DateTime.Now}] ERROR - You've not set a working directory in the User.Default!");
                return;
            }

            Update currentUpdate = null;

            if (File.Exists($"{userSettings.Directory}/update.json"))
            {
                using (StreamReader currentStreamReader = new StreamReader($"{userSettings.Directory}/update.json"))
                {
                    var currentUpdateString = currentStreamReader.ReadToEnd();

                    currentUpdate = JsonConvert.DeserializeObject<Update>(currentUpdateString);

                    currentStreamReader.Dispose();
                }
            }

            if (currentUpdate != null)
            {
                TextBox.AppendText($"\n[{DateTime.Now}] Current AltV Branch: {userSettings.Branch} - Current Build: {currentUpdate.LatestBuildNumber}");

                using (WebClient wc = new WebClient())
                {
                    string newUpdateString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", userSettings.Branch.ToLower());

                    var updateJson = wc.DownloadString($"{newUpdateString}update.json");

                    wc.Dispose();

                    Update updateInfo = JsonConvert.DeserializeObject<Update>(updateJson);

                    if (updateInfo.LatestBuildNumber > currentUpdate.LatestBuildNumber)
                    {
                        TextBox.AppendText($"\n[{DateTime.Now}] New Version Available - Build: {updateInfo.LatestBuildNumber}");
                    }
                }
            }
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
                if (string.IsNullOrEmpty(User.Default.Directory))
                {
                    TextBox.AppendText($"\n[{DateTime.Now}] ERROR - You've not set a working directory in the User.Default!");
                    return;
                }

                Update currentUpdate = null;

                long currentVersion = 0;

                if (File.Exists($"{User.Default.Directory}/update.json"))
                {
                    using (StreamReader currentStreamReader = new StreamReader($"{User.Default.Directory}/update.json"))
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

                if (currentUpdate != null)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string newUpdateString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", User.Default.Branch.ToLower());

                        var updateJson = wc.DownloadString($"{newUpdateString}update.json");

                        wc.Dispose();

                        Update updateInfo = JsonConvert.DeserializeObject<Update>(updateJson);

                        if (updateInfo.LatestBuildNumber == currentUpdate.LatestBuildNumber)
                        {
                            MessageBoxResult result = MessageBox.Show(
                                $"You have the latest version!\nDo you wish to continue?", "Latest Version",
                                MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (result == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                    }
                }

                string downloadString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", User.Default.Branch.ToLower());

                using (WebClient webClient = new WebClient())
                {
                    // altv-server.exe
                    if (File.Exists($"{User.Default.Directory}/altv-server.exe"))
                    {
                        if (File.Exists($"{User.Default.Directory}/altv-server.exe.{currentVersion}"))
                        {
                            File.Delete($"{User.Default.Directory}/altv-server.exe.{currentVersion}");
                        }

                        File.Move($"{User.Default.Directory}/altv-server.exe", $"{User.Default.Directory}/altv-server.exe.{currentVersion}");
                    }

                    webClient.DownloadFile($"{downloadString}altv-server.exe", $"{User.Default.Directory}/altv-server.exe");

                    // libnode.dll

                    if (File.Exists($"{User.Default.Directory}/libnode.dll"))
                    {
                        if (File.Exists($"{User.Default.Directory}/libnode.dll.{currentVersion}"))
                        {
                            File.Delete($"{User.Default.Directory}/libnode.dll.{currentVersion}");
                        }

                        File.Move($"{User.Default.Directory}/libnode.dll", $"{User.Default.Directory}/libnode.dll.{currentVersion}");
                    }

                    webClient.DownloadFile($"https://cdn.altv.mp/node-module/{User.Default.Branch.ToLower()}/x64_win32/libnode.dll", $"{User.Default.Directory}/libnode.dll");

                    if (Directory.Exists($"{User.Default.Directory}/data"))
                    {
                        //vehmods.bin
                        if (File.Exists($"{User.Default.Directory}/data/vehmods.bin"))
                        {
                            if (File.Exists($"{User.Default.Directory}/data/vehmods.bin.{currentVersion}"))
                            {
                                File.Delete($"{User.Default.Directory}/data/vehmods.bin.{currentVersion}");
                            }

                            File.Move($"{User.Default.Directory}/data/vehmods.bin", $"{User.Default.Directory}/data/vehmods.bin.{currentVersion}");
                        }

                        //vehmodels.bin
                        if (File.Exists($"{User.Default.Directory}/data/vehmodels.bin"))
                        {
                            if (File.Exists($"{User.Default.Directory}/data/vehmodels.bin.{currentVersion}"))
                            {
                                File.Delete($"{User.Default.Directory}/data/vehmodels.bin.{currentVersion}");
                            }

                            File.Move($"{User.Default.Directory}/data/vehmodels.bin", $"{User.Default.Directory}/data/vehmodels.bin.{currentVersion}");
                        }
                        webClient.DownloadFile($"{downloadString}data/vehmods.bin", $"{User.Default.Directory}/data/vehmods.bin");
                        webClient.DownloadFile($"{downloadString}data/vehmodels.bin", $"{User.Default.Directory}/data/vehmodels.bin");
                    }
                    else
                    {
                        TextBox.AppendText($"\n[{DateTime.Now}] Unable to download .bin files! Missing /data directory!");
                    }

                    if (User.Default.CSharp == true || User.Default.NodeJs == true)
                    {
                        if (Directory.Exists($"{User.Default.Directory}/modules"))
                        {
                            if (User.Default.CSharp == true)
                            {
                                // csharp-module.dll

                                if (File.Exists($"{User.Default.Directory}/modules/csharp-module.dll"))
                                {
                                    if (File.Exists($"{User.Default.Directory}/modules/csharp-module.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{User.Default.Directory}/modules/csharp-module.dll.{currentVersion}");
                                    }

                                    File.Move($"{User.Default.Directory}/modules/csharp-module.dll", $"{User.Default.Directory}/modules/csharp-module.dll.{currentVersion}");
                                }

                                webClient.DownloadFile(
                                    User.Default.Branch.ToLower() == "alpha"
                                        ? $"https://cdn.altv.mp/coreclr-module/stable/x64_win32/modules/csharp-module.dll"
                                        : $"https://cdn.altv.mp/coreclr-module/{User.Default.Branch.ToLower()}/x64_win32/modules/csharp-module.dll",
                                    $"{User.Default.Directory}/modules/csharp-module.dll");

                                // AltV.Net.Host

                                if (File.Exists($"{User.Default.Directory}/AltV.Net.Host.dll"))
                                {
                                    if (File.Exists($"{User.Default.Directory}/AltV.Net.Host.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{User.Default.Directory}/AltV.Net.Host.dll.{currentVersion}");
                                    }

                                    File.Move($"{User.Default.Directory}/AltV.Net.Host.dll", $"{User.Default.Directory}/AltV.Net.Host.dll.{currentVersion}");
                                }

                                webClient.DownloadFile(
                                    User.Default.Branch.ToLower() == "alpha"
                                        ? $"https://cdn.altv.mp/coreclr-module/stable/x64_win32/AltV.Net.Host.dll"
                                        : $"https://cdn.altv.mp/coreclr-module/{User.Default.Branch.ToLower()}/x64_win32/AltV.Net.Host.dll",
                                    $"{User.Default.Directory}/AltV.Net.Host.dll");

                                // runtimeconfig.json

                                if (File.Exists($"{User.Default.Directory}/AltV.Net.Host.runtimeconfig.json"))
                                {
                                    if (File.Exists($"{User.Default.Directory}/AltV.Net.Host.runtimeconfig.json.{currentVersion}"))
                                    {
                                        File.Delete($"{User.Default.Directory}/AltV.Net.Host.runtimeconfig.json.{currentVersion}");
                                    }

                                    File.Move($"{User.Default.Directory}/AltV.Net.Host.runtimeconfig.json", $"{User.Default.Directory}/AltV.Net.Host.runtimeconfig.json.{currentVersion}");
                                }

                                webClient.DownloadFile(
                                    User.Default.Branch.ToLower() == "alpha"
                                        ? $"https://cdn.altv.mp/coreclr-module/stable/x64_win32/AltV.Net.Host.runtimeconfig.json"
                                        : $"https://cdn.altv.mp/coreclr-module/{User.Default.Branch.ToLower()}/x64_win32/AltV.Net.Host.runtimeconfig.json",
                                    $"{User.Default.Directory}/AltV.Net.Host.runtimeconfig.json");
                            }

                            if (User.Default.NodeJs == true)
                            {
                                // node-module.dll
                                if (File.Exists($"{User.Default.Directory}/modules/node-module.dll"))
                                {
                                    if (File.Exists($"{User.Default.Directory}/modules/node-module.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{User.Default.Directory}/modules/node-module.dll.{currentVersion}");
                                    }

                                    File.Move($"{User.Default.Directory}/modules/node-module.dll", $"{User.Default.Directory}/modules/node-module.dll.{currentVersion}");
                                }
                                webClient.DownloadFile($"https://cdn.altv.mp/node-module/{User.Default.Branch.ToLower()}/x64_win32/modules/node-module.dll", $"{User.Default.Directory}/modules/node-module.dll");
                            }
                        }
                        else
                        {
                            TextBox.AppendText($"\n[{DateTime.Now}] Unable to download .dll files! Missing /modules directory!");
                        }
                    }

                    if (File.Exists($"{User.Default.Directory}/update.json"))
                    {
                        File.Delete($"{User.Default.Directory}/update.json");
                    }

                    webClient.DownloadFile($"{downloadString}update.json", $"{User.Default.Directory}/update.json");

                    webClient.Dispose();
                }

                using (StreamReader streamReader = new StreamReader($"{User.Default.Directory}/update.json"))
                {
                    var updateString = streamReader.ReadToEnd();

                    Update updateInfo = JsonConvert.DeserializeObject<Update>(updateString);

                    TextBox.AppendText($"\n[{DateTime.Now}] Updated AltV Server to {User.Default.Branch} Branch - Build #{updateInfo.LatestBuildNumber}");
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
                TextBox.AppendText($"\n[{DateTime.Now}] Stopping AltV Server!");
                return;
            }

            TextBox.AppendText($"\n[{DateTime.Now}] Unable to find the AltV Process!");
        }

        private void StartServerButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process altVProcess = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.Contains("altv"));

            if (altVProcess != null)
            {
                TextBox.AppendText($"\n[{DateTime.Now}] The server is already running!");
                return;
            }

            ProcessStartInfo altVStartProcess = new ProcessStartInfo($"{User.Default.Directory}/altv-server.exe");
            altVStartProcess.WorkingDirectory = User.Default.Directory;

            altVProcess = Process.Start(altVStartProcess);

            if (altVProcess == null || !altVProcess.Responding)
            {
                TextBox.AppendText($"\n[{DateTime.Now}] Error starting the server!");
                return;
            }

            using (WebClient wc = new WebClient())
            {
                string downloadString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", User.Default.Branch.ToLower());

                var updateJson = wc.DownloadString($"{downloadString}update.json");

                wc.Dispose();

                Update updateInfo = JsonConvert.DeserializeObject<Update>(updateJson);

                if (File.Exists($"{User.Default.Directory}/update.json"))
                {
                    using (StreamReader streamReader = new StreamReader($"{User.Default.Directory}/update.json"))
                    {
                        var updateString = streamReader.ReadToEnd();

                        streamReader.Dispose();

                        Update currentUpdateInfo = JsonConvert.DeserializeObject<Update>(updateString);

                        if (updateInfo.LatestBuildNumber > currentUpdateInfo.LatestBuildNumber)
                        {
                            TextBox.AppendText($"\n[{DateTime.Now}] Server Started!\nNew build available for branch: {User.Default.Branch}\nCurrent Build: {currentUpdateInfo.LatestBuildNumber}\nLatest Build: {updateInfo.LatestBuildNumber}");

                            return;
                        }
                    }
                }
            }
            TextBox.AppendText($"\n[{ DateTime.Now}] Server Started!");
        }

        private void RemoveOldBuildsButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int count = 0;

                if (Directory.Exists($"{User.Default.Directory}\\modules"))
                {
                    foreach (var file in Directory.GetFiles($"{User.Default.Directory}\\modules"))
                    {
                        if (file.Contains($"{User.Default.Directory}\\modules\\csharp-module.dll."))
                        {
#if DEBUG
                            using (var writer = File.AppendText("debug.log"))
                            {
                                writer.WriteLine(file);
                                writer.Dispose();
                            }
#endif
                            File.Delete(file);

                            count++;
                        }
                        if (file.Contains($"{User.Default.Directory}\\modules\\node-module.dll."))
                        {
#if DEBUG
                            using (var writer = File.AppendText("debug.log"))
                            {
                                writer.WriteLine(file);
                                writer.Dispose();
                            }
#endif
                            File.Delete(file);

                            count++;
                        }
                    }
                }

                if (Directory.Exists($"{User.Default.Directory}\\data"))
                {
                    foreach (var file in Directory.GetFiles($"{User.Default.Directory}\\data"))
                    {
                        if (file.Contains($"{User.Default.Directory}\\data\\vehmodels.bin."))
                        {
                            File.Delete(file);
                            count++;
                        }
                        if (file.Contains($"{User.Default.Directory}\\data\\vehmods.bin."))
                        {
                            File.Delete(file);
                            count++;
                        }
                    }
                }

                if (Directory.Exists($"{User.Default.Directory}"))
                {
                    foreach (var file in Directory.GetFiles($"{User.Default.Directory}"))
                    {
                        if (file.Contains("altv-server.exe") && file != $"{User.Default.Directory}\\altv-server.exe")
                        {
                            File.Delete(file);
                            count++;
                        }

                        if (file.Contains("AltV.Net.Host.dll.") || file.Contains("AltV.Net.Host.runtimeconfig.") || file.Contains("node.dll."))
                        {
                            File.Delete(file);
                            count++;
                        }
                    }
                }

                TextBox.AppendText($"\n[{DateTime.Now}] Successfully cleaned {count} items.");
            }
            catch (Exception exception)
            {
                TextBox.AppendText($"\n[{DateTime.Now}] {exception.Message}.");
                File.WriteAllText("crash.log", $"Message: {exception.Message}\nStack: {exception.StackTrace}\nSource: {exception.Source}");
                this.Close();
            }
        }
    }
}