using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginDeMo_v2.F1_2023.Participants
{
    public class TyreSet
    {
        // Participant-related variables
        Participant Participant { get; set; }
        public TyreSet PreviousTyreSet { get; set; } = null;

        // Variables related to fitting
        public float OdometerAtFitting { get; set; }
        public float TyreWearAtFitting { get; set; }
        public int LapNumberAtFitting { get; set; } = 0;
        public float LapsSinceFitting =>
            (CurrentLapNumber - LapNumberAtFitting) // laps since fitting
            + (Participant.LapData.m_lapDistance / TrackLength); // fraction of current lap

        // Variables related to wear
        public float Wear => Participant.CarDamageData.m_tyresWear.Max(); // wear
        public Dictionary<int, float> WearByLap { get; set; } = new Dictionary<int, float>(); // wear by lap
        public float StintWear => Wear - TyreWearAtFitting; // wear since fitting
        public float WearLastLap =>
            WearByLap.ContainsKey(CurrentLapNumber) && WearByLap.ContainsKey(CurrentLapNumber - 1)
                ? WearByLap[CurrentLapNumber] - WearByLap[CurrentLapNumber - 1]
                : 0; // wear last lap
        public float WearPerDistance => StintWear / StintOdometer; // wear per m
        public float AverageWearPerLap => WearPerDistance * TrackLength; // wear per lap based wear per m and track length

        // Variables related to odometer
        public float SessionOdometer => Participant.LapData.m_totalDistance; // distance since session start
        public float StintOdometer => SessionOdometer - OdometerAtFitting; // distance since fitting

        // Variables related to track length
        public float TrackLength => Participant.Session.PacketSessionData.m_trackLength; // distance of a lap

        // Variables related to pit stop window
        public byte PitStopWindowIdealLap =>
            Participant.Session.PacketSessionData.m_pitStopWindowIdealLap > 0
                ? Participant.Session.PacketSessionData.m_pitStopWindowIdealLap
                : Participant.Session.NumLaps; // ideal lap for pit stop, num laps if no data
        public byte PitStopWindowLatestLap =>
            Participant.Session.PacketSessionData.m_pitStopWindowLatestLap > 0
                ? Math.Min(
                    Participant.Session.PacketSessionData.m_pitStopWindowLatestLap,
                    Participant.Session.NumLaps
                ) // i guess latest lap is sometimes higher than num laps for some reason, clipping it to num laps
                : Participant.Session.NumLaps; // latest lap for pit stop, num laps if no data
        public float WearAtIdealPitLap { get; set; } // wear at ideal pit lap
        public float WearAtLatestPitLap { get; set; } // wear at latest pit lap
        public byte ArtificialPredictedPitLap { get; set; } // artificial predicted pit lap based on real window and gausian distribution

        // Variables related to current lap
        public int CurrentLapNumber { get; set; } = 0; // current lap number
        public float WearAtStartOfLap =>
            WearByLap.ContainsKey(CurrentLapNumber) ? WearByLap[CurrentLapNumber] : 0; // wear at start of lap

        public TyreSet(Participant participant, TyreSet previousTyreSet = null)
        {
            Participant = participant;
            PreviousTyreSet = previousTyreSet;
            FitTyreSet();
            Update();

            // artificial predicted pit lap
            byte pitWindowSize = (byte)(PitStopWindowLatestLap - PitStopWindowIdealLap);
            byte artificialEarliestPitLap = (byte)(PitStopWindowIdealLap - (pitWindowSize / 2));
            ArtificialPredictedPitLap = (byte)(
                (byte)
                    Math.Round(
                        Utility.GaussianRandom(
                            mean: pitWindowSize / 2, // mean
                            stdDev: pitWindowSize / 6, // std dev
                            Participant.Session.Randomizer
                        )
                    ) + artificialEarliestPitLap
            );
        }

        public void FitTyreSet()
        {
            OdometerAtFitting = Participant.LapData.m_totalDistance;
            TyreWearAtFitting = Participant.CarDamageData.m_tyresWear.Max();
            LapNumberAtFitting = Participant.CurrentLapNumber;
        }

        public float WearAtDistance(float distance)
        {
            return Wear + (distance - SessionOdometer) * WearPerDistance;
        }

        public void Update()
        {
            //// new lap
            if (Participant.CurrentLapNumber != CurrentLapNumber)
            {
                CurrentLapNumber = Participant.CurrentLapNumber;
                WearByLap.Add(CurrentLapNumber, Wear);
            }

            // track length data exists
            if (TrackLength == 0)
                return;

            //// pit stop window
            WearAtIdealPitLap = WearAtDistance(PitStopWindowIdealLap * TrackLength);
            WearAtLatestPitLap = WearAtDistance(PitStopWindowLatestLap * TrackLength);
            ArtificialPredictedPitLap = (byte)Math.Max(CurrentLapNumber, ArtificialPredictedPitLap); // in case the predicted pit lap is before the current lap
        }

        public struct Tyres
        {
            public const string FrontLeft = "FrontLeft";
            public const string FrontRight = "FrontRight";
            public const string RearLeft = "RearLeft";
            public const string RearRight = "RearRight";
        }

        public static string[] TyresArray = new string[]
        {
            Tyres.RearLeft,
            Tyres.RearRight,
            Tyres.FrontLeft,
            Tyres.FrontRight
        };

        // uint8       m_visualTyreCompound;       // F1 visual (can be different from actual compound)
        //                                         // 16 = soft, 17 = medium, 18 = hard, 7 = inter, 8 = wet
        //                                         // F1 Classic – same as above
        //                                         // F2 ‘19, 15 = wet, 19 – super soft, 20 = soft
        //                                         // 21 = medium , 22 = hard
        public string VisualTyreName
        {
            get
            {
                switch (Participant.CarStatusData.m_visualTyreCompound)
                {
                    case 16:
                        return "soft";
                    case 17:
                        return "medium";
                    case 18:
                        return "hard";
                    case 7:
                        return "inter";
                    case 8:
                        return "wet";
                    case 15:
                        return "wet";
                    case 19:
                        return "super soft";
                    case 20:
                        return "soft";
                    case 21:
                        return "medium";
                    case 22:
                        return "hard";
                    default:
                        return "unknown";
                }
            }
        }
    }
}
