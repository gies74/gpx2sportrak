using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;

namespace Geo
{
    public static class GPX
    {

        public static Route ReadRouteFromFile(string filename)
        {
            var route = new Route();

            List<string> _uniqueAlias = new List<string>();

            var enus = new System.Globalization.CultureInfo("en-us");
            var xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
            int i = 0;
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
            xmlnode = xmldoc.GetElementsByTagName("rte");
            route.Name = xmlnode[0]["name"].InnerText;
            xmlnode = xmldoc.GetElementsByTagName("rtept");
            for (i = 0; i <= xmlnode.Count - 1; i++)
            {
                var lon = Double.Parse(xmlnode[i].Attributes["lon"].Value, enus);
                var lat = Double.Parse(xmlnode[i].Attributes["lat"].Value, enus);
                // 2 waypoints op zelfde coordinaat trekt mapsend niet
                if (route.Any(wp => wp.Coord.Longitude == lon && wp.Coord.Latitude == lat))
                {
                    continue;
                }
                var name = MakeUniqueName(xmlnode[i].ChildNodes.Item(0).InnerText.Trim(), _uniqueAlias);

                // comment wordt niet gebruikt - zet op leeg
                var comment = ""; // xmlnode[i].ChildNodes.Item(1).InnerText.Trim();
                var wpt = new Waypoint(new Coordinate(lon, lat), name, comment);
                route.Add(wpt);
            }

            return route;
        }

        private static string MakeUniqueName(string name, List<string> _uniqueAlias)
        {
            name = name.Replace("Sharp ", "Sh ");
            name = name.Replace("Slight ", "Sl ");
            string candName = name.Substring(0, Math.Min(8, name.Length));
            int seq = 0;
            while (_uniqueAlias.Contains(candName))
            {
                candName = $"{name.Substring(0, Math.Min(6, name.Length))}{seq++:00}";
            }
            _uniqueAlias.Add(candName);

            return candName;

        }
    }
}
