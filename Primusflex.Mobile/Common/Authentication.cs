using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Json;

namespace Primusflex.Mobile.Common
{
    public static class Authentication
    {
        public static async Task<Intent> Start(Activity currActivity, string url, string userName, string password)
        {
            Intent activity;

            // Create an HTTP web request using the URL:
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            string postString = string.Format("grant_type={0}&username={1}&password={2}", "password", userName, password);
            request.ContentLength = postString.Length;

            CookieContainer cookies = new CookieContainer();
            request.CookieContainer = cookies;

            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
            requestWriter.Write(postString);
            requestWriter.Close();

            try
            {
                // Send the request to the server and wait for the response:
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException();
                }

                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    // Use this stream to build a JSON document object:
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));

                    // redirect to HomeActivity after successful login
                    activity = new Intent(currActivity, typeof(HomeActivity));
                    activity.PutExtra("access_token", (string)jsonDoc["access_token"]);
                    activity.PutExtra("user_name", userName);

                    // save 
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Use this reader(stream) to build a JSON document object:
                JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(reader));

                // show error message
                currActivity.FindViewById<ProgressBar>(Resource.Id.progressBarCircle).Visibility = ViewStates.Gone;
                var textViewErrorMessage = currActivity.FindViewById<TextView>(Resource.Id.textViewErrorMessage);
                textViewErrorMessage.Visibility = ViewStates.Visible;
                textViewErrorMessage.Text = jsonDoc["error_description"];

                return null;
            }

            return activity;
        }
    }
}