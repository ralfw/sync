using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace sync.remotefilestore.parse.api
{
    public class ParseObjects
    {
        private const string BASE_URL = "https://api.parse.com/1/classes/";

        private readonly string _appId;
        private readonly string _restKey;


        public ParseObjects(string appId, string restKey)
        {
            _appId = appId;
            _restKey = restKey;
        }


        public string New(string classname, string jsondata)
        {
            var jsonresponse = Request("POST", BASE_URL + classname, jsondata);

            var jss = new JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, string>>(jsonresponse);
            return dict["objectId"];
        }


        public string this[string classname, string objectId]
        {
            get
            {
                return Request("GET", BASE_URL + classname + "/" + objectId, null);
            }

            set
            {
                Request("PUT", BASE_URL + classname + "/" + objectId, value);
            }
        }


        public void Delete(string classname, string objectId)
        {
            Request("DELETE", BASE_URL + classname + "/" + objectId, "");
        }


        public string Query(string classname)
        {
            return Request("GET", BASE_URL + "/" + classname, null);
        }

        public string Query(string classname, string where)
        {
            return Request("GET", BASE_URL + "/" + classname + "?where=" + HttpUtility.UrlEncode(where), null);
        }



        string Request(string method, string url, string data)
        {
            var req = WebRequest.Create(url);
            req.Method = method;
            req.Headers.Add("X-Parse-Application-Id", _appId);
            req.Headers.Add("X-Parse-REST-API-Key", _restKey);
            req.ContentType = "application/json";

            if (data != null)
                using (var sw = new StreamWriter(req.GetRequestStream()))
                {
                    sw.Write(data);
                }

            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
