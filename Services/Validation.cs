namespace SmartTransport.Services
{
    public static class Validation
    {
        public static void CheckPositive(double value, string field)
        {
            if (value <= 0) throw new ArgumentException($"{field} must be a positive number (got {value}).");
        }
    }
}