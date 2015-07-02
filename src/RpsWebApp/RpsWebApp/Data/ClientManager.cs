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

        public HashSet<string> ConnectedIds = new HashSet<string>();
    }
}