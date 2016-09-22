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
using Android.Net;

using PrimusFlex.Mobile.Common;
using Android.Telephony;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace Primusflex.Mobile
{
    [Activity(Label = "PrimusFlex", MainLauncher = true, Icon = "@drawable/icon")]
    public class StartActivity : Activity
    {
        protected string access_token, userName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Detect if there is network connection

            ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

            var phoneState = new PrimusFlex.Mobile.Common.PhoneState((TelephonyManager)GetSystemService(TelephonyService));
            var imei = phoneState.IMEI();

            if (isOnline)
            {
                // if phome IMEI already is saved try to login by(phone)IMEI

                if (TryToLoginByIMEI(imei))
                {
                    Intent homeActivity = new Intent(this, typeof(HomeActivity));
                    homeActivity.PutExtra("access_token", access_token);
                    homeActivity.PutExtra("user_name", userName);
                    StartActivity(homeActivity);
                }
                else
                {
                    // Go to Login activity

                    Intent loginActivity = new Intent(this, typeof(LoginActivity));
                    StartActivity(loginActivity);

                }
            }
            else
            {
                // Go to OfflineHome activity

                Intent offlineHomeActivity = new Intent(this, typeof(OfflineHomeActivity));
                StartActivity(offlineHomeActivity);
            }

            Finish();
        }

        private bool TryToLoginByIMEI(string imei)
        {
            string responseFromServer;
            var uri = Constant.REMOTE_IMEI_LOGIN_URI + "?imei=" + imei;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream stream = response.GetResponseStream())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // read response content
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            responseFromServer = reader.ReadToEnd();
                            access_token = ExtractAccessToken(responseFromServer);
                            userName = ExtractUserName(responseFromServer);

                            return true;
                        }
                    }
                }
            }
            catch(WebException ex)
            {
                var pageContent = new StreamReader(ex.Response.GetResponseStream())
                          .ReadToEnd();
            }

            return false;
        }

        private string ExtractAccessToken(string responseFromServer)
        {
            var value = responseFromServer.Split(new char[] { ':', '\"', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            return value[1];
        }

        private string ExtractUserName(string responseFromServer)
        {
            var value = responseFromServer.Split(new char[] { ':', '\"', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            return value.Last();
        }
    }
}