﻿using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Layout;


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
                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

                PatternLayout pattern = new PatternLayout();
                pattern.ConversionPattern = "%date{MM-dd HH:mm} [%thread] %-5level %logger [%property{NDC}] - %message%newline";
                pattern.ActivateOptions();

                RollingFileAppender rfa = new RollingFileAppender();
                rfa.Layout = pattern;
                rfa.AppendToFile = true;
                rfa.File = System.Environment.GetEnvironmentVariable("DOT_NET_LOG_PATH") + @"AssertOwnership\AssertOwnership.log";
                rfa.RollingStyle = RollingFileAppender.RollingMode.Composite;
                rfa.DatePattern = ".yyyy-MM-dd";
                rfa.MaxSizeRollBackups = 10;
                rfa.StaticLogFileName = true;
                rfa.MaximumFileSize = "1MB";
                rfa.ActivateOptions();

                hierarchy.Root.AddAppender(rfa);

                hierarchy.Root.Level = Level.All;
                hierarchy.Configured = true;

                Log = LogManager.GetLogger(typeof(Global));
                Log.Info("Logging started");
            }
            Log.Info(message);
        }
    }
}