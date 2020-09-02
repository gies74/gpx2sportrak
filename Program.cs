using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Geo;
using Magellan;

namespace gpx2sportrak
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: gpx2ms <infile>\nwhere infile is GPX route");
                return;
            }

            Magellan.SerialPort.HandshakeOff();
            Magellan.Hardware hw = Magellan.SerialPort.ReadDeviceVersion();
            if (hw == Hardware.None)
            {
                Console.Error.WriteLine("No unit was detected!");
                return;
            }
            Console.WriteLine($"Unit {Enum.GetName(typeof(Magellan.Hardware), hw)} was detected!");
            

            Route orgRoute = GPX.ReadRouteFromFile(args[0]);
            List<Route> routes = orgRoute.Split(30);

            Magellan.SerialPort.DeleteAllRoutes();
            Magellan.SerialPort.DeleteAllWaypoints();

            //var fiInput = new FileInfo(args[0]);
            //string outFilename = Path.Combine(fiInput.DirectoryName, $"{Path.GetFileNameWithoutExtension(fiInput.Name)}.wpt");
            // Mapsend.WriteRoutes(outFilename, orgRoute, routes);

            Magellan.SerialPort.SendWaypoints(orgRoute);
            Magellan.SerialPort.SendRoutes(routes);
        }
    }
}
