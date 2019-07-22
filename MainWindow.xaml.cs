using System;
using System.Collections.Generic;
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
            Setting settings = Setting.FetchSettings();

            string downloadString = string.Format("https://cdn.altv.mp/server/{0}/x64_win32/", settings.Branch.ToLower());

            using (WebClient webClient = new WebClient())
            {
                if (File.Exists($"{settings.Directory}/altv-server.exe"))
                {
                    if (File.Exists($"{settings.Directory}/altv-server.exe.old"))
                    {
                        File.Delete($"{settings.Directory}/altv-server.exe.old");
                    }

                    File.Move($"{settings.Directory}/altv-server.exe", $"{settings.Directory}/altv-server.exe.old");
                }

                webClient.DownloadFile($"{downloadString}altv-server.exe", $"{settings.Directory}/altv-server.exe");

                if (Directory.Exists($"{settings.Directory}/data"))
                {
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
                            webClient.DownloadFile("https://alt-cdn.s3.nl-ams.scw.cloud/coreclr-module/beta/x64_win32/csharp-module.dll", $"{settings.Directory}/modules/csharp-module.dll");
                        }

                        if (settings.Node == true)
                        {
                            webClient.DownloadFile("https://alt-cdn.s3.nl-ams.scw.cloud/node-module/beta/x64_win32/node-module.dll", $"{settings.Directory}/modules/node-module.dll");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Unable to download .dll files! Missing /modules directory!", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
    }
}