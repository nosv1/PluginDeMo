using System;
using System.Collections.Generic;
using System.Linq;
using SimHub.Plugins;
using F12023_Packets = CodemastersReader.F12023.Packets;

namespace PluginDeMo_v2.F1_2023
{
    public class Participant
    {
        public Session Session { get; set; }
        public int Index { get; set; } // index in the packet arrays
        public List<Property<object>> Properties { get; set; } = new List<Property<object>>();

        // packets
        public F12023_Packets.ParticipantData ParticipantData { get; set; } // every 5 seconds
        public F12023_Packets.CarTelemetryData CarTelemetryData { get; set; } // rate specified in menus
        public F12023_Packets.CarDamageData CarDamageData { get; set; } // 10 per second
        public F12023_Packets.CarStatusData CarStatusData { get; set; } // rate specified in menus
        public F12023_Packets.CarSetupData CarSetupData { get; set; } // 2 per second
        public F12023_Packets.PacketTyreSetsData PacketTyreSetsData { get; set; } // 20 per second cycling through cars (so 1 per second per car if 20 cars?)
        public F12023_Packets.PacketSessionHistoryData PacketSessionHistoryData { get; set; } // 20 per second cycling through cars (so 1 per second per car if 20 cars?)
        public F12023_Packets.TyreSetData CurrentTyreSetData { get; set; } // can detect change when packetTyreSetsData.m_fittedIndx changes
        public F12023_Packets.LapData LapData { get; set; } // rate specified in menus

        // participant data
        public TyreSet CurrentTyreSet { get; set; }
        public TyreSet PreviousTyreSet { get; set; }
        public Fuel Fuel { get; set; }
        public string Name => Utility.GetStringFromCharArray(ParticipantData.m_name);
        public string AbbreviatedName => Name.Substring(0, Math.Min(Name.Length, 3)).ToUpper(); // Hamilton -> HAM
        public int TeamId => ParticipantData.m_teamId;

        // participant status
        public int CurrentLapNumber { get; set; }
        public byte Position => (byte)Math.Max(1, (int)LapData.m_carPosition);
        public byte NumPitStops => LapData.m_numPitStops;
        public byte PenaltiesInSeconds => LapData.m_penalties; // accumulated time penalties in seconds to be added
        public bool IsPitting => LapData.m_pitStatus >= 1; // 0 = none, 1 = pitting, 2 = in pit area
        public bool InGarage => LapData.m_driverStatus == 0;
        public bool FlyingLap => LapData.m_driverStatus == 1;
        public bool InLap => LapData.m_driverStatus == 2;
        public bool OutLap => LapData.m_driverStatus == 3;
        public bool OnTrack => LapData.m_driverStatus == 4;

        // participant pace
        public const int AVERAGE_LAP_NUM_LAPS = 3; // number of laps to average for average lap time
        public uint BestLapTime =>
            PacketSessionHistoryData
                .m_lapHistoryData[PacketSessionHistoryData.m_bestLapTimeLapNum - 1]
                .m_lapTimeInMS; // in ms
        public uint BestLapPlus10Percent => (uint)(BestLapTime * 1.1f); // in ms
        public uint LastThreeLapAverageLapTime { get; set; } = 0; // average of last 3 laps in ms
        public string LastThreeLapAverageLapTimeFormatted =>
            Utility.SecondsToTimeString(LastThreeLapAverageLapTime / 1000f, @"m\:ss\.fff");
        public int AverageLapLastCalculatedOnLap { get; set; } = 0; // lap number when average lap was last calculated
        public uint DeltaToLeader => LapData.m_deltaToRaceLeaderInMS;
        public float DeltaToLeaderInSeconds => DeltaToLeader / 1000f;

        public Participant(Session session, PluginManager pluginManager, int index = -1)
        {
            Session = session;
            AddProperties(pluginManager, isPlayer: false, index: index);
        }

        ///// PACKET UPDATES /////

        public void ParticpantUpdate(
            F12023_Packets.ParticipantData packetParticipantData,
            int index
        )
        {
            ParticipantData = packetParticipantData;
            Index = index;
        }

        public void CarTelemetryUpdate(F12023_Packets.CarTelemetryData packetCarTelemetryData)
        {
            CarTelemetryData = packetCarTelemetryData;
        }

