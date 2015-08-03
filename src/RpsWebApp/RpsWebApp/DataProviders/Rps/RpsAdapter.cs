using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;
using Common.Logging;
using RpsPositionWebApp.Data;
using RpsPositionWebApp.ViewModels;

namespace RpsPositionWebApp.DataProviders.Rps
{
    public class RpsAdapter
    {
        private readonly ILog log;
        private readonly MessageConverter messageConverter;
        private readonly VehicleEventQueue vehicleEventQueue;

        public RpsAdapter()
        {
            log = LogManager.GetLogger<RpsAdapter>();
            messageConverter = new MessageConverter();
            vehicleEventQueue = VehicleEventQueue.Instance;
        }

        private DateTime lastHearthBeat;
        private long processedMessages = 0L;

        public void LoadState()
        {
            log.Info("Loading state ...");

            string path = HostingEnvironment.MapPath("~/rpsadapter.state");
            if (path == null)
            {
                path = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "/rpsadapter.state";
            }
            try
            {
                if (File.Exists(path))
                {
                    messageConverter.sequenceNumber = ushort.Parse(File.ReadAllText(path));
                }
            }
            catch (Exception exception)
            {
                log.Warn("Failed to load state", exception);
            }
        }

        public void SaveState()
        {
            log.Info("Saving state ...");

            string path = HostingEnvironment.MapPath("~/rpsadapter.state");

            if (path == null)
            {
                path = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "/rpsadapter.state";
            }

            File.WriteAllText(path, messageConverter.sequenceNumber.ToString());
        }

        /// <summary>
        /// Parses and IPv4 or IPv6 adress
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        private IPEndPoint ParseEndPoint(string endPoint)
        {
            if (endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));

            string[] ep = endPoint.Split(':');
            if (ep.Length < 2)
                throw new FormatException("Invalid endpoint format");

            IPAddress ip;

            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }

            int port;

            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }

            return new IPEndPoint(ip, port);
        }

        public void Run()
        {
            try
            {
                /* Initialize worker */
                Thread.CurrentThread.Name = "RpsAdapter";
                log.Info("Starting RpsAdapter ...");

                LoadState();
                lastHearthBeat = DateTime.UtcNow;

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                /* Worker main loop */
                while (!CancellationToken.WaitHandle.WaitOne(1000))
                {
                    int count = vehicleEventQueue.Count;
                    TimeSpan span = DateTime.UtcNow - lastHearthBeat;

                    if (span.TotalMinutes > 5)
                    {
                        log.InfoFormat("Hearth Beat: (queue/total) ({0}/{1}).", count, processedMessages);

                        if (span.TotalMinutes > 10)
                            log.WarnFormat("Hearth Beat was delayed: {0} min.", span.TotalMinutes);

                        lastHearthBeat = DateTime.UtcNow;
                    }

                    if (count > 0)
                    {
                        var currentCount = 0;

                        VehicleEvent vehicleEvent;
                        while (vehicleEventQueue.TryDequeue(out vehicleEvent))
                        {
                            if (vehicleEvent is VehiclePositionEvent && vehicleEvent.Target is string)
                            {
                                var remoteEP = ParseEndPoint(vehicleEvent.Target as string);

                                byte[] bytes = messageConverter.VehiclePositionReport((VehiclePositionEvent)vehicleEvent);
                                log.DebugFormat("Sending Vehicle Event '{0}' to '{1}'.", MessageConverter.ByteArrayToHexViaLookup(bytes), vehicleEvent.Target);
                                socket.SendTo(bytes, remoteEP);
                                currentCount++;
                            }
                        }
                        processedMessages += currentCount;
                    }
                }

                /* Test if shurdown is requested. */
                if (CancellationToken.IsCancellationRequested)
                {
                    log.Info("Stopping RpsAdapter ...");
                    this.SaveState();
                }
            }
            catch (Exception exception)
            {
                log.Error("Fatal Error in RpsAdapter:", exception);
            }
        }

        public CancellationToken CancellationToken { get; set; }

        public long TotalProcessedMessages
        {
            get
            {
                return this.processedMessages;
            }
        }
    }
}
