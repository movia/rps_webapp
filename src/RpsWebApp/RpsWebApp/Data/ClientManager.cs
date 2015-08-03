using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RpsPositionWebApp.Data
{
    public class ClientManager
    {
        private static ClientManager instance = new ClientManager();

        public static ClientManager Instance { get { return instance; } }

        private Dictionary<string, Dictionary<string, object>> configuration;

        public HashSet<string> ConnectedIds { get; private set; }

        public ClientManager()
        {
            ConnectedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            configuration = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, object> GetConfiguration(string clientId)
        {
            if (!configuration.ContainsKey(clientId))
                configuration.Add(clientId, new Dictionary<string, object>());

            return configuration[clientId];
        }

    }
}