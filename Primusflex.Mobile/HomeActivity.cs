using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

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
using Newtonsoft.Json;
using Primusflex.Mobile.Models;
using Android.Views;
using System.Collections;

namespace Primusflex.Mobile
{
    [Activity(Label = "Welcome", Theme = "@style/CustomActionBarTheme")]
    public class HomeActivity : Activity
    {
        // Access Token
        string accessToken;

        // Kitchen Plot Details
        string userName;
        string siteName;
        string plotNumber;
        string picName;
        string kitchenModelName;

        // Table
        TableLayout table;

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

            LinearLayout history = (LinearLayout)FindViewById(Resource.Id.historyMenu);
            history.Click += LoadHistory;

            SetContentView(Resource.Layout.Home);

            // get access token
            accessToken = Intent.GetStringExtra("access_token");

            userName = Intent.GetStringExtra("user_name");

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

            CheckBox checkBox = FindViewById<CheckBox>(Resource.Id.checkBox1);
            checkBox.CheckedChange += CheckBox_CheckedChange;

            CameraHelpers.CreateDirectoryForPictures();

            Button btnOpenCamera = FindViewById<Button>(Resource.Id.btnOpenCamera);
            btnOpenCamera.Click += TakeAPicture;

            Button btnRefresh = (Button)FindViewById(Resource.Id.btnRefresh);
            btnRefresh.Click += UpdateLastKitchenInfo;

            Button btnHistory = FindViewById<Button>(Resource.Id.btnHistory);
            btnHistory.Click += LoadHistory;

            EditText siteName = FindViewById<EditText>(Resource.Id.site);
            siteName.TextChanged += (sender, e) => TryToEnableCameraButton(sender, e); 

            EditText plotNumber = FindViewById<EditText>(Resource.Id.plot);
            plotNumber.TextChanged += (sender, e) => TryToEnableCameraButton(sender, e);

            // Set spinners for Sites
            IList sitesList = GetSiteList();

            Spinner siteSpinner = FindViewById<Spinner>(Resource.Id.siteSpinner);
            ArrayAdapter siteAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, sitesList);
            siteAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            siteSpinner.Adapter = siteAdapter;
            siteSpinner.Tag = "siteSpinner";

            siteSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinnerItemSelected);

            // Set spinner for kitchen models

            Spinner kitchenSpinner = FindViewById<Spinner>(Resource.Id.kitchenSpinner);
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, Constant.KITCHEN_MODELS);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            kitchenSpinner.Adapter = adapter;
            kitchenSpinner.Tag = "kitchenSpinner";
            
            kitchenModelName = Constant.KITCHEN_MODELS[0];
            kitchenSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(SpinnerItemSelected);


            // Table

            table = this.FindViewById<TableLayout>(Resource.Id.tableLayout1);

            // TODO: Add last day uploads review
            AddLastKitchenInformation(userName);
            
        }

        private IList GetSiteList()
        {
            string uri = Constant.STORAGE_URL + "/GetSiteList";
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.Headers.Add("Authorization", "Bearer " + accessToken);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                var stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                var responseFromServer = reader.ReadToEnd();
                
                return JsonConvert.DeserializeObject<List<string>>(responseFromServer);
            }
        }

        private void CheckBox_CheckedChange(object sender, EventArgs e)
        {
            Spinner siteSpinner = FindViewById<Spinner>(Resource.Id.siteSpinner);
            EditText siteEditText = FindViewById<EditText>(Resource.Id.site);
            TextView checkBoxLabel = FindViewById<TextView>(Resource.Id.checkBoxLabel);

            var checkBox = sender as CheckBox;
            if (checkBox.Checked)
            {
                //Toast.MakeText(this, "checked", ToastLength.Short).Show();
                siteSpinner.Visibility = ViewStates.Gone;
                siteEditText.Visibility = ViewStates.Visible;
                checkBoxLabel.Text = "Unchecked box to select site name from list.";
            }
            else
            {
                //Toast.MakeText(this, "UNchecked", ToastLength.Short).Show();
                siteEditText.Visibility = ViewStates.Gone;
                siteSpinner.Visibility = ViewStates.Visible;
                checkBoxLabel.Text = "Site is not i the list.";
            }
        }

        private void LoadHistory(object sender, EventArgs e)
        {
            Intent history = new Intent(this, typeof(HistoryActivity));
            history.PutExtra("access_token", accessToken);
            history.PutExtra("user_name", userName);
            StartActivity(history);
        }

        private void UpdateLastKitchenInfo(Object sender, EventArgs args)
        {
            table.RemoveAllViews();
            AddLastKitchenInformation(userName);
        }

        private void AddLastKitchenInformation(string userName)
        {
            var lastKitchenInfo = GetLastDayKitchenInfo(userName);
            if (lastKitchenInfo != null)
            {
                TableLayout.LayoutParams layoutParameters = new TableLayout.LayoutParams(
                    TableLayout.LayoutParams.MatchParent, TableLayout.LayoutParams.WrapContent);
                layoutParameters.SetMargins(5, 5, 5, 5);
                layoutParameters.Weight = 1;

                SetTableHeader(layoutParameters);

                foreach (var lki in lastKitchenInfo)
                {
                    TableRow tr = new TableRow(this);
                    tr.LayoutParameters = layoutParameters;

                    TextView site = new TextView(this);
                    site.Text = lki.SiteName;
                    tr.AddView(site);

                    TextView space1 = new TextView(this);
                    space1.Text = "     ";
                    tr.AddView(space1);

                    TextView plotNo = new TextView(this);
                    plotNo.Text = lki.PlotNumber;
                    tr.AddView(plotNo);

                    TextView space2 = new TextView(this);
                    space2.Text = "     ";
                    tr.AddView(space2);

                    TextView kitchenModel = new TextView(this);
                    kitchenModel.Text = lki.KitchenModel.ToString();
                    tr.AddView(kitchenModel);

                    TextView space3 = new TextView(this);
                    space3.Text = "     ";
                    tr.AddView(space3);

                    TextView imgNo = new TextView(this);
                    imgNo.Text = lki.ImageNumber.ToString() + (lki.ImageNumber == 1 ? "pic" : "pics");
                    tr.AddView(imgNo);

                    table.AddView(tr, new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent,
                        TableLayout.LayoutParams.WrapContent));
                }

                // getting last kitchen images
                
                HttpWebRequest request = WebRequest.Create(Constant.STORAGE_URL + "/lastKitchenImages") as HttpWebRequest;
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    var responseFromServer = reader.ReadToEnd();
                    
                    List<ImageViewModel> images = JsonConvert.DeserializeObject<List<ImageViewModel>>(responseFromServer); 
                    
                    CloudStorageAccount account = StorageHelpers.StorageAccount(accessToken);

                    CloudBlobClient blobClient = account.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference(Constant.IMAGE_STORAGE_CONTAINER_NAME);
                    
                    Bitmap[] bitmaps = new Bitmap[images.Count];
                    int position = 0;
                    foreach (var image in images)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            CloudBlockBlob blockBlob = container.GetBlockBlobReference(image.Name);
                            blockBlob.DownloadToStream(ms);
                            byte[] data = ms.ToArray();
                            bitmaps[position] = BitmapHelpers.DecodeSampledBitmapFromByteArray(data, 100, 100);
                        }
                        position++;
                    }

                    var gridView = FindViewById<GridView>(Resource.Id.gridView1);
                    gridView.Adapter = new ImageAdapter(this, bitmaps);
                }
            }
        }

        private void SetTableHeader(TableLayout.LayoutParams layoutParameters)
        {
            TableRow tr = new TableRow(this);
            tr.LayoutParameters = layoutParameters;

            TextView site = new TextView(this);
            site.SetTypeface(null, TypefaceStyle.Bold);
            site.Text = "Site";
            tr.AddView(site);

            TextView space1 = new TextView(this);
            space1.Text = "     ";
            tr.AddView(space1);

            TextView plotNo = new TextView(this);
            plotNo.SetTypeface(null, TypefaceStyle.Bold);
            plotNo.Text = "Plot";
            tr.AddView(plotNo);

            TextView space2 = new TextView(this);
            space2.Text = "     ";
            tr.AddView(space2);

            TextView kitchenModel = new TextView(this);
            kitchenModel.SetTypeface(null, TypefaceStyle.Bold);
            kitchenModel.Text = "Kitchen";
            tr.AddView(kitchenModel);

            TextView space3 = new TextView(this);
            space3.Text = "     ";
            tr.AddView(space3);

            TextView imgNo = new TextView(this);
            imgNo.SetTypeface(null, TypefaceStyle.Bold);
            imgNo.Text = "Pics";
            tr.AddView(imgNo);

            table.AddView(tr, new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent,
                TableLayout.LayoutParams.WrapContent));
        }

        private List<KitchenInfoModel> GetLastDayKitchenInfo(string username)
        {
            string uri = Constant.STORAGE_URL + "/lastkitcheninfo?username=" + username;
            WebRequest request = WebRequest.Create(uri);
            request.Headers.Add("Authorization", "Bearer " + accessToken);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    var responseFromServer = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var resultAsObject = JsonConvert.DeserializeObject<List<KitchenInfoModel>>(responseFromServer);
                    return resultAsObject;
                }
            }

            return null;
        }

        private void SpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner) sender;
            switch ((string)spinner.Tag)
            {
                case "siteSpinner":
                    Toast.MakeText(this, "site", ToastLength.Short).Show();
                    break;
                case "kitchenSpinner":
                    kitchenModelName = spinner.GetItemAtPosition(e.Position).ToString();
                    Toast.MakeText(this, "kitchen", ToastLength.Short).Show();
                    break;
                default:
                    break;
            }
            
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
            picName = String.Format("img_{0}.jpg", DateTime.Now.ToString("yyyyMMddHHmmss"));
            
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
                    SaveImage(Intent.GetStringExtra("user_name") , siteName, plotNumber, picName, kitchenModelName);

                    // upload image from camera to azure blob storage

                    App.Bitmap = App.File.Path.LoadBitmap();
                    await UploadPhoto(sas);

                    // delete picture from gallery
                    // TODO: delete does not work correctly!
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

        private void SaveImage(string userName, string siteName, string plotNumber, string picName, string kitchenModelName)
        {
            System.Uri uri = new System.Uri(Constant.STORAGE_URL + "/saveimage");
            string postData = string.Format("UserName={0}&SiteName={1}&PlotNumber={2}&ImageName={3}&KitchenModel={4}", userName, siteName, plotNumber, picName, kitchenModelName);
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