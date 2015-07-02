using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RpsPositionWebApp.ViewModels;

namespace RpsPositionWebApp.Data
{
    public class VehicleEventQueue : ConcurrentQueue<VehicleEvent>
    {
        private static VehicleEventQueue instance = new VehicleEventQueue();

        public static VehicleEventQueue Instance { get { return instance; } }
    }
}