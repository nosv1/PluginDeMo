using Newtonsoft.Json.Linq;

namespace PluginDeMo_v2.F1_2023
{
    public class Settings
    {
        public const string Name = "F1_2023";

        public struct Keys
        {
            public const string PlayerRivals = "PlayerRivals";
        }

        public string[] PlayerRivals { get; set; } = new string[0];

        // {
        //     "F1_2023": {
        //         "PlayerRivals": ["PIASTRI", "LAWSON"]
        //     }
        // }

        public Settings(JObject settingsJObject)
        {
            if (!settingsJObject.ContainsKey(Name))
                return;

            JObject f1_2023JObject = settingsJObject[Name].ToObject<JObject>();

            if (f1_2023JObject.ContainsKey(Keys.PlayerRivals))
                PlayerRivals = f1_2023JObject[Keys.PlayerRivals].ToObject<string[]>();
        }

        public void Load() { }
    }
}
