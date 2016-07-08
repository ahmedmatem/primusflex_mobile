using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Json;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using PrimusFlex.Mobile.Common;
using Primusflex.Mobile.Common;

namespace Primusflex.Mobile
{
    [Activity(Label = "PrimusFlex", Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.NoActionBar.Fullscreen")]
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
                
                var activity = await Authentication.Start(this, url, userName, password);
                if (activity != null)
                {
                    StartActivity(activity);
                }
            };
        }

        protected override void OnRestart()
        {
            base.OnRestart();

            ResetLoginActivity();

        }

        private void ResetLoginActivity()
        {
            this.FindViewById(Resource.Id.progressBarCircle).Visibility = ViewStates.Gone;
            this.FindViewById<EditText>(Resource.Id.editTextUserName).Text = "";
            this.FindViewById<EditText>(Resource.Id.editTextPassword).Text = "";
        }
    }
}

