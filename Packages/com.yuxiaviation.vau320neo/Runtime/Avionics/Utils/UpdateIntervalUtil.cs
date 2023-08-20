using UnityEngine;

namespace A320VAU.Utils {
    public class UpdateIntervalUtil {
        public static float GetUpdateIntervalFromFPS(int fps) {
            return 1f / fps;
        }
        
        public static float GetUpdateIntervalFromSeconds(int seconds) {
            return seconds;
        }

        public static float GetUpdateIntervalFromMilliseconds(int milliseconds) {
            return 0.001f * milliseconds;
        }
        
        public static bool CanUpdate(ref float lastUpdate, float updateInterval) {
            if (Time.time - lastUpdate < updateInterval) {
                return false;
            }

            lastUpdate = Time.time;
            return true;
        }
    }
}