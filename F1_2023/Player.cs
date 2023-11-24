using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameReaderCommon;
using SimHub.Plugins;

namespace PluginDeMo_v2.F1_2023
{
    public class Player
    {
        public Participant Participant { get; set; }
        public string[] RivalNames { get; set; } = new string[] { "BOTTAS", "VESTI" }; // TODO: all caps, and make this configurable

        public Player(PluginManager pluginManager, int participantCount)
        {
            Participant.AddProperties(pluginManager, isPlayer: true);
            AddProperties(pluginManager, participantCount);
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

        public void PropertiesUpdate(PluginManager pluginManager)
        {
            string namePrefix = Utility.ParticipantPrefix(true, -1);

            // status
            pluginManager.SetPropertyValue(
                $"{namePrefix}Position",
                Participant.Position.GetType(),
                Participant.Position
            );

            // tyre temperatures
            if (Participant.CarTelemetryData.m_tyresInnerTemperature != null)
            {
                string tyreTempPrefix = $"{namePrefix}TyreTemperature_";
                pluginManager.SetPropertyValue(
                    $"{tyreTempPrefix}{TyreSet.Tyres.FrontLeft}",
                    Participant.CarTelemetryData.m_tyresInnerTemperature[2].GetType(),
                    Participant.CarTelemetryData.m_tyresInnerTemperature[2]
                );
                pluginManager.SetPropertyValue(
                    $"{tyreTempPrefix}{TyreSet.Tyres.FrontRight}",
                    Participant.CarTelemetryData.m_tyresInnerTemperature[3].GetType(),
                    Participant.CarTelemetryData.m_tyresInnerTemperature[3]
                );
                pluginManager.SetPropertyValue(
                    $"{tyreTempPrefix}{TyreSet.Tyres.RearLeft}",
                    Participant.CarTelemetryData.m_tyresInnerTemperature[0].GetType(),
                    Participant.CarTelemetryData.m_tyresInnerTemperature[0]
                );
                pluginManager.SetPropertyValue(
                    $"{tyreTempPrefix}{TyreSet.Tyres.RearRight}",
                    Participant.CarTelemetryData.m_tyresInnerTemperature[1].GetType(),
                    Participant.CarTelemetryData.m_tyresInnerTemperature[1]
                );
            }

            // tyre wear
            string tyreWearPrefix = $"{namePrefix}TyreWear_";
            if (Participant.CarDamageData.m_tyresWear != null)
            {
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}{TyreSet.Tyres.FrontLeft}",
                    Participant.CarDamageData.m_tyresWear[2].GetType(),
                    Participant.CarDamageData.m_tyresWear[2]
                );
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}{TyreSet.Tyres.FrontRight}",
                    Participant.CarDamageData.m_tyresWear[3].GetType(),
                    Participant.CarDamageData.m_tyresWear[3]
                );
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}{TyreSet.Tyres.RearLeft}",
                    Participant.CarDamageData.m_tyresWear[0].GetType(),
                    Participant.CarDamageData.m_tyresWear[0]
                );
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}{TyreSet.Tyres.RearRight}",
                    Participant.CarDamageData.m_tyresWear[1].GetType(),
                    Participant.CarDamageData.m_tyresWear[1]
                );
            }
            if (Participant.CurrentTyreSet != null)
            {
                // tyre wear
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}LastLap",
                    Participant.CurrentTyreSet.WearLastLap.GetType(),
                    Participant.CurrentTyreSet.WearLastLap
                );
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}AveragePerLap",
                    Participant.CurrentTyreSet.AverageWearPerLap.GetType(),
                    Participant.CurrentTyreSet.AverageWearPerLap
                );

                // pit stop window
                pluginManager.SetPropertyValue(
                    $"{namePrefix}PitStopWindowIdealLap",
                    Participant.CurrentTyreSet.PitStopWindowIdealLap.GetType(),
                    Participant.CurrentTyreSet.PitStopWindowIdealLap
                );
                pluginManager.SetPropertyValue(
                    $"{namePrefix}PitStopWindowLatestLap",
                    Participant.CurrentTyreSet.PitStopWindowLatestLap.GetType(),
                    Participant.CurrentTyreSet.PitStopWindowLatestLap
                );
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}AtIdealPitLap",
                    Participant.CurrentTyreSet.WearAtIdealPitLap.GetType(),
                    Participant.CurrentTyreSet.WearAtIdealPitLap
                );
                pluginManager.SetPropertyValue(
                    $"{tyreWearPrefix}AtLatestPitLap",
                    Participant.CurrentTyreSet.WearAtLatestPitLap.GetType(),
                    Participant.CurrentTyreSet.WearAtLatestPitLap
                );
            }

            // car status
            string carStatusPrefix = $"{namePrefix}CarStatus_";

            // fuel
            string fuelPrefix = $"{carStatusPrefix}Fuel_";
            if (Participant.Fuel != null)
            {
                pluginManager.SetPropertyValue(
                    $"{fuelPrefix}ConsumedLastLap",
                    Participant.Fuel.FuelConsumedLastLap.GetType(),
                    Participant.Fuel.FuelConsumedLastLap
                );
                pluginManager.SetPropertyValue(
                    $"{fuelPrefix}AveragePerLap",
                    Participant.Fuel.AverageFuelConsumedPerLap.GetType(),
                    Participant.Fuel.AverageFuelConsumedPerLap
                );
                pluginManager.SetPropertyValue(
                    $"{fuelPrefix}Remaining",
                    Participant.Fuel.FuelAtEndOfStint.GetType(),
                    Participant.Fuel.FuelAtEndOfStint
                );
            }

            // deltas + rivals
            string deltaPrefix = $"{namePrefix}DeltaTo_";
            string deltaToPositionPrefix = $"{deltaPrefix}Position_";
            string rivalPrefix = $"{namePrefix}Rival_";
            for (int i = 0; i < Participant.Session.ParticipantCount; i++)
            {
                Participant participant = Participant.Session.ParticipantsByPosition[i];
                float deltaToPosition =
                    participant.DeltaToLeaderInSeconds - Participant.DeltaToLeaderInSeconds;
                string deltaToPositionString =
                    deltaToPosition > 0
                        ? $"+{deltaToPosition.ToString("0.0")}" // +0.0
                        : deltaToPosition.ToString("0.0"); // -0.0
                deltaToPositionString = deltaToPosition == 0 ? "" : deltaToPositionString; // blank if participant is player
                pluginManager.SetPropertyValue(
                    // PdM_Player_DeltaTo_Position_01
                    $"{deltaToPositionPrefix}{(i + 1).ToString().PadLeft(2, '0')}",
                    deltaToPositionString.GetType(),
                    deltaToPositionString
                );

                // rivals
                bool participantIsRival = RivalNames.Contains(participant.Name);
                if (participantIsRival)
                {
                    int rivalNumber = Array.IndexOf(RivalNames, participant.Name) + 1;
                    pluginManager.SetPropertyValue(
                        $"{rivalPrefix}{rivalNumber}_Position",
                        participant.Position.GetType(),
                        participant.Position
                    );
                }
            }
        }

        public void AddProperties(PluginManager pluginManager, int participantCount)
        {
            string namePrefix = Utility.ParticipantPrefix(true, -1);

            // status
            pluginManager.AddProperty($"{namePrefix}Position", ((byte)1).GetType(), 0);

            //// tyres ////
            string tyreTempPrefix = $"{namePrefix}TyreTemperature_";
            string tyreWearPrefix = $"{namePrefix}TyreWear_";
            foreach (string tyre in TyreSet.TyresArray)
            {
                pluginManager.AddProperty($"{tyreTempPrefix}{tyre}", ((byte)1).GetType(), 0); // tyre temp
                pluginManager.AddProperty($"{tyreWearPrefix}{tyre}", ((float)1).GetType(), 0); // tyre wear
            }
            pluginManager.AddProperty($"{tyreWearPrefix}LastLap", ((float)1).GetType(), 0); // tyre wear last lap
            pluginManager.AddProperty($"{tyreWearPrefix}AveragePerLap", ((float)1).GetType(), 0); // tyre wear average per lap

            // pit stop window
            pluginManager.AddProperty($"{namePrefix}PitStopWindowIdealLap", ((byte)1).GetType(), 0);
            pluginManager.AddProperty(
                $"{namePrefix}PitStopWindowLatestLap",
                ((byte)1).GetType(),
                0
            );
            pluginManager.AddProperty($"{tyreWearPrefix}AtIdealPitLap", ((float)1).GetType(), 0); // tyre wear at ideal pit lap
            pluginManager.AddProperty($"{tyreWearPrefix}AtLatestPitLap", ((float)1).GetType(), 0); // tyre wear at latest pit lap

            //// car status ////
            string carStatusPrefix = $"{namePrefix}CarStatus_";
            //// fuel ////
            string fuelPrefix = $"{carStatusPrefix}Fuel_";
            pluginManager.AddProperty($"{fuelPrefix}ConsumedLastLap", ((float)1).GetType(), 0); // fuel consumed last lap
            pluginManager.AddProperty($"{fuelPrefix}AveragePerLap", ((float)1).GetType(), 0); // fuel consumed average per lap
            pluginManager.AddProperty($"{fuelPrefix}Remaining", ((float)1).GetType(), 0); // fuel remaining

            //// deltas ////
            string deltaPrefix = $"{namePrefix}DeltaTo_";
            string deltaToPositionPrefix = $"{deltaPrefix}Position_";
            for (int i = 0; i < participantCount; i++)
            {
                pluginManager.AddProperty(
                    // PdM_Player_DeltaTo_Position_01
                    $"{deltaToPositionPrefix}{(i + 1).ToString().PadLeft(2, '0')}",
                    "".GetType(),
                    "+0.0"
                );
            }

            //// rivals ////
            string rivalPrefix = $"{namePrefix}Rival_";
            for (int i = 1; i <= 2; i++)
            {
                pluginManager.AddProperty($"{rivalPrefix}{i}_Position", ((byte)1).GetType(), 0); // PdM_Player_Rival_#_Position
            }
        }
    }
}
