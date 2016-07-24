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
using Android.Telephony;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using PrimusFlex.Mobile.Common;
using Primusflex.Mobile.Common;

namespace Primusflex.Mobile
{
    [Activity(Label = "Welcome", Theme = "@style/CustomActionBarTheme")]
    public class HomeActivity : Activity
    {
        // Access Token
        string accessToken;

        // Kitchen Plot Details
        string siteName;
        string plotNumber;
        string picName;
        string kitchenModelName;

        // Cloud Storage
        static CloudStorageAccount storageAccount;
        
        static CloudBlobClient blobClient;

        static CloudBlobContainer container;
        
        string sas;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // disable default ActionBar view and set custom view
            ActionBar.SetCustomView(Resource.Layout.action_bar);
            ActionBar.SetDisplayShowCustomEnabled(true);

            SetContentView(Resource.Layout.Home);

            // get access token
            accessToken = Intent.GetStringExtra("access_token");

            //Parse the connection string and return a reference to the storage account.
            storageAccount = StorageHelpers.StorageAccount(accessToken);

            //Create the blob client object.
            blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            container = blobClient.GetContainerReference(Constant.IMAGE_STORAGE_CONTAINER_NAME);

            // Use the shared access signature (SAS) to perform container operations
            sas = StorageHelpers.GetContainerSasUri(container);

            var updateMenu = FindViewById<LinearLayout>(Resource.Id.updateMenu);
            updateMenu.Selected = true;

            CameraHelpers.CreateDirectoryForPictures();

            Button btnOpenCamera = FindViewById<Button>(Resource.Id.btnOpenCamera);
            btnOpenCamera.Click += TakeAPicture;

            EditText siteName = FindViewById<EditText>(Resource.Id.site);
            siteName.TextChanged += (sender, e) => TryToEnableCameraButton(sender, e); 

            EditText plotNumber = FindViewById<EditText>(Resource.Id.plot);
            plotNumber.TextChanged += (sender, e) => TryToEnableCameraButton(sender, e);

            // Set spinner for kitchen models

            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1);
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, Constant.KITCHEN_MODELS);
            spinner.Adapter = adapter;

            // spinner default value
            kitchenModelName = Constant.KITCHEN_MODELS[0];

            // add spinner ItemSelected Event Handler
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinnerItemSelected);
            
        }

        private void SpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spn = (Spinner) sender;
            kitchenModelName = spn.GetItemAtPosition(e.Position).ToString();
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
            // Collect plot details
            siteName = FindViewById<EditText>(Resource.Id.site).Text;
            plotNumber = FindViewById<EditText>(Resource.Id.plot).Text;
            picName = String.Format("img_sn{0}_pl{1}_m{2}_d{3}.jpg", siteName, plotNumber, kitchenModelName, DateTime.Now.ToString("yyyyMMddHHmmss"));
            
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App.File = new Java.IO.File(App.Dir,picName);
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
                    // save imige information in azure sql database
                    SaveImage(siteName, plotNumber, picName, kitchenModelName);

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

        private void SaveImage(string siteName, string plotNumber, string picName, string kitchenModelName)
        {
            System.Uri uri = new System.Uri(Constant.STORAGE_URL + "/saveimage");
            string postData = string.Format("SiteName={0}&PlotNumber={1}&ImageName={2}&KitchenModel={3}", siteName, plotNumber, picName, kitchenModelName);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    wc.Headers.Add("Authorization", "Bearer " + accessToken);
                    wc.UploadString(uri, "POST", postData);
                }
            }
            catch(WebException e)
            {
                var errorMessage = (new StreamReader(e.Response.GetResponseStream())).ReadToEnd();
            }
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