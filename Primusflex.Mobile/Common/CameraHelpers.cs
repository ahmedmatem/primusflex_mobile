using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;
using Android.Content.PM;
using Java.IO;
using PrimusFlex.Mobile.Android.Common;

namespace Primusflex.Mobile.Common
{
    public static class CameraHelpers
    {
        public static void CreateDirectoryForPictures()
        {
            App.Dir = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), Constant.AppGalleryDirName);
            if (!App.Dir.Exists())
            {
                App.Dir.Mkdir();
            }
        }
    }
}