        public void CarDamageUpdate(F12023_Packets.CarDamageData packetCarDamageData)
        {
            CarDamageData = packetCarDamageData;
        }

        public void TyreSetUpdate(F12023_Packets.PacketTyreSetsData packetTyreSetsData)
        {
            bool hasTyreSetdata = PacketTyreSetsData.m_tyreSetData != null;
            bool tyreSetChanged =
                hasTyreSetdata && PacketTyreSetsData.m_fittedIdx != packetTyreSetsData.m_fittedIdx;

            PacketTyreSetsData = packetTyreSetsData;
            CurrentTyreSetData = PacketTyreSetsData.m_tyreSetData[PacketTyreSetsData.m_fittedIdx];
            UpdateTyreSet(tyreSetChanged);
        }

        public void LapDataUpdate(F12023_Packets.LapData packetLapData)
        {
            LapData = packetLapData;
            CurrentLapNumber = LapData.m_currentLapNum;
        }

        public void CarStatusUpdate(F12023_Packets.CarStatusData packetCarStatusData)
        {
            CarStatusData = packetCarStatusData;
            UpdateFuel();
        }

        public void SessionHistoryUpdate(
            F12023_Packets.PacketSessionHistoryData packetSessionHistoryData
        )
        {
            PacketSessionHistoryData = packetSessionHistoryData;

            // only update if PacketSessionHistoryData.m_lapHistoryData[CurrentLapNumber - 1] == 0 and CurrentLapNumber - 2 > 0
            // basically if the lap history data is 0 and the previous lap is not 0
            bool firstLapCompleted = CurrentLapNumber > 1;
            if (!firstLapCompleted)
                return;

            bool previousLapCompleted =
                PacketSessionHistoryData.m_lapHistoryData[CurrentLapNumber - 2].m_lapTimeInMS != 0;
            bool currentLapNotCompleted =
                PacketSessionHistoryData.m_lapHistoryData[CurrentLapNumber - 1].m_lapTimeInMS == 0;
            bool outOfDateAverageLap = CurrentLapNumber > AverageLapLastCalculatedOnLap; // if the average lap is out of date
            if (previousLapCompleted && currentLapNotCompleted && outOfDateAverageLap)
            {
                UpdateAverageLapTime(AVERAGE_LAP_NUM_LAPS);
                AverageLapLastCalculatedOnLap = CurrentLapNumber;
            }
        }

        ///// ATTRIBUTE UPDATES /////

        public void UpdateAverageLapTime(int numPreviousLaps)
        {
            List<int> lapTimes = new List<int>();
            int endIndex = CurrentLapNumber - 1; // < end index is exclusive
            int startLap = Math.Max(0, endIndex - numPreviousLaps);

            for (int i = startLap; i < endIndex; i++)
            {
                uint lapTime = PacketSessionHistoryData.m_lapHistoryData[i].m_lapTimeInMS;
                // ignoring laps that are slower than best lap + 10% for accuracy sake
                bool within10Percent = lapTime < BestLapPlus10Percent;
                if (within10Percent)
                    lapTimes.Add((int)lapTime);
            }
            if (lapTimes.Count > 0)
                LastThreeLapAverageLapTime = (uint)lapTimes.Average();
        }

        public void UpdateTyreSet(bool tyreSetChanged)
        {
            if (CurrentTyreSet == null)
                CurrentTyreSet = new TyreSet(this);
            else if (tyreSetChanged)
            {
                if (LapData.m_totalDistance < CurrentTyreSet.OdometerAtFitting)
                {
                    // this handles the case where a flash back happens right after a pit stop
                    // we switch back to the previous tyre set before the pit stop (and before the flashback)
                    TyreSet previousTyreSet = PreviousTyreSet;
                    CurrentTyreSet = PreviousTyreSet;
                    PreviousTyreSet = previousTyreSet?.PreviousTyreSet;
                }
                else
                {
                    // normal tyre set change, from garage or pit stop
                    PreviousTyreSet = CurrentTyreSet;
                    CurrentTyreSet = new TyreSet(this, PreviousTyreSet);
                }
            }
            CurrentTyreSet?.Update();
        }

        public void UpdateFuel()
        {
            // fuel should be set anytime the car is 'refueled' this only happens when it's the garage or before a car starts a
            // race, so we reset it anytime the car is in the garage or when the fuel is null
            if (InGarage)
                Fuel = null;
            else if (Fuel == null)
                Fuel = new Fuel(this);
            Fuel?.Update();
        }

