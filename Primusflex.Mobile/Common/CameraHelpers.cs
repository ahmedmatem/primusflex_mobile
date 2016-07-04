using Java.IO;

namespace PrimusFlex.Mobile.Common
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