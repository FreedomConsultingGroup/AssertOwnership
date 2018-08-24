using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCG.AssertOwnership
{
    public class Global
    {
        public static string BaseDirectory { get { return @"C:\inetpub\wwwroot\portal\Ownership"; } }

        public static string StaticDirectory { get { return BaseDirectory + @"\static"; } }

        public static string PortalUrl { get { return @"https://fcg-arcgis-srv.freedom.local/portal/"; } }
    }
}