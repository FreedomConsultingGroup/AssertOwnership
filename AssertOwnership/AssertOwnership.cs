using System;
using System.Net;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AssertOwnership
{
    public class AssertOwnershipHandler : IHttpHandler
    {
        private readonly string portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";
        private readonly string certPath = Environment.GetEnvironmentVariable("ADMIN_CERT");

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            // TODO do actual trasfer request here
        }


        public string GetRequest(string url, string[] keys, string[] values)
        {
            string parameters = UrlEncodeQuery(keys, values);
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url + "?" + parameters);

            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(certPath, "", X509KeyStorageFlags.PersistKeySet);
            request.ClientCertificates = collection;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }


        public string UrlEncodeQuery(string[] keys, string[] values)
        {
            if(keys.Length != values.Length)
            {
                throw new ArgumentException("Length of array \"keys\" must match length of array \"values\".");
            }else if(keys.Length < 1 || values.Length < 1)
            {
                throw new ArgumentException("Arrays must be of at least length 1");
            }

            string parameters = "";
            parameters += HttpUtility.UrlEncode(keys[0]) + "=" + HttpUtility.UrlEncode(values[0]);
            for (int i = 1; i < keys.Length; i++)
            {
                parameters += "&" + HttpUtility.UrlEncode(keys[i]) + "=" + HttpUtility.UrlEncode(values[i]);
            }

            return parameters;
        }


        public string GenerateToken()
        {
            string jsonResponseString = GetRequest(portalUrl + "sharing/rest/generateToken",
                                                   new string[] { "client", "referer", "expiration", "f" },
                                                   new string[] { "referer", portalUrl, "60", "json" });

            JObject jsonResponseObject = JsonConvert.DeserializeObject<JObject>(jsonResponseString);

            return jsonResponseObject["token"].ToString();
        }


        public JObject GetItemInfo(string token, string itemId)
        {
            string jsonResponseString = GetRequest(portalUrl + "sharing/content/items/" + itemId,
                                                   new string[] { "token", "f" },
                                                   new string[] { token, "json" });

            return JsonConvert.DeserializeObject<JObject>(jsonResponseString);
        }
    }
}