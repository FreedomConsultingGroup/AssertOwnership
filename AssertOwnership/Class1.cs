using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Security.Cryptography.X509Certificates

namespace AssertOwnership
{
    public class Class1
    {
        private const string portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";
        public string GenerateToken()
        {
            string certPath = Environment.GetEnvironmentVariable("ADMIN_CERT");

            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(portalUrl + "sharing/rest/generateToken");

            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(certPath, "", X509KeyStorageFlags.PersistKeySet);
            request.ClientCertificates = collection;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        }
    }
}