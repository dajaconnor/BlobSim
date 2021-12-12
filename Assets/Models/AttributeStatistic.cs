namespace Assets.Models
{
    public class AttributeStatistic
    {
        public float? Min { get; private set; }
        public float? Max { get; private set; }
        public float Average { get; set; }

        public void UpdateMinMax(float newValue)
        {
            if (newValue < Min || Min == null) Min = newValue;
            if (newValue > Max || Max == null) Max = newValue;
        }

        public string PrintMinMax(bool isInt)
        {
            if (isInt) return $"{Min:F0} {Average:F0} {Max:F0}";

            return $"{Min:F2} {Average:F2} {Max:F2}";
        }
    }
}
