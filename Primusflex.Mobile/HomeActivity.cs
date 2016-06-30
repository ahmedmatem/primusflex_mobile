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
using Android.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Android.Graphics;

namespace Primusflex.Mobile
{
    [Activity(Label = "Welcome", Theme = "@style/CustomActionBarTheme")]
    public class HomeActivity : Activity
    {
        // Use the shared access signature (SAS) to perform container operations
        string sas = "https://primusflex.blob.core.windows.net/image-container?sv=2015-04-05&sr=c&sig=n85Ud0pAbIHZEXK6tJF1I%2FxnT7AK6ic2mN9t2MI%2FjBE%3D&se=2016-07-01T11%3A56%3A34Z&sp=rwdl";

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
            App.File = new Java.IO.File(App.Dir, String.Format("img_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App.File));
            StartActivityForResult(intent, 0);
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App.File);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // upload image from camera to azure blob storage (in photos-container)
            
            if (resultCode == Result.Ok)
            {
                //var bitmap = contentUri.Path.LoadAndResizeBitmap(100, 100);

                await UseContainerSAS(sas);
            }
        }

        private async Task UseContainerSAS(string sas)
        {
            CloudBlobContainer container = new CloudBlobContainer(new System.Uri(sas));
            try
            {
                //Write operation: write a new blob to the container.
                CloudBlockBlob blob = container.GetBlockBlobReference(App.File.Name);

                await blob.UploadFromFileAsync(App.File.Path);
            }
            catch (Exception e)
            {
                // TODO:
            }
        }
    }
}