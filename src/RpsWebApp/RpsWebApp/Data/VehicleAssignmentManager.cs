using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Logging;

namespace RpsPositionWebApp.Data
{
    public class VehicleAssignmentManager
    {
        private static VehicleAssignmentManager instance = new VehicleAssignmentManager();

        public static VehicleAssignmentManager Instance { get { return instance; } }

        private readonly ILog log;
        private readonly Dictionary<string, string> vehicleToServiceJourneyMap;
        private readonly Dictionary<string, string> serviceJourneyToVehicleMap;

        public VehicleAssignmentManager()
        {
            log = LogManager.GetLogger<VehicleAssignmentManager>();
            vehicleToServiceJourneyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            serviceJourneyToVehicleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void AssignVehicle(string vehicleId, string journeyId)
        {
            log.InfoFormat("Assigning Vehicle Id '{0}' to Journey Id '{1}'.", vehicleId, journeyId);

            if (vehicleToServiceJourneyMap.ContainsKey(vehicleId) && vehicleToServiceJourneyMap[vehicleId].Equals(journeyId, StringComparison.OrdinalIgnoreCase))
            {
                log.InfoFormat("Journey Id '{0}' was already assigned to Vehicle Id '{1}'. Assignment is ignored.", journeyId, vehicleId);
                return;
            }

            if (vehicleToServiceJourneyMap.ContainsKey(vehicleId))
            {
                var currentJourneyId = vehicleToServiceJourneyMap[vehicleId];
                
                log.InfoFormat("Vehicle Id '{0}' was already assigned to Journey Id '{1}'. Unassining Vehicle Id '{0}'.", vehicleId, currentJourneyId);

                vehicleToServiceJourneyMap.Remove(vehicleId);
                serviceJourneyToVehicleMap.Remove(currentJourneyId);
            }

            if (serviceJourneyToVehicleMap.ContainsKey(journeyId))
            {
                var currentVehicleId = serviceJourneyToVehicleMap[journeyId];

                log.InfoFormat("Vehicle Id '{0}' was already assigned to Journey Id '{1}'. Unassining Vehicle Id '{0}'.", currentVehicleId, journeyId);

                vehicleToServiceJourneyMap.Remove(currentVehicleId);
                serviceJourneyToVehicleMap.Remove(journeyId);
            }

            vehicleToServiceJourneyMap.Add(vehicleId, journeyId);
            serviceJourneyToVehicleMap.Add(journeyId, vehicleId);
        }

        public string GetJourneyId(string vehicleId)
        {
            var _vehicleId = vehicleId.ToLower();
            if (vehicleToServiceJourneyMap.ContainsKey(_vehicleId))
                return vehicleToServiceJourneyMap[_vehicleId];
            return null;
        }
    }
}