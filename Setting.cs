using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;

namespace AltVUpdate
{
    public class Setting
    {
        public string Directory { get; set; }
        public string Branch { get; set; }
        public bool? CSharp { get; set; }
        public bool? Node { get; set; }

        public Setting()
        {
            Directory = "C:/";
            Branch = "Stable";
            CSharp = true;
            Node = true;
        }

        public static Setting FetchSettings()
        {
            if (!File.Exists("settings.json"))
            {
                Debug.WriteLine("Settings File Not Found");

                Setting newSettings = new Setting();

                using (StreamWriter streamWriter = new StreamWriter("settings.json"))
                {
                    Debug.WriteLine("Settings File Created (StreamWriter)");
                    streamWriter.Write(JsonConvert.SerializeObject(newSettings));
                    Debug.WriteLine("Settings File Written");
                    streamWriter.Dispose();
                    return newSettings;
                }
            }

            using (StreamReader streamReader = new StreamReader("settings.json"))
            {
                Debug.WriteLine("StreamReader found file");
                var jsonSettings = streamReader.ReadToEnd();
                Debug.WriteLine("StreamReader read to end");
                streamReader.Dispose();
                Debug.WriteLine("StreamReader Disposed");
                Setting settings = JsonConvert.DeserializeObject<Setting>(jsonSettings);

                Debug.WriteLine("StreamReader JsonConvert");
                if (settings == null)
                {
                    Debug.WriteLine("Settings Null! Creating new");
                    using (StreamWriter streamWriter = new StreamWriter("settings.json"))
                    {
                        Setting newSettings = new Setting();
                        Debug.WriteLine("Settings File Created (StreamWriter)");
                        streamWriter.Write(JsonConvert.SerializeObject(newSettings));
                        Debug.WriteLine("Settings File Written");
                        streamWriter.Dispose();
                        return newSettings;
                    }
                }

                return settings;
            }
        }

        public static void SaveSettings(Setting settings)
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
        }
    }
}