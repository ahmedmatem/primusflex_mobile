namespace PrimusFlex.Mobile.Common
{
    public static class Constant
    {
        // web services
        public const string TokenRequestUrl = "http://primusflexwebservices.azurewebsites.net/token";

        public const string STORAGE_URL = "http://primusflexwebservices.azurewebsites.net/api/storage";
        public const string LOCAL_STORAGE_URL = "http://localhost:52173/api/storage";

        public const string ACCOUNT_URL = "http://primusflexwebservices.azurewebsites.net/api/account";

        public static string REMOTE_IMEI_LOGIN_URI = "http://primusflexwebservices.azurewebsites.net/api/login/byimei";
        public static string LOCAL_IMEI_LOGIN_URI = "http://localhost:52173/api/login/byimei";

        public static string LOGIN_SERVICE_URI = "http://primusflexwebservices.azurewebsites.net/api/login/";
        public static string LOCAL_LOGIN_SERVICE_URI = "http://localhost:52173/api/login/";

        public const string AppGalleryDirName = "PrimusFlex";
        public static string APPLICATION_NAME = "PrimusFlex.Mobile";

        public const string IMAGE_STORAGE_CONTAINER_NAME = "image-container";

        public const int IMAGE_QUALITY = 40;

        public const float BUTTON_ALPHA = 0.7f;

        public static string[] KITCHEN_MODELS = new string[] { "Symphony", "HATT", "Comador", "Manhattan" };



    }
}