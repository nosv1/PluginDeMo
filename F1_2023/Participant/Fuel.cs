namespace PluginDeMo_v2.F1_2023.Participants
{
    public class Fuel
    {
        // Participant-related variables
        Participant Participant { get; set; }

        // Variables related to stint start
        public float OdometerAtStart { get; set; } // distance since exiting garage
        public float FuelAtStart { get; set; } // fuel at time of exiting garage

        // Variables related to fuel consumption
        public float FuelInTank { get; set; } // current fuel
        public float FuelConsumed => FuelAtStart - FuelInTank; // fuel consumed since garage exit
        public float FuelConsumedLastLap { get; set; } // fuel consumed last lap
        public float FuelPerDistance => FuelConsumed / StintOdometer; // fuel consumed per m
        public float AverageFuelConsumedPerLap { get; set; } // fuel per lap based on fuel per m and track length

        // Variables related to odometer
        public float StintOdometer => Participant.LapData.m_totalDistance - OdometerAtStart; // distance since exiting garage

        // Variables related to track length
        public float TrackLength => Participant.Session.PacketSessionData.m_trackLength; // distance of a lap

        // variables related to prediction
        public float FuelAtEndOfStint { get; set; } // fuel at end of stint as a percentage of average per lap

        // Variables related to current lap
        public int CurrentLapNumber { get; set; } = 0; // current lap number
        public float FuelAtStartOfLap { get; set; } // fuel at start of lap

        public Fuel(Participant participant)
        {
            Participant = participant;
            StartStint();
            Update();
        }

        public void StartStint()
        {
            OdometerAtStart = Participant.LapData.m_totalDistance;
            FuelAtStart = Participant.CarStatusData.m_fuelInTank;
        }

        public float FuelRemainingAtDistance(float distance)
        {
            return FuelInTank - FuelPerDistance * (distance - StintOdometer);
        }

        public void Update()
        {
            // fuel consumption
            FuelInTank = Participant.CarStatusData.m_fuelInTank;

            // fuel last lap
            if (Participant.CurrentLapNumber != CurrentLapNumber)
            {
                CurrentLapNumber = Participant.CurrentLapNumber;
                FuelConsumedLastLap = FuelAtStartOfLap - FuelInTank;
                FuelAtStartOfLap = FuelInTank;
            }

            // fuel per lap
            if (TrackLength == 0)
                return;

            AverageFuelConsumedPerLap = FuelPerDistance * TrackLength;
            float fuelAtDistance = FuelRemainingAtDistance(
                Participant.Session.NumLaps * TrackLength
            );
            FuelAtEndOfStint = fuelAtDistance / AverageFuelConsumedPerLap;
        }
    }
}
