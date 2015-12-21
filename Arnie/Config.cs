using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Arnie.Config
{
    public class ArnieConfig : ConfigurationSection
    {
        private static ArnieConfig _Settings = ConfigurationManager.GetSection("arnieConfig") as ArnieConfig;

        public static ArnieConfig Settings
        {
            get { return _Settings; }
        }

        [ConfigurationProperty("allowedAddresses", DefaultValue= "127.0.0.1,::1")]
        public string AllowedAddresses
        {
            get { return this["allowedAddresses"] as string; }
            set { this["allowedAdresses"] = value; }
        }

        [ConfigurationProperty("queuePath", DefaultValue = @".\private$\dscAutomation")]
        public string QueuePath
        {
            get { return this["queuePath"] as string; }
            set { this["queuePath"] = value; }
        }
    }
}