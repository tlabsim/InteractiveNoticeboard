using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveNoticeboard.Data_Structures.API.OpenWeatherMap
{
    //{
    //"coord":{"lon":91.1,"lat":22.79},
    //"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],
    //"base":"stations",
    //"main":{"temp":299.1,"pressure":1007.72,"humidity":79,"temp_min":299.1,"temp_max":299.1,"sea_level":1007.72,"grnd_level":1007.13},
    //"wind":{"speed":4.81,"deg":268.26},
    //"clouds":{"all":100},
    //"dt":1556984319,
    //"sys":{"message":0.0049,"country":"BD","sunrise":1556925676,"sunset":1556972609},
    //"id":1196292,
    //"name":"Lakshmipur",
    //"cod":200
    //}

    public class Coord
    {
        public double lon, lat;
    }

    public class WeatherSummary
    {
        public int id;
        public string main;
        public string description;
        public string icon;
    }

    public class WeatherDetails
    {
        public double temp;
        public double pressure;
        public double humidity;
        public double temp_min;
        public double temp_max;
        public double sea_level;
        public double grnd_level;
    }

    public class WindData
    {
        public double speed;
        public double deg;
    }

    public class CloudData
    {
        public double all;
    }

    public class LocationData
    {
        public double message;
        public string country;
        public int sunrise;
        public int sunset;
    }

    public class WeatherData
    {
        public Coord coord;
        public List<WeatherSummary> weather;
        public WeatherDetails main;
        public WindData wind;
        public CloudData clouds;
        public int dt;
        public LocationData sys;
    }
}
