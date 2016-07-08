using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Provider;
using Android.Net;
using Android.Graphics;


using PrimusFlex.Mobile.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Primusflex.Mobile
{
    [Activity(Label = "Welcome", Theme = "@style/CustomActionBarTheme")]
    public class HomeActivity : Activity
    {
        // Access Token
        string accessToken;

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

            // get access token
            accessToken = Intent.GetStringExtra("access_token");

            var updateMenu = FindViewById<LinearLayout>(Resource.Id.updateMenu);
            updateMenu.Selected = true;

            CameraHelpers.CreateDirectoryForPictures();

            Button btnOpenCamera = FindViewById<Button>(Resource.Id.btnOpenCamera);
            btnOpenCamera.Click += TakeAPicture;

            EditText siteName = FindViewById<EditText>(Resource.Id.site);
            siteName.TextChanged += (sender, e) => TryToEnableCameraButton(sender, e); 
            EditText plotNumber = FindViewById<EditText>(Resource.Id.plot);
            plotNumber.TextChanged += (sender, e) => TryToEnableCameraButton(sender, e);
            
        }

        public override void OnBackPressed()
        {
            // Do nothing
        }

        private void TryToEnableCameraButton(object sender, EventArgs e)
        {
            bool enableCameraButton = true;
            Button cameraButton = FindViewById<Button>(Resource.Id.btnOpenCamera);

            LinearLayout layoutPictureInfo = FindViewById<LinearLayout>(Resource.Id.layoutPictureInfo);

            for (int i = 0; i < layoutPictureInfo.ChildCount; i++)
            {
                var childView = layoutPictureInfo.GetChildAt(i);

                if (childView is EditText)
                {
                    EditText childViewAsEditText = (EditText)childView;
                    if (string.IsNullOrEmpty(childViewAsEditText.Text) ||
                        string.IsNullOrWhiteSpace(childViewAsEditText.Text))
                    {
                        enableCameraButton = false;
                        break;
                    }
                }
            }

            FindViewById<Button>(Resource.Id.btnOpenCamera).Enabled = enableCameraButton;
            cameraButton.Alpha = enableCameraButton ? 1 : Constant.BUTTON_ALPHA;
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
                    /* 
                     * collect picture info and
                     * save picture information to azure sql server
                     * info: SiteName, PlotNo, PictureName
                     */

                    string siteName = FindViewById<EditText>(Resource.Id.site).Text;
                    string plotNumber = FindViewById<EditText>(Resource.Id.plot).Text;
                    string picName = App.File.Name;

                    SavePictureInfo(siteName, plotNumber, picName);

                    // upload image from camera to azure blob storage

                    App.Bitmap = App.File.Path.LoadBitmap();
                    await UploadPhoto(sas);

                    // delete picture from gallery

                    if (App.File.Exists())
                    {
                        App.File.Delete();
                    }
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

        private void SavePictureInfo(string siteName, string plotNumber, string picName)
        {
            /*
             * use storage web service to save picture information
             * http://primusflexwebservices.azurewebsites.net/api/storage/pictureinfo
             */

            System.Uri uri = new System.Uri(Constant.STORAGE_URL + "/pictureInfo");
            WebClient client = new WebClient();
            string postString = string.Format("SiteName={0}&PlotNumber={1}&PictureName={2}", siteName, plotNumber, picName);
            byte[] postBytes = Encoding.UTF8.GetBytes(postString);
            client.Headers.Add("Authorization", "Bearer " + accessToken);
            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=utf-8");
            client.UploadData(uri, postBytes);
        }

        private async Task UploadPhoto(string sas)
        {
            try
            {
                //Write operation: write a new blob to the container.

                CloudBlockBlob blob = container.GetBlockBlobReference(App.File.Name);

                //await blob.UploadFromFileAsync(App.File.Path);
                using (var stream = new MemoryStream())
                {
                    App.Bitmap.Compress(Bitmap.CompressFormat.Jpeg, Constant.IMAGE_QUALITY, stream);
                    var bitmapBytes = stream.ToArray();
                    await blob.UploadFromByteArrayAsync(bitmapBytes, 0, bitmapBytes.Length);
                }
            }
            catch (Exception e)
            {
                // TODO:
            }
            
        }
    }
}