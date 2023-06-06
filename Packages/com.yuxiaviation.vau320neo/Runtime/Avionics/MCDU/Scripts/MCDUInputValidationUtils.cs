namespace A320VAU.MCDU
{
    public class MCDUInputValidationUtils
    {
        #region Frequency
        public static bool TryGetFrequency(string frequency, out float frequencyValue)
        {
            if (ValidateFrequencyMHz(frequency))
            {
                frequencyValue = float.Parse(frequency);
                return true;
            }

            frequencyValue = -1;
            return false;
        }
        
        public static bool ValidateFrequencyMHz(string frequency) =>
            float.TryParse(frequency, out var frequencyFloat) && ValidateFrequencyMHz(frequencyFloat);
        
        public static bool ValidateFrequencyMHz(float frequency) => frequency > 108 && frequency < 117.95;
        #endregion
        
        public static bool ValidateNavaid(string navaid) => navaid.Length <= 5;

        #region Course
        public static bool TryGetCourse(string course, out int courseValue)
        {
            if (!ValidateCourse(course))
            {
                courseValue = -1;
                return false;
            }
            
            courseValue = int.Parse(course);
            return true;

        }
        
        public static bool ValidateCourse(string course) =>
            int.TryParse(course, out var courseInt) && ValidateCourse(courseInt);
        
        public static bool ValidateCourse(int course) => course > 0 && course <= 360;
        #endregion
    }
}