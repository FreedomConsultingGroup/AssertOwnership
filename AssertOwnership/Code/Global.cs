using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCG.AssertOwnership
{
    public class Global
    {
        // Global variables used by different files, as well as things that may need to be changed by other users
        public const string BaseDirectory = @"C:\inetpub\wwwroot\portal\Ownership";

        public const string StaticDirectory = BaseDirectory + @"\static";

        public const string PortalUrl = @"https://fcg-arcgis-srv.freedom.local/portal/";
    }
}