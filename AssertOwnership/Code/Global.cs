using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Layout;
using System.Collections.Generic;
using System;

namespace FCG.AssertOwnership
{
    public class Global
    {
        // Global variables used by different files, as well as things that may need to be changed by other users
        public const string RootDirectory = @"C:\inetpub\";

        public const string BaseDirectory = RootDirectory + @"wwwroot\portal\Ownership\";

        public const string StaticDirectory = BaseDirectory + @"static";

        public const string PortalUrl = @"https://fcg-arcgis-srv.freedom.local/portal/";

        // Whitelist of groups to ignore when transfering ownership. Currently only contains the featured maps group
        public static readonly string[] GroupWhitelist = new string[] { "6349c41193684f6399faeadc19dff9d4" };

        public static readonly RestHttpHandler[] RestHandlers = GetImplementedRestClasses();

        private static ILog Log = null;
        
        public static void LogInfo(string message)
        {
            if(Log == null)
            {
                LogSetup();
                Log = LogManager.GetLogger(typeof(Global));
                Log.Info("Logging started");
            }
            Log.Info(message);
        }

        // Set configuration for logging
        public static void LogSetup()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout pattern = new PatternLayout();
            pattern.ConversionPattern = "%date{MM-dd HH:mm} [%thread] %-5level %logger [%property{NDC}] - %message%newline";
            pattern.ActivateOptions();

            RollingFileAppender rfa = new RollingFileAppender
            {
                Layout = pattern,
                AppendToFile = true,
                File = @"C:\inetpub\logs\AssertOwnership\AssertOwnership.log",
                RollingStyle = RollingFileAppender.RollingMode.Composite,
                DatePattern = ".yyyy-MM-dd",
                MaxSizeRollBackups = 10,
                StaticLogFileName = true,
                MaximumFileSize = "1MB"
            };
            rfa.ActivateOptions();

            hierarchy.Root.AddAppender(rfa);

            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }

        // Returns an array with instances of all classes that are subclasses of RestHttpHandler
        private static RestHttpHandler[] GetImplementedRestClasses()
        {
            List<RestHttpHandler> handlers = new List<RestHttpHandler>();
            // Get assemblies used in the current domain
            foreach (var domain in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Get all types in each assembly
                foreach (Type t in domain.GetExportedTypes())
                {
                    // Check if it inherits from RestHttpHandler
                    if (t.IsSubclassOf(typeof(RestHttpHandler)) && !t.IsAbstract)
                    {
                        // Initialize the object and add it to the list
                        handlers.Add((RestHttpHandler)Activator.CreateInstance(t));
                    }
                }
            }
            return handlers.ToArray();
        }
    }
}