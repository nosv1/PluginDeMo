using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodemastersReader;
using GameReaderCommon;
using SimHub.Plugins;
using EventData = CodemastersReader.F12023.Packets.PacketEventData;
using F12023_Packets = CodemastersReader.F12023.Packets;
using TelemetryContainerEx = CodemastersReader.F12023.Packets.F12023TelemetryContainerEx;

namespace PluginDeMo_v2.F1_2023
{
    public class Session
    {
        public int ParticipantCount { get; set; }
        public Participant[] Participants { get; set; }
        public Participant[] ParticipantsByPosition { get; set; }
        public Player Player { get; set; }
        public List<Property<object>> Properties { get; set; } = new List<Property<object>>();

        // session data
        public F12023_Packets.PacketSessionData PacketSessionData { get; set; } // 2 per second
        public F12023_Packets.PacketLapData PacketLapData { get; set; } // rate specified in menus
        public F12023_Packets.PacketEventData PacketEventData { get; set; }

        // session duration
        public int TimeLeft => PacketSessionData.m_sessionTimeLeft; // in seconds
        public string TimeLeftFormatted => Utility.SecondsToTimeString(TimeLeft, @"mm\:ss");
        public byte NumLaps => PacketSessionData.m_totalLaps;

        // session type
        // uint8           m_sessionType;       // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P
        //                                      // 5 = Q1, 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ
        //                                      // 10 = R, 11 = R2, 12 = R3, 13 = Time Trial
        public bool IsPractice =>
            PacketSessionData.m_sessionType >= 1 && PacketSessionData.m_sessionType <= 4;
        public bool IsQualifying =>
            PacketSessionData.m_sessionType >= 5 && PacketSessionData.m_sessionType <= 9;
        public bool IsRace =>
            PacketSessionData.m_sessionType >= 10 && PacketSessionData.m_sessionType <= 12;
        public string SessionType => SessionTypes[PacketSessionData.m_sessionType];
        public string Track => TrackIds[PacketSessionData.m_trackId];

        // session state
        public byte[] LastEventCode => PacketEventData.m_eventStringCode;
        public bool SessionStarted =>
            LastEventCode != null && Utility.GetStringFromByteArray(LastEventCode) == "SSTA";
        public bool SessionEnded =>
            LastEventCode != null && Utility.GetStringFromByteArray(LastEventCode) == "SEND";

        public Session(PluginManager pluginManager, int numParticipants)
        {
            ParticipantCount = numParticipants;

            // create the particpants
            Participants = new Participant[ParticipantCount];
            ParticipantsByPosition = new Participant[ParticipantCount];
            for (int i = 0; i < ParticipantCount; i++)
            {
                Participants[i] = new Participant(this, pluginManager, i);
                ParticipantsByPosition[i] = Participants[i]; // not the actual position of participant yet, that gets set in the data update
            }

            // create the player
            Player = new Player(pluginManager: pluginManager, tempParticipant: Participants[0]);

            AddProperties(pluginManager);
        }

