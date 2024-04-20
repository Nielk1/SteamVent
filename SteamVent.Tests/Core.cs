using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Tests
{
    internal class Core
    {
        private static object configLock = new object();
        private static IConfiguration? config;
        public static IConfiguration Configuration
        {
            get
            {
                lock (configLock)
                {
                    config = config ?? new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        //.AddEnvironmentVariables()
                        .Build();
                    return config;
                }
            }
        }
    }
}
