using log4net;
using log4net.Config;
using System.IO;

namespace FCG.AssertOwnership
{
    public class Global
    {
        // Global variables used by different files, as well as things that may need to be changed by other users
        public const string RootDirectory = @"C:\inetpub\";

        public const string BaseDirectory = RootDirectory + @"wwwroot\portal\Ownership\";

        public const string StaticDirectory = BaseDirectory + @"static";

        public const string PortalUrl = @"https://fcg-arcgis-srv.freedom.local/portal/";
        
        private static readonly string LogConfig = BaseDirectory + @"log4net.config";
        private static ILog Log = null;
        
        public static void LogInfo(string message)
        {
            if(Log == null)
            {
                Log = LogManager.GetLogger(typeof(Global));
                XmlConfigurator.Configure(new FileInfo(LogConfig));
                Log.Info("Logging started");
            }
            Log.Info(message);
        }
    }
}