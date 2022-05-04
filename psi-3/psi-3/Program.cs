using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace psi_3
{
    class Program
    {
        private const string ISS_NOW_API = "http://api.open-notify.org/iss-now.json";
        private const string SUNSET_SUNRICE_API = "https://api.sunrise-sunset.org/json?";
        //private const string UNIX_TIMESTAMP_API = "https://showcase.api.linx.twenty57.net/UnixTime/fromunix?timestamp="; // Not working since 3.05
        private const string UNIX_TIMESTAMP_API = "https://helloacm.com/api/unix-timestamp-converter/?cached&s="; 

        static void Main(string[] args)
        {
            // Get ISS position
            string json = GetISSLocationNow();
            dynamic issObj = JsonConvert.DeserializeObject(json);

            // Get sunset sunrise information
            string sunsetSunriseJson = GetSunsetSunrise(Convert.ToString(issObj.iss_position.latitude), Convert.ToString(issObj.iss_position.longitude));
            dynamic sunsetSunriseObj = JsonConvert.DeserializeObject(sunsetSunriseJson);

            // Parse time
            DateTime actual = ConvertUnixTimeToHumanTime(Convert.ToString(issObj.timestamp));
            DateTime  sunrise = DateTime.Parse(sunsetSunriseObj.results.sunrise.ToString());
            DateTime  sunset = DateTime.Parse(sunsetSunriseObj.results.sunset.ToString());

            // Write down the results
            Console.WriteLine("[ISS]");
            Console.WriteLine($"Latitude:\t{Convert.ToString(issObj.iss_position.latitude)}");
            Console.WriteLine($"Longitude:\t{Convert.ToString(issObj.iss_position.longitude)}");
            Console.WriteLine($"Actual time:\t{actual}");
            Console.WriteLine($"Sunrise:\t{sunrise}");
            Console.WriteLine($"Sunset:\t\t{sunset}");
            Console.WriteLine();

            if (Between(actual, sunrise, sunrise.AddHours(-2)))    // 2 hours before sunrise
            {
                var hours = actual.Subtract(sunrise);
                Console.WriteLine($"Ideal conditions to observer ISS ({Convert.ToString(hours)}h before sunrise)");
            }
            else if (Between(actual, sunset, sunset.AddHours(2)))  // 2 hours after sunset
            {
                var hours = actual.Subtract(sunset);
                Console.WriteLine($"Ideal conditions to observer ISS ({Convert.ToString(hours)}h after sunset)");
            }
            else
            {
                Console.WriteLine("Not ideal conditions to observer ISS");
            }

            // Calculate light or dart part of the Earth
            string dayornight = DayOrNight(sunset, sunrise, actual) ? "light" : "dark";
            Console.Write($"ISS is now on {dayornight} part og the Earth");

            Console.ReadLine();
        }

        private static bool DayOrNight(DateTime sunset, DateTime sunrise, DateTime actual)
        {
            int compareSunset = actual.CompareTo(sunset);
            int compareSunrise = actual.CompareTo(sunrise);

            if (compareSunset < 0) // Earlier 
            {
                return true;
            }

            if (compareSunset > 0) // Later  
            {
                return false;
            }

            if (compareSunset == 0) // Same  
            {
                return false;
            }

            if (compareSunrise < 0) // Earlier 
            {
                return false;
            }

            if (compareSunrise > 0) // Later  
            {
                return true;
            }

            if (compareSunrise == 0) // Same  
            {
                return true;
            }

            return false;
        }

        private static string GetISSLocationNow()
        {
            return new WebClient().DownloadString(ISS_NOW_API);
        }

        private static DateTime ConvertUnixTimeToHumanTime(string timestamp)
        {
            string str = string.IsNullOrEmpty(timestamp) ? null : new WebClient().DownloadString(UNIX_TIMESTAMP_API + timestamp).Replace("\"", string.Empty);

            DateTime date = DateTime.Parse(str);

            return date;
        }

        private static string GetSunsetSunrise(string lat, string lng)
        {
            // api.sunrise-sunset.org has expired certificate now 24.04.2022
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true; // Disable certificate checking
        
            return string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng)
                ? null
                : new WebClient().DownloadString(SUNSET_SUNRICE_API + "lat=" + lat + "&lng=" + lng);
        }

        public static bool Between(DateTime input, DateTime date1, DateTime date2)
        {
            return (input > date1 && input < date2);
        }
    }
}
