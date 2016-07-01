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
using Primusflex.Mobile.Common;

namespace Primusflex.Mobile
{
    [Activity(Label = "PrimusFlex", MainLauncher = true, Icon = "@drawable/icon")]
    public class StartActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Detect if there is network connection

            ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

            if (isOnline)
            {
                // Go to Login activity

                Intent loginActivity = new Intent(this, typeof(LoginActivity));
                StartActivity(loginActivity);
            }
            else
            {
                // Go to OfflineHome activity

                Intent offlineHomeActivity = new Intent(this, typeof(OfflineHomeActivity));
                StartActivity(offlineHomeActivity);
            }
        }
    }
}