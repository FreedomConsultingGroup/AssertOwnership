﻿using System;
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
        /* set the base url for the portal and get path of certificate */
        private readonly string portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";
        private readonly string certPath = Environment.GetEnvironmentVariable("ADMIN_CERT");

        //set the reusable property to true
        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            /* ProcessRequest is automatically called by IIS when it receives a
               request to the url pointed to by web.config */

            // Get the username from the identity of the request (which was set by PKIAuthenticationModule)
            string user = context.User.Identity.Name;
            // Get the info for the user
            JObject userInfo = JObject.Parse(GetRequest(portalUrl + "sharing/rest/community/users/" + user,
                                                                      new string[] { "f" },
                                                                      new string[] { "json" }));
            // If the user doesn't exist, exit with 403 response (Forbidden)
            if (userInfo["error"] != null || ((string)userInfo["level"]) != "2")
            {
                context.Response.StatusCode = 403;
                return;
            }

            // Get item ID, the new owner of the item, and the destination folder from the request parameters
            HttpRequest request = context.Request;
            string itemID = request.QueryString["itemid"];
            string newOwner = request.QueryString["newowner"];
            string newFolder = request.QueryString["newfolder"];

            // If the item ID or new owner aren't specified exit with 400 response (Client Error)
            if (itemID == null || newOwner == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            // If the destination folder is not specified, default to "/"
            JObject itemInfo = GetItemInfo(itemID);
            if (itemInfo["ownerFolder"] == null)
            {
                itemInfo["ownerFolder"] = "/";
            }

            /* Check to see if the item, the current owner, and the new owner all share a group.
               If not, then exit with 401 response (Unauthorized) */
            if (InvalidGroups(itemInfo, newOwner))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Generate a token to use with API resuests
            string token = GenerateToken();

            JObject response = JsonConvert.DeserializeObject<JObject>(GetRequest(portalUrl + "/sharing/rest/content/users/" + itemInfo["owner"] + "/" + itemInfo["ownerFolder"] + "/items/" + itemID + "/reassign",
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
            JObject oldUserInfo = GetUserInfo((string)itemInfo["owner"]);
            JObject newUserInfo = GetUserInfo(newOwner);

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


        public string GetRequest(string url, string[] keys, string[] values)
        {
            // Send a get request to the specified portal url, with the parameters specified by the keys and values arrays
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
    }
}