        public void F1_2023_DataUpdate(
            PluginManager pluginManager,
            ref GameData<TelemetryContainerEx> telemetry_data,
            ref GameData<EventData> event_data
        )
        {
            //// get the packets ////
            F12023_Packets.PacketParticipantsData packetParticipantsData = telemetry_data
                .GameNewData
                .Raw
                .PacketParticipantsData;
            F12023_Packets.PacketCarTelemetryData packetCarTelemetryData = telemetry_data
                .GameNewData
                .Raw
                .PacketCarTelemetryData;
            F12023_Packets.PacketCarDamageData packetCarDamageData = telemetry_data
                .GameNewData
                .Raw
                .PacketCarDamageData;
            F12023_Packets.PacketCarStatusData packetCarStatusData = telemetry_data
                .GameNewData
                .Raw
                .PacketCarStatusData;

            Dictionary<int, F12023_Packets.PacketTyreSetsData> packetTyreSetsData = telemetry_data
                .GameNewData
                .Raw
                .PacketTyreSetsData;
            Dictionary<int, F12023_Packets.PacketSessionHistoryData> packetSessionHistoryData =
                telemetry_data.GameNewData.Raw.PacketSessionHistoryData;

            PacketSessionData = telemetry_data.GameNewData.Raw.PacketSessionData;
            PacketLapData = telemetry_data.GameNewData.Raw.PacketLapData;
            if (event_data != null)
                PacketEventData = event_data.GameNewData.Raw;

            // player index
            int playerIndex = packetParticipantsData.m_header.m_playerCarIndex; // assuming the player index doesn't change...

            //// update the particpants ////

            for (int i = 0; i < ParticipantCount; i++)
            {
                Participants[i].ParticpantUpdate(packetParticipantsData.m_participants[i], i);
                Participants[i].CarTelemetryUpdate(packetCarTelemetryData.m_carTelemetryData[i]);
                Participants[i].CarDamageUpdate(packetCarDamageData.m_carDamageData[i]);
                Participants[i].LapDataUpdate(PacketLapData.m_lapData[i]);
                Participants[i].CarStatusUpdate(packetCarStatusData.m_carStatusData[i]);

                if (packetTyreSetsData.ContainsKey(i))
                    Participants[packetTyreSetsData[i].m_carIdx].TyreSetUpdate(
                        packetTyreSetsData[i]
                    );

                if (packetSessionHistoryData.ContainsKey(i))
                    Participants[packetSessionHistoryData[i].m_carIdx].SessionHistoryUpdate(
                        packetSessionHistoryData[i]
                    );

                // Participants[i].PropertiesUpdate(pluginManager, i == playerIndex);
                Participants[i].UpdateProperties(isPlayer: i == playerIndex);
                ParticipantsByPosition[Participants[i].Position - 1] = Participants[i];
            }

            //// update the player ////
            Player.Participant = Participants[playerIndex];
            Player.UpdateProperties();

            //// update player's teammate ////
            Player.Teammate.UpdateProperties();

            //// update the session ////
            UpdateProperties();
        }

        public void UpdateProperties()
        {
            foreach (Property<object> property in Properties)
            {
                property.Update();
            }
        }

        public void AddProperties(PluginManager pluginManager)
        {
            string namePrefix = Utility.SessionPrefix(); // PdM_Session_

            //// session duration ////
            Properties.Add(
                new Property<object>( // session time left
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "TimeLeft",
                    pluginType: typeof(string),
                    valueFunc: () => TimeLeftFormatted,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // session num laps
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "NumLaps",
                    pluginType: typeof(byte),
                    valueFunc: () => NumLaps,
                    updateRate: 1000
                )
            );

            //// session type ////
            Properties.Add(
                new Property<object>( // session is practice
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "IsPractice",
                    pluginType: typeof(bool),
                    valueFunc: () => IsPractice,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // session is qualifying
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "IsQualifying",
                    pluginType: typeof(bool),
                    valueFunc: () => IsQualifying,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // session is race
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "IsRace",
                    pluginType: typeof(bool),
                    valueFunc: () => IsRace,
                    updateRate: 1000
                )
            );
        }

        public Dictionary<int, string> SessionTypes { get; set; } =
            new Dictionary<int, string>
            {
                { 0, "Unknown" },
                { 1, "P1" },
                { 2, "P2" },
                { 3, "P3" },
                { 4, "Short P" },
                { 5, "Q1" },
                { 6, "Q2" },
                { 7, "Q3" },
                { 8, "Short Q" },
                { 9, "OSQ" },
                { 10, "R" },
                { 11, "R2" },
                { 12, "R3" },
                { 13, "Time Trial" }
            };

        public Dictionary<int, string> TrackIds { get; set; } =
            new Dictionary<int, string>
            {
                { 0, "Melbourne" },
                { 1, "Paul Ricard" },
                { 2, "Shanghai" },
                { 3, "Sakhir (Bahrain)" },
                { 4, "Catalunya" },
                { 5, "Monaco" },
                { 6, "Montreal" },
                { 7, "Silverstone" },
                { 8, "Hockenheim" },
                { 9, "Hungaroring" },
                { 10, "Spa" },
                { 11, "Monza" },
                { 12, "Singapore" },
                { 13, "Suzuka" },
                { 14, "Abu Dhabi" },
                { 15, "Texas" },
                { 16, "Brazil" },
                { 17, "Austria" },
                { 18, "Sochi" },
                { 19, "Mexico" },
                { 20, "Baku (Azerbaijan)" },
                { 21, "Sakhir Short" },
                { 22, "Silverstone Short" },
                { 23, "Texas Short" },
                { 24, "Suzuka Short" },
                { 25, "Hanoi" },
                { 26, "Zandvoort" },
                { 27, "Imola" },
                { 28, "Portimão" },
                { 29, "Jeddah" },
                { 30, "Miami" },
                { 31, "Las Vegas" },
                { 32, "Losail" }
            };
    }
}
