using System;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FCG.AssertOwnership
{
    public class OwnershipHelper
    {
        /* Contains helper functions that prevent writing the same code over and over. */

        /* Set the base url for the portal and get path of certificate */
        private readonly string certPath = Environment.GetEnvironmentVariable("ADMIN_CERT_PATH", EnvironmentVariableTarget.Machine);
        private HttpClient client;
        private static OwnershipHelper instance = new OwnershipHelper();

        private OwnershipHelper()
        {
            byte[] certFileBinary = File.ReadAllBytes(certPath + "cgoodTEMP.pfx");
            string passwd = File.ReadAllText(certPath + "passwd.txt");

            X509Certificate2 cert = new X509Certificate2();
            cert.Import(certFileBinary, passwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            WebRequestHandler handler = new WebRequestHandler();
            handler.ClientCertificates.Add(cert);

            client = new HttpClient(handler);
        }


        public static OwnershipHelper getInstance()
        {
            return instance;
        }


        public async Task<string> Request(string url, string[] keys, string[] values, string method)
        {
            // Send a get request to the specified portal url, with the parameters specified by the keys and values arrays
            if (method == "GET")
            {
                string parameters = EncodeParamsGet(keys, values);
                string responseString = await client.GetStringAsync(url + "?" + parameters).ConfigureAwait(false);
                return responseString;
            }
            else if (method == "POST")
            {
                HttpContent parameters = EncodeParamsPost(keys, values);
                HttpResponseMessage response = await client.PostAsync(url, parameters).ConfigureAwait(false);
                string responseString = "";
                if (response.IsSuccessStatusCode)
                {
                    responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    responseString = "error";
                }
                return responseString;
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        public string EncodeParamsGet(string[] keys, string[] values)
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

        private HttpContent EncodeParamsPost(string[] keys, string[] values)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            for (int i = 0; i < keys.Length; i++)
            {
                dict.Add(keys[i], values[i]);
            }
            return new FormUrlEncodedContent(dict);
        }

        public T DeserializeJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }


        public string JsonToString(JObject json)
        {
            return JsonConvert.SerializeObject(json);
        }


        public string GenerateToken()
        {
            // Generate a token for use with the API
            string jsonResponseString = Request(Global.PortalUrl + "sharing/rest/generateToken",
                                                   new string[] { "client", "referer", "expiration", "f" },
                                                   new string[] { "referer", Global.PortalUrl, "60", "json" },
                                                   "GET").Result;

            JObject jsonResponseObject = JsonConvert.DeserializeObject<JObject>(jsonResponseString);

            return jsonResponseObject["token"].ToString();
        }


        public JObject GetItemInfo(string itemId)
        {
            // Get detailed information on an item from the portal
            string jsonResponseString = Request(Global.PortalUrl + "sharing/rest/content/items/" + itemId,
                                                   new string[] { "f" },
                                                   new string[] { "json" },
                                                   "GET").Result;
            JObject groups = JsonConvert.DeserializeObject<JObject>(Request(Global.PortalUrl + "sharing/rest/content/items/" + itemId + "/groups",
                                                   new string[] { "f" },
                                                   new string[] { "json" },
                                                   "GET").Result);

            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(jsonResponseString);
            jsonObj["groups"] = groups["admin"];
            foreach (JToken group in groups["member"])
            {
                jsonObj["groups"].Last.AddAfterSelf(group);
            }
            foreach (JToken group in groups["other"])
            {
                jsonObj["groups"].Last.AddAfterSelf(group);
            }
            return jsonObj;
        }


        public JObject GetUserInfo(string user)
        {
            // Get detailed information on a user from the portal
            string jsonResponseString = Request(Global.PortalUrl + "sharing/rest/community/users/" + user,
                                                   new string[] { "f" },
                                                   new string[] { "json" },
                                                   "GET").Result;

            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(jsonResponseString);
            return jsonObj;
        }


        public JObject GetUserContent(string username, string folderId)
        {
            // Get user content for a specified folder
            return DeserializeJson<JObject>(Request(Global.PortalUrl + "sharing/rest/content/users/" + username + "/" + folderId,
                                 new string[] { "f" },
                                 new string[] { "json" },
                                 "GET").Result);
        }


        public JObject GetGroupContent(string groupId)
        {
            return DeserializeJson<JObject>(Request(Global.PortalUrl + "sharing/rest/content/groups/" + groupId,
                                new string[] { "f" },
                                new string[] { "json" },
                                "GET").Result);
        }
    }
}