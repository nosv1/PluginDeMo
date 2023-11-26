using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GameReaderCommon;
using SimHub.Plugins;

namespace PluginDeMo_v2
{
    [PluginDescription("")]
    [PluginAuthor("Mo#9991")]
    [PluginName("Plugin de Mo")]
    public class PluginDeMo_v2 : IPlugin, IDataPlugin, IWPFSettings
    {
        public PluginManager PluginManager { get; set; }
        public F1_2023.Session F1_2023_Session { get; set; }

        // property names class
        public class PropertyNames
        {
            public const string PdM_Time = "PdM_Time";
        }

        public class GameNames
        {
            public const string F1_2023 = "F12023";
        }

        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            pluginManager.SetPropertyValue(
                PropertyNames.PdM_Time,
                GetType(),
                DateTime.Now.ToString("HH:mm:ss")
            );

            string gameName = data.GameName;

            if (gameName == GameNames.F1_2023)
            {
                GameData<CodemastersReader.F12023.Packets.F12023TelemetryContainerEx> f1_2023_telemetry_data =
                    data as GameData<CodemastersReader.F12023.Packets.F12023TelemetryContainerEx>;
                GameData<CodemastersReader.F12023.Packets.PacketEventData> f1_2023_event_data =
                    data as GameData<CodemastersReader.F12023.Packets.PacketEventData>;

                bool new_f1_2023_data = f1_2023_telemetry_data.GameNewData != null;
                if (new_f1_2023_data)
                {
                    bool active_cars =
                        f1_2023_telemetry_data
                            .GameNewData
                            .Raw
                            .PacketParticipantsData
                            .m_numActiveCars > 0;
                    if (!active_cars)
                    {
                        F1_2023_Session = null;
                        return;
                    }

                    if (
                        F1_2023_Session == null // f1 2023 session does not exist
                        || F1_2023_Session.PacketLapData.m_header.m_sessionTime == 0 // in game session hasn't begun
                    )
                    {
                        // create the session
                        F1_2023_Session = new F1_2023.Session(
                            pluginManager: pluginManager,
                            numParticipants: f1_2023_telemetry_data
                                .GameNewData
                                .Raw
                                .PacketParticipantsData
                                .m_numActiveCars
                        );
                    }
                    // update the session
                    F1_2023_Session.F1_2023_DataUpdate(
                        pluginManager,
                        ref f1_2023_telemetry_data,
                        ref f1_2023_event_data
                    );
                }
            }
        }

        public void End(PluginManager pluginManager) { }

        public Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new UserControl();
        }

        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("Starting Plugin de Mo 0.2.0");

            pluginManager.AddProperty(PropertyNames.PdM_Time, GetType(), "");
            _ = new F1_2023.Session(pluginManager, 10); // arbitrary number of participants, will be updated in data update, at the time of writing this, needs to be at least > number of rivals player has set
        }
    }
}
