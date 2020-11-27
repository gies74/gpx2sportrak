using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Geo
{
    public class Waypoint
    {
        public Coordinate Coord;
        public string Name;
        public string Comment;
        public string Icon;

        public static System.Globalization.CultureInfo _ci = new System.Globalization.CultureInfo("en-us");

        public Waypoint(Coordinate coord, string name, string comment)
        {
            this.Coord = coord;
            this.Name = name;
            this.Comment = comment;
            this.Icon = "a";
        }



        public static Waypoint FromMagellanCmd(string cmd)
        {
            // example cmd $PMGNWPL,5149.858,N,00550.805,E,0000000,M,thuis,,a*26
            var parts = Regex.Split(cmd, "[$*,]");
            var coord = new Coordinate(FromMagellanCoord(parts[4], parts[5]), FromMagellanCoord(parts[2], parts[3]));
            return new Waypoint(coord, parts[8], "");
        }

        private static double FromMagellanCoord(string degMinute, string hemi)
        {
            if (!(degMinute.Length == 8 && Regex.IsMatch(hemi, "^[NS]$") || degMinute.Length == 9 && Regex.IsMatch(hemi, "^[WE]$")))
            {
                throw new Exception("Wrong param");
            }
            var deg = Double.Parse(degMinute.Substring(0, degMinute.Length - 6), _ci);
            var minute = Double.Parse(degMinute.Substring(degMinute.Length - 6), _ci) / 60;
            return ((hemi == "N" || hemi == "E") ? 1 : -1)  * (deg + minute);

        }

    }
}