        ///// PROPERTIES /////
        public void UpdateProperties(bool isPlayer = false)
        {
            string namePrefix = Utility.ParticipantPrefix(isPlayer: true, index: Position);
            foreach (Property<object> property in Properties)
            {
                property.Prefix = namePrefix;
                property.Update();

                // this allows to update the player variant and the participant at the same rate
                // we're basically undoing the update so the participant variant can update too
                if (isPlayer)
                    property.LastUpdated -= TimeSpan.FromMilliseconds(property.UpdateRate);
            }

            if (isPlayer)
                UpdateProperties(isPlayer: false);
        }

        public void AddProperties(PluginManager pluginManager, bool isPlayer, int index = -1)
        {
            // index is defaulted to -1 so that we don't have to pass it in when adding the player properties
            string namePrefix = Utility.ParticipantPrefix(isPlayer: isPlayer, index: index + 1); // when we set properties they're based on position, so we add 1 to the index

            Properties = new List<Property<object>>()
            {
                //// participant data ////
                new Property<object>( // participant name
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "Name",
                    pluginType: typeof(string),
                    valueFunc: () => Session.ParticipantsByPosition[Position - 1]?.Name,
                    updateRate: 1000
                ),
                new Property<object>( // abbreviated participant name
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "AbbreviatedName",
                    pluginType: typeof(string),
                    valueFunc: () => Session.ParticipantsByPosition[Position - 1]?.AbbreviatedName,
                    updateRate: 1000
                ),
                new Property<object>( // current lap number
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "CurrentLapNumber",
                    pluginType: typeof(int),
                    valueFunc: () => Session.ParticipantsByPosition[Position - 1]?.CurrentLapNumber,
                    updateRate: 1000
                ),
                //// participant status ////
                new Property<object>( // stint lap
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "StintLap",
                    pluginType: typeof(float),
                    valueFunc: () =>
                        Session
                            .ParticipantsByPosition[Position - 1]
                            ?.CurrentTyreSet
                            ?.LapsSinceFitting,
                    updateRate: 1000
                ),
                new Property<object>( // artificial predicted pit lap
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "ArtificialPredictedPitLap",
                    pluginType: typeof(byte),
                    valueFunc: () =>
                        Session
                            .ParticipantsByPosition[Position - 1]
                            ?.CurrentTyreSet
                            ?.ArtificialPredictedPitLap,
                    updateRate: 1000
                ),
                new Property<object>( // visual tyre name
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "VisualTyreName",
                    pluginType: typeof(string),
                    valueFunc: () =>
                        Session
                            .ParticipantsByPosition[Position - 1]
                            ?.CurrentTyreSet
                            ?.VisualTyreName,
                    updateRate: 1000
                ),
                //// participant pace ////
                new Property<object>( // average lap time
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "AverageLapTime",
                    pluginType: typeof(string),
                    valueFunc: () =>
                        Session
                            .ParticipantsByPosition[Position - 1]
                            ?.LastThreeLapAverageLapTimeFormatted,
                    updateRate: 1000
                ),
                new Property<object>( // delta to leader
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "DeltaToLeader",
                    pluginType: typeof(float),
                    valueFunc: () =>
                        Session.ParticipantsByPosition[Position - 1]?.DeltaToLeaderInSeconds,
                    updateRate: 500
                ),
                //// participant pit stop ////
                new Property<object>( // num pit stops
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "NumPitStops",
                    pluginType: typeof(byte),
                    valueFunc: () => Session.ParticipantsByPosition[Position - 1]?.NumPitStops,
                    updateRate: 1000
                ),
                new Property<object>( // penalties in seconds
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "PenaltiesInSeconds",
                    pluginType: typeof(byte),
                    valueFunc: () =>
                        Session.ParticipantsByPosition[Position - 1]?.PenaltiesInSeconds,
                    updateRate: 1000
                ),
                new Property<object>( // is pitting
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "IsPitting",
                    pluginType: typeof(bool),
                    valueFunc: () => Session.ParticipantsByPosition[Position - 1]?.IsPitting,
                    updateRate: 1000
                ),
                //// participant telemetry ////
            };
        }
    }
}
