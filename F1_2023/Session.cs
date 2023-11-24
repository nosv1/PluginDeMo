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
        public bool isPractice =>
            PacketSessionData.m_sessionType >= 1 && PacketSessionData.m_sessionType <= 4;
        public bool isQualifying =>
            PacketSessionData.m_sessionType >= 5 && PacketSessionData.m_sessionType <= 9;
        public bool isRace =>
            PacketSessionData.m_sessionType >= 10 && PacketSessionData.m_sessionType <= 12;

        // session state
        public byte[] LastEventCode => PacketEventData.m_eventStringCode;
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
            Player = new Player(pluginManager, ParticipantCount);

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
            Player.PropertiesUpdate(pluginManager);

            //// update the session ////
            PropertiesUpdate(pluginManager);
        }

        public void PropertiesUpdate(PluginManager pluginManager)
        {
            string namePrefix = Utility.SessionPrefix();

            // duration
            pluginManager.SetPropertyValue(
                $"{namePrefix}TimeLeft",
                TimeLeftFormatted.GetType(),
                TimeLeftFormatted
            );
            pluginManager.SetPropertyValue($"{namePrefix}NumLaps", NumLaps.GetType(), NumLaps);

            // session type
            pluginManager.SetPropertyValue(
                $"{namePrefix}IsPractice",
                isPractice.GetType(),
                isPractice
            );
            pluginManager.SetPropertyValue(
                $"{namePrefix}IsQualifying",
                isQualifying.GetType(),
                isQualifying
            );
            pluginManager.SetPropertyValue($"{namePrefix}IsRace", isRace.GetType(), isRace);
        }

        public static void AddProperties(PluginManager pluginManager)
        {
            string namePrefix = Utility.SessionPrefix(); // PdM_Session_

            // duration
            pluginManager.AddProperty($"{namePrefix}TimeLeft", 1.ToString().GetType(), "00:00");
            pluginManager.AddProperty($"{namePrefix}NumLaps", ((byte)1).GetType(), 0);

            // type
            pluginManager.AddProperty($"{namePrefix}IsPractice", true.GetType(), false);
            pluginManager.AddProperty($"{namePrefix}IsQualifying", true.GetType(), false);
            pluginManager.AddProperty($"{namePrefix}IsRace", true.GetType(), false);
        }
    }
}
