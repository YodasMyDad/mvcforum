using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace MVCForum.Utilities
{
    public static class LocalisationUtils
    {
        public static List<string> GeocodeGoogle(string postcode)
        {
            var longlat = new List<string>();
            var req = (HttpWebRequest)WebRequest.Create(string.Format("http://www.google.com/uds/GlocalSearch?q={0}&v=1.0", postcode));
            using (var resp = req.GetResponse())
            using (var respStream = resp.GetResponseStream())
            using (var reader = new StreamReader(respStream, true))
            {
                var response = reader.ReadToEnd();
                var serializer = new JavaScriptSerializer();
                var deserialized = (Dictionary<string, object>)serializer.DeserializeObject(response);
                var responseData = (Dictionary<string, object>)deserialized["responseData"];
                var results = (object[])responseData["results"];
                try
                {
                    var resultsData = (Dictionary<string, object>)results[0];
                    longlat.Add(resultsData["lat"].ToString());
                    longlat.Add(resultsData["lng"].ToString());
                    longlat.Add(resultsData["title"].ToString());
                }
                catch (Exception)
                {
                    longlat.Add("0");
                    longlat.Add("0");
                    longlat.Add("No Result");
                }
                return longlat;
            }
        }
    }
}
