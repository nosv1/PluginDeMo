using System;
using System.Text;

namespace PluginDeMo_v2
{
    public class Utility
    {
        //// string manipulation ////
        public static string GetStringFromCharArray(char[] chars)
        {
            return new string(chars).Trim('\0');
        }

        public static string GetStringFromByteArray(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes).Trim('\0');
        }

        public static string SecondsToTimeString(float seconds, string format = @"mm\:ss")
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.ToString(format);
        }

        //// random numbers ////
        public static float GaussianRandom(float mean, float stdDev, Random randomizer)
        {
            double u1 = 1.0 - randomizer.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - randomizer.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

            return (float)randNormal;
        }

        //// property names ////
        public static string PdMPropertyPrefix()
        {
            return "PdM_";
        }

        public static string ParticipantPrefix(
            bool isPlayer = false,
            bool isTeammate = false,
            int index = -1
        )
        {
            if (isPlayer)
                return $"{PdMPropertyPrefix()}Player_"; // PdM_Player_
            else if (isTeammate)
                return $"{PdMPropertyPrefix()}Teammate_"; // PdM_Teammate_
            else
                return $"{PdMPropertyPrefix()}Participant{index.ToString().PadLeft(2, '0')}_"; // PdM_Participant00_
        }

        public static string SessionPrefix()
        {
            return $"{PdMPropertyPrefix()}Session_"; // PdM_Session_
        }
    }
}
