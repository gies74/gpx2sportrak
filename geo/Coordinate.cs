using System;
using System.Collections.Generic;
using System.Text;

namespace Geo
{
    public class Coordinate
    {
        public double Longitude;
        public double Latitude;

        private static System.Globalization.CultureInfo _ci = new System.Globalization.CultureInfo("en-us");

        public Coordinate(double lon, double lat)
        {
            this.Longitude = lon;
            this.Latitude = lat;
        }

        public string ToMagellanLat()
        {
            return $"{ToMagellan(this.Latitude, 2)},{(this.Latitude > 0 ? 'N' : 'S')}";
        }
        public string ToMagellanLon()
        {
            return $"{ToMagellan(this.Longitude, 3)},{(this.Longitude > 0 ? 'E' : 'W')}";
        }

        private static string ToMagellan(double decDeg, int pos)
        {
            var resp = Math.Truncate(decDeg).ToString(new string('0', pos));
            var minute = (decDeg - Math.Truncate(decDeg)) * 60;
            return resp + minute.ToString("00.000", _ci);
        }
    }
}
