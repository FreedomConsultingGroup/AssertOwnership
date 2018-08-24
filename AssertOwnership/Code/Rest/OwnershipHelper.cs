using System;
using System.Web;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FCG.AssertOwnership
{
    public class OwnershipHelper
    {
        /* Contains helper functions that prevent writing the same code over and over. */

        /* Set the base url for the portal and get path of certificate */
        private readonly string certPath = Environment.GetEnvironmentVariable("ADMIN_CERT_PATH", EnvironmentVariableTarget.Machine);
        private X509Certificate2Collection collection;

        public OwnershipHelper()
        {
            byte[] certFileBinary = File.ReadAllBytes(certPath + "cgoodTEMP.pfx");
            string passwd = File.ReadAllText(certPath + "passwd.txt");
            X509Certificate2 cert = new X509Certificate2();
            cert.Import(certFileBinary, passwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            this.collection = new X509Certificate2Collection(cert);
        }


        public string GetRequest(string url, string[] keys, string[] values)
        {
            // Send a get request to the specified portal url, with the parameters specified by the keys and values arrays
            string parameters = UrlEncodeQuery(keys, values);
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url + "?" + parameters);

            request.ClientCertificates = this.collection;

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }


        public JObject StringToJson(string json)
        {
            return JsonConvert.DeserializeObject<JObject>(json);
        }


        public string JsonToString(JObject json)
        {
            return JsonConvert.SerializeObject(json);
        }


        public string UrlEncodeQuery(string[] keys, string[] values)
        {
            // Encode the keys and values into URL compliant get parameters
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
            // Generate a token for use with the API
            string jsonResponseString = GetRequest(Global.PortalUrl + "sharing/rest/generateToken",
                                                   new string[] { "client", "referer", "expiration", "f" },
                                                   new string[] { "referer", Global.PortalUrl, "60", "json" });

            JObject jsonResponseObject = JsonConvert.DeserializeObject<JObject>(jsonResponseString);

            return jsonResponseObject["token"].ToString();
        }


        public JObject GetItemInfo(string itemId)
        {
            // Get detailed information on an item from the portal
            string jsonResponseString = GetRequest(Global.PortalUrl + "sharing/rest/content/items/" + itemId,
                                                   new string[] { "f" },
                                                   new string[] { "json" });
            string groups = GetRequest(Global.PortalUrl + "sharing/rest/content/items/" + itemId + "/groups",
                                                   new string[] { "f" },
                                                   new string[] { "json" });

            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(jsonResponseString);
            jsonObj["groups"] = JsonConvert.DeserializeObject<JObject>(groups);
            return jsonObj;
        }


        public JObject GetUserInfo(string user)
        {
            // Get detailed information on a user from the portal
            string jsonResponseString = GetRequest(Global.PortalUrl + "sharing/rest/community/users/" + user,
                                                   new string[] { "f" },
                                                   new string[] { "json" });

            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(jsonResponseString);
            return jsonObj;
        }


        public JObject GetUserContent(string username, string folderId)
        {
            // Get user content for a specified folder
            return StringToJson(GetRequest(Global.PortalUrl + "sharing/rest/content/users/" + username + "/" + folderId,
                                 new string[] { "f" },
                                 new string[] { "json" }));
        }


        public JObject GetGroupContent(string groupId)
        {
            return StringToJson(GetRequest(Global.PortalUrl + "sharing/rest/content/groups/" + groupId,
                                new string[] { "f" },
                                new string[] { "json" }));
        }
    }
}