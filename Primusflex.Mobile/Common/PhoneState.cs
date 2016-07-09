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
using Android.Telephony;

namespace Primusflex.Mobile.Common
{
    public class PhoneState
    {
        public static TelephonyManager telehonyManager;

        public PhoneState(Activity activity, string telephonyService)
        {
            telehonyManager = (TelephonyManager) activity.ApplicationContext.GetSystemService(telephonyService);
        }

        // Get telephone number
        public static string TelephoneNumber()
        {
            return telehonyManager.Line1Number;
        }

        // Get IMEI Number
        public static string IMenuItem()
        {
            return telehonyManager.DeviceId;
        }
    }
}