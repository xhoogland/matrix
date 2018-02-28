using Microsoft.Extensions.Configuration;
using System;

namespace Matrix.Parsers
{
    class ParserHelper
    {
        public static IConfigurationSection GetLvmsConfig()
        {
            var confBuilder = new ConfigurationBuilder()
                             .SetBasePath(Environment.CurrentDirectory)
                             .AddJsonFile("appsettings.json", true, true);
            return confBuilder.Build().GetSection("Lvms");
        }
    }
}
