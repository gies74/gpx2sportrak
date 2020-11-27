using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Geo;

namespace Magellan
{
    public static class SerialPort
    {
        public static bool HandshakeOff()
        {
            try
            {
                var s = SendString("$PMGNCMD,HANDOFF*", AwaitResponse.False);
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return false;
            }
        }
        public static Hardware ReadDeviceVersion()
        {
            try
            {
                var s = SendString("$PMGNCMD,VERSION*")[0];
                var parts = Regex.Split(s, "[$,*]");
                return (Hardware)Enum.Parse(typeof(Hardware), parts[4].Replace(" ", ""));
            }
            catch
            {
                return Hardware.None;
            }
        }

        public static void SendWaypoint(Waypoint wp)
        {
            var s = SendString("$PMGNWPL,5150.000,N,00550.000,E,0,M,Gies,,a,00*", AwaitResponse.False);
        }


        public static IEnumerable<int> RetrieveAllRouteIDs()
        {
            var routes = SendString("$PMGNCMD,ROUTE*", AwaitResponse.UntilEnd);

            return routes.Select(r => Route.FromMagellanCmd(r)).Distinct();

        }

        public static void DeleteRoutes(IEnumerable<int> routeIDs)
        {
            var cmds = routeIDs.Select(r => $"$PMGNDRT,{(r - 1).ToString("00")}*");
            SendString(cmds, AwaitResponse.False);
        }

        public static void DeleteAllRoutes()
        {
            SendString("$PMGNCMD,DELETE,ROUTES*", AwaitResponse.False);
        }

        public static void DeleteAllWaypoints()
        {
            SendString("$PMGNCMD,DELETE,WAYPOINTS*", AwaitResponse.False);
        }


        public static List<Waypoint> RetrieveAllWaypoints() {
            var wpts = SendString("$PMGNCMD,WAYPOINT*", AwaitResponse.UntilEnd);

            var l = new List<Waypoint>();
            foreach (var wpt in wpts)
            {
                l.Add(Waypoint.FromMagellanCmd(wpt));
            }
            return l;
        }

        public static void DeleteWaypoints(List<Waypoint> wpts)
        {
            // $PMGNDWP,c---c,xx*
            var cmds = wpts.Select(w => $"$PMGNDWP,{w.Name},a*");
            SendString(cmds, AwaitResponse.Single);
        }

        public static void SendWaypoints(IEnumerable<Waypoint> wpts)
        {
            var cmds = wpts.Select(w => $"$PMGNWPL,{w.Coord.ToMagellanLat()},{w.Coord.ToMagellanLon()},0,M,{w.Name},,{w.Icon}*");
            // $PMGNWPL,5150.000,N,00550.000,E,0,M,Gies,comment,a,00*
            // $PMGNWPL,5149.843,N,00550.815,E,0,M,Start,comment,a,00*
            SendString(cmds, AwaitResponse.False);

        }
        public static void SendRoutes(List<Route> routes)
        {
            var cmds = new List<string>();
            foreach (Route rte in routes)
            {
                const int WPTS_PER_ROUTE_MSG = 8;
                var total = Math.Ceiling((decimal)rte.Count / WPTS_PER_ROUTE_MSG);
                for (int i = 0; i < rte.Count; i += WPTS_PER_ROUTE_MSG)
                {
                    var batchNo = Math.Ceiling((decimal)i/WPTS_PER_ROUTE_MSG) + 1;

                    cmds.Add($"$PMGNRTE,{total},{batchNo},c,{routes.IndexOf(rte) + 1},{String.Join(",", rte.Skip(i).Take(WPTS_PER_ROUTE_MSG).Select((w,idx) => $"{w.Name},{w.Icon}"))}*");
                }
            }
            // $PMGNRTE,2,1,c,1,FOO,POINT1,b,POINT2,c,POINT3,d*6C
            SendString(cmds, AwaitResponse.False);
        }


        private static List<string> SendString(string cmd, AwaitResponse awaitResponse = AwaitResponse.Single)
        {
            return SendString(new List<string> { cmd }, awaitResponse);
        }

        private static List<string> SendString(IEnumerable<string> cmds, AwaitResponse awaitResponse = AwaitResponse.Single)
        {
            var responses = new List<string>();
            var portNames = System.IO.Ports.SerialPort.GetPortNames();
            if (portNames.Length != 1) throw new Exception("No or too many COM ports available");

            const int MSGS_PER_SLEEPCYCLE = 8;
            for (int i = 0; i < cmds.Count(); i += MSGS_PER_SLEEPCYCLE)
            {

                using (var sp = new System.IO.Ports.SerialPort(portNames[0], 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
                {
                    sp.Open();
                    sp.ReadTimeout = 500;

                    sp.DiscardInBuffer();

                    foreach (var cmd in cmds.Skip(i).Take(MSGS_PER_SLEEPCYCLE))
                    {
                        sp.Write(Utils.CS(cmd));

                        while (awaitResponse != AwaitResponse.False)
                        {
                            var readData = sp.ReadLine();
                            string response = Utils.CS(readData);
                            if (awaitResponse == AwaitResponse.Single || awaitResponse == AwaitResponse.UntilEnd && response.StartsWith("$PMGNCMD,END*"))
                            {
                                if (awaitResponse == AwaitResponse.Single)
                                {
                                    responses.Add(response);
                                }
                                break;
                            }
                            responses.Add(response);
                        }
                    }
                    sp.Close();
                }
                System.Threading.Thread.Sleep(1000);
            }

            return responses;

        }

        public enum AwaitResponse
        {
            False,
            Single,
            UntilEnd
        }
    }
}
