using System;
using System.Net;
using System.Web;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace AssertOwnership
{
    public class AssertOwnershipHandler : IHttpHandler
    {
        private OwnershipHelper helper = new OwnershipHelper();

        //set the reusable property to true
        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            /* ProcessRequest is automatically called by IIS when it receives a
               request to the url pointed to by web.config */

            HttpRequest request = context.Request;
            if (request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;
                return;
            }
            // Get the username from the identity of the request (which was set by PKIAuthenticationModule)
            string user = context.User.Identity.Name;
            // Get the info for the user
            JObject userInfo = helper.GetUserInfo(user);
            // If the user doesn't exist, exit with 403 response (Forbidden)
            if (userInfo["error"] != null || ((string)userInfo["level"]) != "2")
            {
                context.Response.StatusCode = 403;
                return;
            }

            // Get item ID, the new owner of the item, and the destination folder from the request parameters
            string itemID = request["itemid"];
            string newOwner = request["newowner"];
            string newFolder = request["newfolder"];

            // If the item ID or new owner aren't specified exit with 400 response (Client Error)
            if (itemID == null || newOwner == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            // If the destination folder is not specified, default to "/"
            if (newFolder == null)
            {
                newFolder = "";
            }

            JObject itemInfo = helper.GetItemInfo(itemID);
            if (itemInfo["ownerFolder"] == null)
            {
                itemInfo["ownerFolder"] = "";
            }

            /* Check to see if the item, the current owner, and the new owner all share a group.
               If not, then exit with 401 response (Unauthorized) */
            if (InvalidGroups(itemInfo, newOwner))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Generate a token to use with API resuests
            string token = helper.GenerateToken();

            JObject response = helper.StringToJson(helper.GetRequest(helper.portalUrl + "/sharing/rest/content/users/" + itemInfo["owner"] + "/" + itemInfo["ownerFolder"] + "/items/" + itemID + "/reassign",
                                         new string[] { "targetUsername", "targetFoldername", "token", "f" },
                                         new string[] { newOwner, newFolder, token, "json" }));

            // Return the word success if it correectly transfered ownership. Otherwise, return the error message
            if (response["success"] != null)
            {
                context.Response.Write("success");
                return;
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.Write((string)response["error"]);
                return;
            }
        }


        private bool InvalidGroups(JObject itemInfo, string newOwner)
        {
            /* Checks which group(s) the old owner and the item share, then check to see if the new owner is in those group(s) as well. */
            JToken itemGroupInfo = itemInfo["groups"]["member"];
            JObject oldUserInfo = helper.GetUserInfo((string)itemInfo["owner"]);
            JObject newUserInfo = helper.GetUserInfo(newOwner);

            bool matches = false;
            string[] matchingGroup = { };
            // Add matching groups to the groups to check in the new owner
            foreach (JToken itemGroup in itemGroupInfo)
            {
                foreach (JToken oldUserGroup in oldUserInfo["groups"])
                {
                    if (oldUserGroup["id"] != null && itemGroup["id"] == oldUserGroup["id"])
                    {
                        matchingGroup[matchingGroup.Length] = (string)itemGroup["id"];
                        break;
                    }
                }
            }

            if (matchingGroup.Length == 0) { return false; }

            // Check new owner for any of the matching groups
            foreach (JToken newUserGroup in newUserInfo["groups"])
            {
                if (matchingGroup.Contains((string)newUserGroup["id"]))
                {
                    matches = true;
                    break;
                }
            }

            return matches;
        }
    }


    public class OwnershipHelper
    {
        /* Contains helper functions that prevent writing the same code over and over. */

        /* Set the base url for the portal and get path of certificate */
        public readonly string portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";
        private readonly string certPath = Environment.GetEnvironmentVariable("ADMIN_CERT_PATH", EnvironmentVariableTarget.Machine);
        private X509Certificate2Collection collection;

        public OwnershipHelper()
        {
            //X509Certificate2 cert = new X509Certificate2(certPath, "", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
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
            // Generate a token for use with the API
            string jsonResponseString = GetRequest(portalUrl + "sharing/rest/generateToken",
                                                   new string[] { "client", "referer", "expiration", "f" },
                                                   new string[] { "referer", portalUrl, "60", "json" });

            JObject jsonResponseObject = JsonConvert.DeserializeObject<JObject>(jsonResponseString);

            return jsonResponseObject["token"].ToString();
        }


        public JObject GetItemInfo(string itemId)
        {
            // Get detailed information on an item from the portal
            string jsonResponseString = GetRequest(portalUrl + "sharing/rest/content/items/" + itemId,
                                                   new string[] { "f" },
                                                   new string[] { "json" });
            string groups = GetRequest(portalUrl + "sharing/rest/content/items/" + itemId + "/groups",
                                                   new string[] { "f" },
                                                   new string[] { "json" });

            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(jsonResponseString);
            jsonObj["groups"] = JsonConvert.DeserializeObject<JObject>(groups);
            return jsonObj;
        }


        public JObject GetUserInfo(string user)
        {
            // Get detailed information on a user from the portal
            string jsonResponseString = GetRequest(portalUrl + "sharing/rest/community/users/" + user,
                                                   new string[] { "f" },
                                                   new string[] { "json" });

            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(jsonResponseString);
            return jsonObj;
        }


        public JObject GetUserContent(string username, string folderId)
        {
            // Get user content for a specified folder
            return StringToJson(GetRequest(portalUrl + "sharing/rest/content/users/" + username + "/" + folderId,
                                 new string[] { "f" },
                                 new string[] { "json" }));
        }
    }
}