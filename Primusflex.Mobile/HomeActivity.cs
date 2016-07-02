using System;
using System.Configuration;
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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using PrimusFlex.Mobile.Android.Common;
using System.ComponentModel;

namespace Primusflex.Mobile
{
    [Activity(Label = "Welcome", Theme = "@style/CustomActionBarTheme")]
    public class HomeActivity : Activity
    {
        //Parse the connection string and return a reference to the storage account.
        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=primusflex;AccountKey=1N+U65eUzC1GpNNuJ9JnMBsziPti12Nopj5WDUHGzDVJJFB2UHkC8boSkZ3li97yQ/qAZ22Ub+Mm2Xtw7diKNw==");

        //Create the blob client object.
        static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        //Get a reference to a container to use for the sample code, and create it if it does not exist.
        static CloudBlobContainer container = blobClient.GetContainerReference(Constant.IMAGE_STORAGE_CONTAINER_NAME);
        
        // Use the shared access signature (SAS) to perform container operations
        string sas = StorageHelpers.GetContainerSasUri(container);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // disable default ActionBar view and set custom view
            ActionBar.SetCustomView(Resource.Layout.action_bar);
            ActionBar.SetDisplayShowCustomEnabled(true);

            SetContentView(Resource.Layout.Home);

            var updateMenu = FindViewById<LinearLayout>(Resource.Id.updateMenu);
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

            if (resultCode == Result.Ok)
            {
                // Detect if there is network connection

                ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
                bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

                if (isOnline)
                {
                    // upload image from camera to azure blob storage

                    await UploadPhoto(sas);
                }
                else
                {
                    // Make it available in the gallery

                    Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                    Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App.File);
                    mediaScanIntent.SetData(contentUri);
                    SendBroadcast(mediaScanIntent);
                }
            }
        }

        private async Task UploadPhoto(string sas)
        {
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