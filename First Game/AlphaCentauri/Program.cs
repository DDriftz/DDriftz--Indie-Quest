using System;

class Mission5
{
    static void Main()
    {
        // Constants
        // Speed of light in meters per second (exact by definition)
        double speedOfLight = 299792458; // m/s

        // Number of seconds in a Julian year (365.25 days)
        double secondsPerYear = 365.25 * 24 * 3600; // 31,557,600 seconds

        // Calculate one light year in meters
        double lightYearMeters = speedOfLight * secondsPerYear;
        // Approximately 9.4607304725808e15 meters

        // Astronomical Unit (AU) in meters (exact by IAU definition)
        double AU = 149597870700; // meters

        // One parsec is defined as (AU * 648000) / π
        double parsecMeters = (AU * 648000) / Math.PI;
        // Approximately 3.0856775814913673e16 meters

        // Given distance to Alpha Centauri in light years
        double alphaCentauriDistanceLy = 4.365;

        // Conversion: distance in parsecs = (light years * meters per light year) / meters per parsec
        double alphaCentauriDistancePc = alphaCentauriDistanceLy * lightYearMeters / parsecMeters;

        // Display the distance with high precision (6 decimal places)
        Console.WriteLine("Alpha Centauri is approximately {0:F6} parsecs away.", alphaCentauriDistancePc);
    }
}
