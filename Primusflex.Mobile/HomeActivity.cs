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
    [Activity(Label = "Welcome", Theme = "@style/CustomActionBarTheme")]
    public class HomeActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // disable default ActionBar view and set custom view
            ActionBar.SetCustomView(Resource.Layout.action_bar);
            ActionBar.SetDisplayShowCustomEnabled(true);

            SetContentView(Resource.Layout.Home);

            var updateMenu = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            updateMenu.Selected = true;
            updateMenu.Click += (snder, e) =>
            {
                
            };
            //string text = Intent.GetStringExtra("access_token");
            //string error = Intent.GetStringExtra("error_description");
            //if (error != null)
            //{
            //    text = error;
            //}

            //var textView = this.FindViewById<TextView>(Resource.Id.textView1);
            //textView.Text = text;

        }
    }
}