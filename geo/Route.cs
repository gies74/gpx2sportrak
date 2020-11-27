using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Geo
{
    public class Route : List<Waypoint>
    {
        public string Name;


        public Route() : base()
        {

        }

        public Route(IEnumerable<Waypoint> r) : base(r)
        {

        }

        public static int FromMagellanCmd(string cmd)
        {
            // geef alleen de index terug
            return Int32.Parse(Regex.Split(cmd, "[$*,]")[5]);
        }

        public List<Route> Split(int maxWptsPerRoute)
        {
            var retVal = new List<Route>();
            var num = 0;
            for (int i = 0; i < this.Count - 1; i += (maxWptsPerRoute - 1))
            {
                var r = new Route(this.GetRange(i, Math.Min(maxWptsPerRoute, this.Count - i)));
                r.Last().Icon = "b";
                r.Name = $"{this.Name.Substring(0, Math.Min(6, this.Name.Length))}{num++:00}";
                retVal.Add(r);
            }
            return retVal;
        }
    }
}
