using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace Matrix.Parsers
{
    public class ParserHelper
    {
        public static IConfigurationSection GetLvmsConfig()
        {
            var confBuilder = new ConfigurationBuilder()
                             .SetBasePath(Environment.CurrentDirectory)
                             .AddJsonFile("appsettings.json", true, true);
            return confBuilder.Build().GetSection("Lvms");
        }

        public static string GetAssemblyName()
        {
            var type = MethodBase.GetCurrentMethod().DeclaringType;
            return Assembly.GetAssembly(type).FullName;
        }
    }
}
