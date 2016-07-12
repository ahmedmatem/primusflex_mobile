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

namespace PrimusFlex.Mobile.Common
{
    public class PhoneState
    {
        public TelephonyManager telehonyManager;

        public PhoneState(TelephonyManager telehonyManager)
        {
            this.telehonyManager = telehonyManager;
        }

        // Get telephone number
        public string TelephoneNumber()
        {
            return telehonyManager.Line1Number;
        }

        // Get IMEI Number
        public string IMEI()
        {
            return telehonyManager.DeviceId;
        }
    }
}