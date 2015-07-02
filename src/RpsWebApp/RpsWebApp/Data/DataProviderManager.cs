using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using RpsPositionWebApp.DataProviders.Rps;

namespace RpsPositionWebApp.Data
{
    public class DataProviderManager
    {
        private static DataProviderManager instance = new DataProviderManager();

        public static DataProviderManager Instance { get { return instance; } }

        private List<RpsAdapter> positionAdapters;

        public List<RpsAdapter> PositionAdapters { get { return positionAdapters; } }

        public DataProviderManager()
        {
            positionAdapters = new List<RpsAdapter>();
        }

        public void Startup(CancellationToken token) {
            foreach (var adapter in positionAdapters)
                Task.Factory.StartNew(adapter.Run, TaskCreationOptions.LongRunning);
        }
    }
}