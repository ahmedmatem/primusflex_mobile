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

namespace Primusflex.Mobile
{
    [Activity(Label = "History", Theme = "@style/CustomActionBarTheme")]
    public class HistoryActivity : Activity
    {
        string accessToken, userName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            accessToken = Intent.GetStringExtra("access_token");
            userName = Intent.GetStringExtra("user_name");

            // disable default ActionBar view and set custom view
            ActionBar.SetCustomView(Resource.Layout.action_bar);
            ActionBar.SetDisplayShowCustomEnabled(true);

            LinearLayout upload = (LinearLayout)FindViewById(Resource.Id.updateMenu);
            upload.Click += LoadUpload;

            var historyMenu = FindViewById<LinearLayout>(Resource.Id.historyMenu);
            historyMenu.Selected = true;

            SetContentView(Resource.Layout.History);

            TextView tv = FindViewById<TextView>(Resource.Id.textView1);
            tv.Text = Intent.GetStringExtra("access_token");
        }

        private void LoadUpload(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(HomeActivity));
            intent.PutExtra("access_token", accessToken);
            intent.PutExtra("user_name", userName);
            StartActivity(intent);
        }
    }
}