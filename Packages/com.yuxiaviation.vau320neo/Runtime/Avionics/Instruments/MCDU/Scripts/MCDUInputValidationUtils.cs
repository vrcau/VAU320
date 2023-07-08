namespace A320VAU.MCDU {
    public class MCDUInputValidationUtils {
        public static bool ValidateNavaid(string navaid) {
            return navaid.Length <= 5;
        }

    #region Frequency

        public static bool TryGetFrequency(string frequency, out float frequencyValue) {
            if (ValidateFrequencyMHz(frequency)) {
                frequencyValue = float.Parse(frequency);
                return true;
            }

            frequencyValue = -1;
            return false;
        }

        public static bool ValidateFrequencyMHz(string frequency) {
            return float.TryParse(frequency, out var frequencyFloat) && ValidateFrequencyMHz(frequencyFloat);
        }

        public static bool ValidateFrequencyMHz(float frequency) {
            return frequency > 108 && frequency < 117.95;
        }

    #endregion

    #region Course

        public static bool TryGetCourse(string course, out int courseValue) {
            if (!ValidateCourse(course)) {
                courseValue = -1;
                return false;
            }

            courseValue = int.Parse(course);
            return true;
        }

        public static bool ValidateCourse(string course) {
            return int.TryParse(course, out var courseInt) && ValidateCourse(courseInt);
        }

        public static bool ValidateCourse(int course) {
            return course > 0 && course <= 360;
        }

    #endregion
    }
}