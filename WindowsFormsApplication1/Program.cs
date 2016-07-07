using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text;
using System.Web;


namespace WindowsFormsApplication1
{
    
    public class GAWrapper
    { 
        
       public static Dictionary<string,string> BuildScreenViewPostString(TelemetryType type, string appinstallid, string screenname) {

            var result = new Dictionary<string, string> {

                 { "v", "1" },
                 { "tid", "UA-49625868-2" },
                 { "cid", "5ea42fb3-1af0-4f7b-bde8-9a65f7bb84e8"},
                 { "t", type.ToString() },
                 {"an","Product1" }, //appname
                 {"av","1.0" }, //app version
                 { "aid", "00212" }, //app id
                 { "aiid", appinstallid}, //install id
                 { "cd", screenname },
             };
            return result;

        }
        public static Dictionary<string, string> BuildEventPostString(TelemetryType type, string category, string action, string label,int? value = null)
        {
            
            var result = new Dictionary<string, string> {
                              
                  { "v", "1" },
                  { "tid", "UA-49625868-2" },
                  { "cid", "5ea42fb3-1af0-4f7b-bde8-9a65f7bb84e8"},
                  { "t", type.ToString() },
                  { "ec", category },
                  { "ea", action },
               };
            if (!string.IsNullOrEmpty(label))
            {
                result.Add("el", label);
            }
            if (value.HasValue)
            {
                result.Add("ev", value.ToString());
            }

            return result;

        }
                      
        private static void PostRequest(Dictionary<string,string> stringData)
            {
                
                var request = (HttpWebRequest)WebRequest.Create("http://www.google-analytics.com/collect");
                request.Method = "POST";
                request.KeepAlive = false;

                var postDataString = stringData
                    .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                                                                 HttpUtility.UrlEncode(next.Value)))
                    .TrimEnd('&');

                // set the Content-Length header to the correct value
                request.ContentLength = Encoding.UTF8.GetByteCount(postDataString);

                // write the request body to the request
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postDataString);
                }

                try
                {
                    var webResponse = (HttpWebResponse)request.GetResponse();
                    if (webResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new HttpException((int)webResponse.StatusCode,
                                                "Google Analytics tracking did not return OK 200");
                    }
                webResponse.Close();
                }
                catch (Exception ex)
                {
                
                // do what you like here, we log to Elmah
                // ElmahLog.LogError(ex, "Google Analytics tracking failed");
            }

        }

        public static void TrackEvent(string category, string action, string label, int? value = null)
        {
            var stringToPost = new Dictionary<string, string> { };
            stringToPost = BuildEventPostString(TelemetryType.@event, category, action, label);
            PostRequest(stringToPost);
        }

        public static void TrackScreenView(string appinstallid, string screenname)

        {
            var stringToPost = new Dictionary<string, string> { };
            stringToPost = BuildScreenViewPostString(TelemetryType.screenview, appinstallid, screenname);
            PostRequest(stringToPost);
        }
          
        public enum TelemetryType
            {
                // ReSharper disable InconsistentNaming
                @event,
                @screenview,
                // ReSharper restore InconsistentNaming
            }
        }
  
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
