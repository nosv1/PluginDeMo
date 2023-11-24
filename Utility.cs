using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static float GaussianRandom(float min, float max, float stdDev)
        {
            Random random = new Random();
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = min + stdDev * randStdNormal; //random normal(mean,stdDev^2)

            if (randNormal < min)
                randNormal = min;
            else if (randNormal > max)
                randNormal = max;

            return (float)randNormal;
        }

        //// property names ////
        public static string PdMPropertyPrefix()
        {
            return "PdM_";
        }

        public static string ParticipantPrefix(bool isPlayer, int index)
        {
            if (isPlayer)
                return $"{PdMPropertyPrefix()}Player_"; // PdM_Player_
            else
                return $"{PdMPropertyPrefix()}Participant{index.ToString().PadLeft(2, '0')}_"; // PdM_Participant00_
        }

        public static string SessionPrefix()
        {
            return $"{PdMPropertyPrefix()}Session_"; // PdM_Session_
        }
    }
}
