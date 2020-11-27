using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Geo
{
    public static class Mapsend
    {
        private const int WAYPOINTS_ROUTES = 1;
        private const byte DEFAULT_ICON = 0;
        private const byte NOT_USED_IN_ROUTES = 1;
        private const byte USED_IN_ROUTES = 2;



        public static void WriteRoutes(string filename, Route allWaypoints, List<Route> routes)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                WriteHeader(writer, WAYPOINTS_ROUTES);
                WriteWaypoints(writer, allWaypoints);
                WriteRoutes(writer, allWaypoints, routes);
                writer.Close();
            }

        }

        private static void WriteHeader(BinaryWriter writer, int type)
        {
            writer.Write((byte)13);
            writer.Write("4D533336 MS".ToCharArray());
            writer.Write("36".ToCharArray());
            writer.Write(type);
        }

        private static void WriteWaypoints(BinaryWriter writer, Route allWaypoints)
        {
            writer.Write(allWaypoints.Count);
            int number = 1;
            foreach (var wp in allWaypoints)
            {
                writer.Write(wp.Name);
                writer.Write(wp.Comment);
                writer.Write(number++);
                writer.Write(DEFAULT_ICON);
                writer.Write(USED_IN_ROUTES);
                writer.Write(0d); // altitude
                writer.Write(wp.Coord.Longitude);
                writer.Write(-1 * wp.Coord.Latitude);
            }
        }

        private static void WriteRoutes(BinaryWriter writer, Route allWaypoints, List<Route> routes)
        {
            writer.Write(routes.Count);
            int number = 1;
            foreach (var route in routes)
            {
                writer.Write(route.Name);
                writer.Write(number++);
                WriteRouteWaypoints(writer, allWaypoints, route);
            }
        }

        private static void WriteRouteWaypoints(BinaryWriter writer, Route allWaypoints, Route route)
        {
            writer.Write(route.Count);
            foreach (var waypoint in route)
            {
                writer.Write(waypoint.Name);
                writer.Write(allWaypoints.IndexOf(waypoint) + 1);
                writer.Write(waypoint.Coord.Longitude);
                writer.Write(-1 * waypoint.Coord.Latitude);
                writer.Write(DEFAULT_ICON);
            }
        }
    }
}
