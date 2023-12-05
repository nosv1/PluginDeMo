using System.IO;
using Newtonsoft.Json.Linq;

namespace PluginDeMo_v2
{
    public class Settings
    {
        public string CurrentDirectory { get; set; }
        public const string FileName = "DashTemplates\\PdM_Settings\\Settings.json";
        public string Path => System.IO.Path.Combine(CurrentDirectory, FileName);

        //// settings classes ////
        public F1_2023.Settings F1_2023 { get; set; }

        public Settings(string currentDirectory)
        {
            CurrentDirectory = currentDirectory;
            JObject settingsJObject = JObject.Parse(File.ReadAllText(Path));

            F1_2023 = new F1_2023.Settings(settingsJObject);
        }
    }
}
