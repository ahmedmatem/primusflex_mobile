using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using PrimusFlex.Mobile.Android.Common;
using System.Json;

namespace Primusflex.Mobile
{
    [Activity(Label = "PrimusFlex", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Login);

            var btnLogin = this.FindViewById<Button>(Resource.Id.btnLogin);
            btnLogin.Click += async (sender, e) =>
            {
                // start loading circle image (progress bar)
                FindViewById<TextView>(Resource.Id.textViewErrorMessage).Visibility = ViewStates.Gone;
                var progressBarCircle = FindViewById<ProgressBar>(Resource.Id.progressBarCircle);
                progressBarCircle.Visibility = ViewStates.Visible;

                var userName = this.FindViewById<EditText>(Resource.Id.editTextUserName).Text;
                var password = this.FindViewById<EditText>(Resource.Id.editTextPassword).Text;
                string url = Constant.TokenRequestUrl;

                var activity = await StartAuthentication(url, userName, password);
                if (activity != null)
                {
                    StartActivity(activity);
                }
            };
        }

        private async Task<Intent> StartAuthentication(string url, string userName, string password)
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
                    activity = new Intent(this, typeof(HomeActivity));
                    activity.PutExtra("access_token", (string)jsonDoc["access_token"]);
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Use this reader(stream) to build a JSON document object:
                JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(reader));

                // show error message
                FindViewById<ProgressBar>(Resource.Id.progressBarCircle).Visibility = ViewStates.Gone;
                var textViewErrorMessage = this.FindViewById<TextView>(Resource.Id.textViewErrorMessage);
                textViewErrorMessage.Visibility = ViewStates.Visible;
                textViewErrorMessage.Text = jsonDoc["error_description"];

                return null;
            }

            return activity;
        }
    }
}

