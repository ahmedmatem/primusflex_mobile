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

namespace PrimusFlex.Mobile.Android.Common
{
    public static class Constant
    {
        public const string TokenRequestUrl = "http://primusflexwebservices.azurewebsites.net/token";

        public const string AppGalleryDirName = "PrimusFlex";

        public const string IMAGE_STORAGE_CONTAINER_NAME = "image-container";
    }
}