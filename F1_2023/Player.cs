using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GameReaderCommon;
using SimHub.Plugins;

namespace PluginDeMo_v2.F1_2023
{
    public class Player
    {
        public Participant Participant { get; set; }
        public List<Property<object>> Properties { get; set; } = new List<Property<object>>();
        public string[] RivalNames { get; set; } = new string[] { "BOTTAS", "VESTI" }; // TODO: all caps, and make this configurable
        public List<Participant> Rivals { get; set; } = new List<Participant>();

        public Player(PluginManager pluginManager, Participant tempParticipant)
        {
            Participant = tempParticipant;
            Participant.AddProperties(pluginManager, isPlayer: true);
            AddProperties(pluginManager);
        }

        // function to return a sorted order of participants based on relative lap_distance from player
        public Participant[] SortParticipantsByRelativeLapDistance()
        {
            // get the participants
            Participant[] participants = Participant.Session.Participants;
            int participantCount = Participant.Session.ParticipantCount;

            // get the player
            Participant player = Participant.Session.Player.Participant;

            // get the relative lap distances
            float[] relativeLapDistances = new float[participantCount];
            for (int i = 0; i < participantCount; i++)
            {
                Participant participant = participants[i];
                relativeLapDistances[i] =
                    participant.LapData.m_lapDistance - player.LapData.m_lapDistance;
            }

            // sort the participants
            Participant[] sortedParticipants = new Participant[participantCount];
            for (int i = 0; i < participantCount; i++)
            {
                int index = Array.IndexOf(relativeLapDistances, relativeLapDistances.Min());
                sortedParticipants[i] = participants[index];
                relativeLapDistances[index] = float.MaxValue;
            }

            return sortedParticipants;
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
            string namePrefix = Utility.ParticipantPrefix(true, -1);

            Properties.Add(
                new Property<object>( // participant name
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "Position",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.Position,
                    updateRate: 1000
                )
            );

            //// tyres ////
            string tyreTempPrefix = $"{namePrefix}TyreTemperature_";
            string tyreWearPrefix = $"{namePrefix}TyreWear_";
            foreach (string tyre in TyreSet.TyresArray)
            {
                Properties.Add(
                    new Property<object>( // tyre temp
                        pluginManager: pluginManager,
                        prefix: tyreTempPrefix,
                        suffix: tyre,
                        pluginType: typeof(byte),
                        valueFunc: () =>
                            Participant
                                .CarTelemetryData
                                .m_tyresInnerTemperature
                                ?[Array.IndexOf(TyreSet.TyresArray, tyre)],
                        updateRate: 1000
                    )
                );
                Properties.Add(
                    new Property<object>( // tyre wear
                        pluginManager: pluginManager,
                        prefix: tyreWearPrefix,
                        suffix: tyre,
                        pluginType: typeof(float),
                        valueFunc: () =>
                            Participant
                                .CarDamageData
                                .m_tyresWear
                                ?[Array.IndexOf(TyreSet.TyresArray, tyre)],
                        updateRate: 1000
                    )
                );
            }
            Properties.Add(
                new Property<object>( // tyre wear last lap
                    pluginManager: pluginManager,
                    prefix: tyreWearPrefix,
                    suffix: "LastLap",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CurrentTyreSet?.WearLastLap,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // tyre wear average per lap
                    pluginManager: pluginManager,
                    prefix: tyreWearPrefix,
                    suffix: "AveragePerLap",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CurrentTyreSet?.AverageWearPerLap,
                    updateRate: 1000
                )
            );

            // pit stop window
            Properties.Add(
                new Property<object>( // ideal pit lap
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "PitStopWindowIdealLap",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CurrentTyreSet?.PitStopWindowIdealLap,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // latest pit lap
                    pluginManager: pluginManager,
                    prefix: namePrefix,
                    suffix: "PitStopWindowLatestLap",
                    pluginType: typeof(byte),
                    valueFunc: () => Participant.CurrentTyreSet?.PitStopWindowLatestLap,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // tyre wear at ideal pit lap
                    pluginManager: pluginManager,
                    prefix: tyreWearPrefix,
                    suffix: "AtIdealPitLap",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CurrentTyreSet?.WearAtIdealPitLap,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // tyre wear at latest pit lap
                    pluginManager: pluginManager,
                    prefix: tyreWearPrefix,
                    suffix: "AtLatestPitLap",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.CurrentTyreSet?.WearAtLatestPitLap,
                    updateRate: 1000
                )
            );

            //// car status ////
            string carStatusPrefix = $"{namePrefix}CarStatus_";

            //// fuel ////
            string fuelPrefix = $"{carStatusPrefix}Fuel_";
            Properties.Add(
                new Property<object>( // fuel consumed last lap
                    pluginManager: pluginManager,
                    prefix: fuelPrefix,
                    suffix: "ConsumedLastLap",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.Fuel?.FuelConsumedLastLap,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // fuel consumed average per lap
                    pluginManager: pluginManager,
                    prefix: fuelPrefix,
                    suffix: "AveragePerLap",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.Fuel?.AverageFuelConsumedPerLap,
                    updateRate: 1000
                )
            );
            Properties.Add(
                new Property<object>( // fuel remaining
                    pluginManager: pluginManager,
                    prefix: fuelPrefix,
                    suffix: "Remaining",
                    pluginType: typeof(float),
                    valueFunc: () => Participant.Fuel?.FuelAtEndOfStint,
                    updateRate: 1000
                )
            );

            //// deltas + rivals ////
            string deltaPrefix = $"{namePrefix}DeltaTo_";
            string deltaToPositionPrefix = $"{deltaPrefix}Position_";
            for (int i = 0; i < Participant.Session.ParticipantCount; i++)
            {
                Participant participant = Participant.Session.Participants[i];
                Properties.Add(
                    new Property<object>( // PdM_Player_DeltaTo_Position_01
                        pluginManager: pluginManager,
                        prefix: deltaToPositionPrefix,
                        suffix: $"{(i + 1).ToString().PadLeft(2, '0')}",
                        pluginType: typeof(float),
                        valueFunc: () =>
                            Participant
                                .Session
                                .ParticipantsByPosition[participant.Index]
                                .DeltaToLeaderInSeconds - Participant.DeltaToLeaderInSeconds,
                        updateRate: 500
                    )
                );
            }

            string rivalPrefix = $"{namePrefix}Rival_";
            foreach (string rivalName in RivalNames)
            {
                int rivalIndex = Array.IndexOf(RivalNames, rivalName);
                Properties.Add(
                    new Property<object>( // PdM_Player_Rival_01
                        pluginManager: pluginManager,
                        prefix: $"{rivalPrefix}{rivalIndex + 1}_",
                        suffix: "Position",
                        pluginType: typeof(byte),
                        valueFunc: () =>
                        // needs to be a loop because Participant names aren't always known, espically at the start of the plugin
                        {
                            foreach (Participant participant in Participant.Session.Participants)
                            {
                                if (participant.Name == rivalName)
                                    return participant.Position;
                            }
                            return 0;
                        },
                        updateRate: 500
                    )
                );
            }
        }
    }
}
