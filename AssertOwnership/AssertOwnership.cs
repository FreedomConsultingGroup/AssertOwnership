using System;
using System.Net;
using System.Web;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AssertOwnership
{
    public class AssertOwnershipHandler : IHttpHandler
    {
        private readonly string portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";
        private readonly string certPath = Environment.GetEnvironmentVariable("ADMIN_CERT");

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            string user = context.User.Identity.Name;
            JObject userInfo = JsonConvert.DeserializeObject<JObject>(GetRequest(portalUrl + "sharing/rest/community/users/" + user,
                                                                      new string[] { "f" },
                                                                      new string[] { "json" }));
            if (userInfo["error"] != null)
            {
                context.Response.StatusCode = 403;
                return;
            }
            else if (((string)userInfo["level"]) != "2")
            {
                context.Response.StatusCode = 403;
                return;
            }

            HttpRequest request = context.Request;
            string itemID = request.QueryString["itemid"];
            string newOwner = request.QueryString["newowner"];
            string newFolder = request.QueryString["newfolder"];

            if (itemID == null || newOwner == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            string token = GenerateToken();

            JObject itemInfo = GetItemInfo(token, itemID);
            if (itemInfo["ownerFolder"] == null)
            {
                itemInfo["ownerFolder"] = "/";
            }

            JObject response = JsonConvert.DeserializeObject<JObject>(GetRequest(portalUrl + "/sharing/rest/content/users/" + itemInfo["owner"] + "/" + itemInfo["ownerFolder"] + "/items/" + itemID + "/reassign",
                                         new string[] { "targetUsername", "targetFoldername", "token", "f" },
                                         new string[] { newOwner, newFolder, token, "json" }));

            if (response["success"] != null)
            {
                return;
            }
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
            if (keys.Length != values.Length)
            {
                throw new ArgumentException("Length of array \"keys\" must match length of array \"values\".");
            }
            else if (keys.Length < 1 || values.Length < 1)
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