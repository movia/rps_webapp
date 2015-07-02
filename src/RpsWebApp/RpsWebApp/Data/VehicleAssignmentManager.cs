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

        private ILog log;

        private Dictionary<string, string> vehicleToServiceJourneyMap;
        private Dictionary<string, string> serviceJourneyToVehicleMap;

        public VehicleAssignmentManager()
        {
            log = LogManager.GetLogger<VehicleAssignmentManager>();

            vehicleToServiceJourneyMap = new Dictionary<string, string>();
            serviceJourneyToVehicleMap = new Dictionary<string, string>();
        }

        public void AssignVehicle(string vehicleId, string journeyId)
        {
            log.InfoFormat("Assigning Vehicle Id '{0}' to Journey Id '{1}'.", vehicleId, journeyId);

            var _vehicleId = vehicleId.ToLower();
            var _journeyId = journeyId.ToLower();

            if (vehicleToServiceJourneyMap.ContainsKey(_vehicleId) && vehicleToServiceJourneyMap[_vehicleId].Equals(journeyId, StringComparison.OrdinalIgnoreCase))
            {
                log.InfoFormat("Journey Id '{0}' was already assigned to Vehicle Id '{1}'. Assignment is ignored.", journeyId, vehicleId);
                return;
            }

            if (vehicleToServiceJourneyMap.ContainsKey(_vehicleId))
            {
                var currentJourneyId = vehicleToServiceJourneyMap[_vehicleId];
                var _currentJourneyId = currentJourneyId.ToLower();

                log.InfoFormat("Vehicle Id '{0}' was already assigned to Journey Id '{1}'. Unassining Vehicle Id '{0}'.", vehicleId, currentJourneyId);

                vehicleToServiceJourneyMap.Remove(_vehicleId);
                serviceJourneyToVehicleMap.Remove(_currentJourneyId);
            }

            if (serviceJourneyToVehicleMap.ContainsKey(_journeyId))
            {
                var currentVehicleId = serviceJourneyToVehicleMap[_journeyId];
                var _currentVehicleId = currentVehicleId.ToLower();

                log.InfoFormat("Vehicle Id '{0}' was already assigned to Journey Id '{1}'. Unassining Vehicle Id '{0}'.", currentVehicleId, journeyId);

                vehicleToServiceJourneyMap.Remove(_currentVehicleId);
                serviceJourneyToVehicleMap.Remove(_journeyId);
            }

            vehicleToServiceJourneyMap.Add(_vehicleId, journeyId);
            serviceJourneyToVehicleMap.Add(_journeyId, vehicleId);
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