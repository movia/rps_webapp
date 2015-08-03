using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Microsoft.Framework.Configuration;

namespace RpsPositionWebApp.Data
{
    public class ConfigurationManager
    {
        private static ConfigurationManager instance = new ConfigurationManager();

        public static ConfigurationManager Instance { get { return instance; } }

        public IConfiguration Configuration { get; private set; }

        public ConfigurationManager()
        {
            string basePath = HostingEnvironment.MapPath("~");

            var builder = new ConfigurationBuilder(basePath)
                .AddJsonFile("config/app.json");

            Configuration = builder.Build();
        }
    }
}