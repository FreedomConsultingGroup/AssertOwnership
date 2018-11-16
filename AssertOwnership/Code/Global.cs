using System.Collections.Generic;
using System;
using System.Xml;
using log4net.Config;
using log4net;

[assembly: XmlConfigurator(ConfigFile=@"C:\inetpub\wwwroot\portal\Ownership\log4net.config", Watch=true)]

namespace FCG.AssertOwnership
{
    public static class Global
    {
        private static readonly XmlElement config = Configure();

        // Global variables used by different files, as well as things that may need to be changed by other users
        public static readonly string RootDirectory = config.SelectNodes("RootDirectory")[0].InnerText;
        public static readonly string BaseDirectory = RootDirectory + config.SelectNodes("BaseDirectory")[0].InnerText;
        public static readonly string StaticDirectory = BaseDirectory + config.SelectNodes("StaticDirectory")[0].InnerText;
        public static readonly string PortalUrl = config.SelectNodes("PortalUrl")[0].InnerText;

        // Whitelist of groups to ignore when transfering ownership. Currently only contains the featured maps group
        public static readonly string[] GroupWhitelist = GetWhitelistedGroups();

        public static readonly APIHttpHandler[] RestHandlers = GetImplementedRestClasses();

        private static ILog Log = null;
        
        public static void LogInfo(string message)
        {
            if(Log == null)
            {
                Log = LogManager.GetLogger(typeof(Global));
                Log.Info("Logging started");
            }
            Log.Info(message);
        }

        // Returns an array with instances of all classes that are subclasses of APIHttpHandler
        private static APIHttpHandler[] GetImplementedRestClasses()
        {
            List<APIHttpHandler> handlers = new List<APIHttpHandler>();
            // Get assemblies used in the current domain
            foreach (var domain in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Get all types in each assembly
                foreach (Type t in domain.GetExportedTypes())
                {
                    // Check if it inherits from APIHttpHandler
                    if (t.IsSubclassOf(typeof(APIHttpHandler)) && !t.IsAbstract)
                    {
                        // Initialize the object and add it to the list
                        handlers.Add((APIHttpHandler)Activator.CreateInstance(t));
                    }
                }
            }
            return handlers.ToArray();
        }

        private static XmlElement Configure()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Environment.GetEnvironmentVariable("ASSERT_OWNERSHIP_CONFIG"));
            return doc.DocumentElement;
        }

        private static string[] GetWhitelistedGroups()
        {
            XmlNodeList nodes = config.SelectNodes("WhiteListedGroups/Group");
            string[] whiteList = new string[nodes.Count];
            int i = 0;
            foreach (XmlNode node in nodes)
            {
                whiteList[i++] = node.InnerText;
            }
            return whiteList;
        }
    }
}