﻿using System;
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
            TextBox.AppendText($"\n[{DateTime.Now}] Started AltVUpdater - Version 1.8");

            Setting settings = Setting.FetchSettings();

            Update currentUpdate = null;

            if (File.Exists($"{settings.Directory}/update.json"))
            {
                using (StreamReader currentStreamReader = new StreamReader($"{settings.Directory}/update.json"))
                {
                    var currentUpdateString = currentStreamReader.ReadToEnd();

                    currentUpdate = JsonConvert.DeserializeObject<Update>(currentUpdateString);

                    currentStreamReader.Dispose();
                }
            }

            if (currentUpdate != null)
            {
                TextBox.AppendText($"\n[{DateTime.Now}] Current AltV Branch: {settings.Branch} - Current Build: {currentUpdate.LatestBuildNumber}");

                using (WebClient wc = new WebClient())
                {
                    string newUpdateString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", settings.Branch.ToLower());

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

                if (currentUpdate != null)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string newUpdateString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", settings.Branch.ToLower());

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

                string downloadString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", settings.Branch.ToLower());

                using (WebClient webClient = new WebClient())
                {
                    // altv-server.exe
                    if (File.Exists($"{settings.Directory}/altv-server.exe"))
                    {
                        if (File.Exists($"{settings.Directory}/altv-server.exe.{currentVersion}"))
                        {
                            File.Delete($"{settings.Directory}/altv-server.exe.{currentVersion}");
                        }

                        File.Move($"{settings.Directory}/altv-server.exe", $"{settings.Directory}/altv-server.exe.{currentVersion}");
                    }

                    webClient.DownloadFile($"{downloadString}altv-server.exe", $"{settings.Directory}/altv-server.exe");

                    // libnode.dll

                    if (File.Exists($"{settings.Directory}/libnode.dll"))
                    {
                        if (File.Exists($"{settings.Directory}/libnode.dll.{currentVersion}"))
                        {
                            File.Delete($"{settings.Directory}/libnode.dll.{currentVersion}");
                        }

                        File.Move($"{settings.Directory}/libnode.dll", $"{settings.Directory}/libnode.dll.{currentVersion}");
                    }

                    webClient.DownloadFile($"https://cdn.altv.mp/node-module/{settings.Branch.ToLower()}/x64_win32/libnode.dll", $"{settings.Directory}/libnode.dll");

                    if (Directory.Exists($"{settings.Directory}/data"))
                    {
                        //vehmods.bin
                        if (File.Exists($"{settings.Directory}/data/vehmods.bin"))
                        {
                            if (File.Exists($"{settings.Directory}/data/vehmods.bin.{currentVersion}"))
                            {
                                File.Delete($"{settings.Directory}/data/vehmods.bin.{currentVersion}");
                            }

                            File.Move($"{settings.Directory}/data/vehmods.bin", $"{settings.Directory}/data/vehmods.bin.{currentVersion}");
                        }

                        //vehmodels.bin
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
                        TextBox.AppendText($"\n[{DateTime.Now}] Unable to download .bin files! Missing /data directory!");
                    }

                    if (settings.CSharp == true || settings.Node == true)
                    {
                        if (Directory.Exists($"{settings.Directory}/modules"))
                        {
                            if (settings.CSharp == true)
                            {
                                // csharp-module.dll

                                if (File.Exists($"{settings.Directory}/modules/csharp-module.dll"))
                                {
                                    if (File.Exists($"{settings.Directory}/modules/csharp-module.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{settings.Directory}/modules/csharp-module.dll.{currentVersion}");
                                    }

                                    File.Move($"{settings.Directory}/modules/csharp-module.dll", $"{settings.Directory}/modules/csharp-module.dll.{currentVersion}");
                                }

                                webClient.DownloadFile(
                                    settings.Branch.ToLower() == "alpha"
                                        ? $"https://cdn.altv.mp/coreclr-module/stable/x64_win32/modules/csharp-module.dll"
                                        : $"https://cdn.altv.mp/coreclr-module/{settings.Branch.ToLower()}/x64_win32/modules/csharp-module.dll",
                                    $"{settings.Directory}/modules/csharp-module.dll");

                                // AltV.Net.Host

                                if (File.Exists($"{settings.Directory}/AltV.Net.Host.dll"))
                                {
                                    if (File.Exists($"{settings.Directory}/AltV.Net.Host.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{settings.Directory}/AltV.Net.Host.dll.{currentVersion}");
                                    }

                                    File.Move($"{settings.Directory}/AltV.Net.Host.dll", $"{settings.Directory}/AltV.Net.Host.dll.{currentVersion}");
                                }

                                webClient.DownloadFile(
                                    settings.Branch.ToLower() == "alpha"
                                        ? $"https://cdn.altv.mp/coreclr-module/stable/x64_win32/AltV.Net.Host.dll"
                                        : $"https://cdn.altv.mp/coreclr-module/{settings.Branch.ToLower()}/x64_win32/AltV.Net.Host.dll",
                                    $"{settings.Directory}/AltV.Net.Host.dll");

                                // runtimeconfig.json

                                if (File.Exists($"{settings.Directory}/AltV.Net.Host.runtimeconfig.json"))
                                {
                                    if (File.Exists($"{settings.Directory}/AltV.Net.Host.runtimeconfig.json.{currentVersion}"))
                                    {
                                        File.Delete($"{settings.Directory}/AltV.Net.Host.runtimeconfig.json.{currentVersion}");
                                    }

                                    File.Move($"{settings.Directory}/AltV.Net.Host.runtimeconfig.json", $"{settings.Directory}/AltV.Net.Host.runtimeconfig.json.{currentVersion}");
                                }

                                webClient.DownloadFile(
                                    settings.Branch.ToLower() == "alpha"
                                        ? $"https://cdn.altv.mp/coreclr-module/stable/x64_win32/AltV.Net.Host.runtimeconfig.json"
                                        : $"https://cdn.altv.mp/coreclr-module/{settings.Branch.ToLower()}/x64_win32/AltV.Net.Host.runtimeconfig.json",
                                    $"{settings.Directory}/AltV.Net.Host.runtimeconfig.json");
                            }

                            if (settings.Node == true)
                            {
                                // node-module.dll
                                if (File.Exists($"{settings.Directory}/modules/node-module.dll"))
                                {
                                    if (File.Exists($"{settings.Directory}/modules/node-module.dll.{currentVersion}"))
                                    {
                                        File.Delete($"{settings.Directory}/modules/node-module.dll.{currentVersion}");
                                    }

                                    File.Move($"{settings.Directory}/modules/node-module.dll", $"{settings.Directory}/modules/node-module.dll.{currentVersion}");
                                }
                                webClient.DownloadFile($"https://cdn.altv.mp/node-module/{settings.Branch.ToLower()}/x64_win32/modules/node-module.dll", $"{settings.Directory}/modules/node-module.dll");
                            }
                        }
                        else
                        {
                            TextBox.AppendText($"\n[{DateTime.Now}] Unable to download .dll files! Missing /modules directory!");
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

                    TextBox.AppendText($"\n[{DateTime.Now}] Updated AltV Server to {settings.Branch} Branch - Build #{updateInfo.LatestBuildNumber}");
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

            Setting currentSettings = Setting.FetchSettings();

            ProcessStartInfo altVStartProcess = new ProcessStartInfo($"{currentSettings.Directory}/altv-server.exe");
            altVStartProcess.WorkingDirectory = currentSettings.Directory;

            altVProcess = Process.Start(altVStartProcess);

            if (altVProcess == null || !altVProcess.Responding)
            {
                TextBox.AppendText($"\n[{DateTime.Now}] Error starting the server!");
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
                            TextBox.AppendText($"\n[{DateTime.Now}] Server Started!\nNew build available for branch: {currentSettings.Branch}\nCurrent Build: {currentUpdateInfo.LatestBuildNumber}\nLatest Build: {updateInfo.LatestBuildNumber}");

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
                Setting settings = Setting.FetchSettings();

                int count = 0;

                if (Directory.Exists($"{settings.Directory}\\modules"))
                {
                    foreach (var file in Directory.GetFiles($"{settings.Directory}\\modules"))
                    {
                        if (file.Contains($"{settings.Directory}\\modules\\csharp-module.dll."))
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
                        if (file.Contains($"{settings.Directory}\\modules\\node-module.dll."))
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

                if (Directory.Exists($"{settings.Directory}\\data"))
                {
                    foreach (var file in Directory.GetFiles($"{settings.Directory}\\data"))
                    {
                        if (file.Contains($"{settings.Directory}\\data\\vehmodels.bin."))
                        {
                            File.Delete(file);
                            count++;
                        }
                        if (file.Contains($"{settings.Directory}\\data\\vehmods.bin."))
                        {
                            File.Delete(file);
                            count++;
                        }
                    }
                }

                if (Directory.Exists($"{settings.Directory}"))
                {
                    foreach (var file in Directory.GetFiles($"{settings.Directory}"))
                    {
                        if (file.Contains("altv-server.exe") && file != $"{settings.Directory}\\altv-server.exe")
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