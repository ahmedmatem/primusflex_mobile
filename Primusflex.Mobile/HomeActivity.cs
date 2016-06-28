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
using Primusflex.Mobile.Common;
using Java.IO;
using Android.Provider;

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

            CameraHelpers.CreateDirectoryForPictures();

            Button btnOpenCamera = FindViewById<Button>(Resource.Id.btnOpenCamera);
            btnOpenCamera.Click += TakeAPicture;

            //string text = Intent.GetStringExtra("access_token");
            //string error = Intent.GetStringExtra("error_description");
            //if (error != null)
            //{
            //    text = error;
            //}

            //var textView = this.FindViewById<TextView>(Resource.Id.textView1);
            //textView.Text = text;

        }

        private void TakeAPicture(object sender, EventArgs e)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App.File = new File(App.Dir, String.Format("img_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App.File));
            StartActivityForResult(intent, 0);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App.File);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);
        }
    }
}