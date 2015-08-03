using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common.Logging;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using RpsPositionWebApp.Data;
using RpsPositionWebApp.ViewModels;

namespace RpsPositionWebApp
{
    [HubName("vehicleEvents")]
    public class VehicleEventHub : Hub
    {
        private ILog log;
        private ClientManager clientManager;
        private VehicleAssignmentManager vehicleAssignmentManager;
        private VehicleEventQueue vehicleEventQueue;

        public VehicleEventHub()
        {
            log = LogManager.GetLogger<VehicleEventHub>();
            clientManager = ClientManager.Instance;
            vehicleEventQueue = VehicleEventQueue.Instance;
            vehicleAssignmentManager = VehicleAssignmentManager.Instance;
        }

        public void SetEndpoint(string endpointAddress)
        {
            clientManager.ConnectedIds.Add(Context.ConnectionId);
            var config = clientManager.GetConfiguration(Context.ConnectionId);
            config["endpointAddress"] = endpointAddress;
        }

        public void VehicleAssignment(VehicleAssignmentEvent vehicleAssignmentEvent)
        {            
            clientManager.ConnectedIds.Add(Context.ConnectionId);
            vehicleAssignmentEvent.Source = Context.ConnectionId;
            vehicleEventQueue.Enqueue(vehicleAssignmentEvent);

            vehicleAssignmentManager.AssignVehicle(vehicleAssignmentEvent.VehicleId, vehicleAssignmentEvent.JourneyId);
        }

        public void VehiclePosition(VehiclePositionEvent vehiclePositionEvent)
        {
            clientManager.ConnectedIds.Add(Context.ConnectionId);
            var config = clientManager.GetConfiguration(Context.ConnectionId);

            vehiclePositionEvent.Source = Context.ConnectionId;
            vehiclePositionEvent.Target = config["endpointAddress"];
            vehicleEventQueue.Enqueue(vehiclePositionEvent);
        }

        public override Task OnConnected()
        {
            log.InfoFormat("Client '{0}' connected.", Context.ConnectionId);
            clientManager.ConnectedIds.Add(Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            log.InfoFormat("Client '{0}' disconnected.", Context.ConnectionId);
            clientManager.ConnectedIds.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }
}