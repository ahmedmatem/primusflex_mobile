using System;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Content;
using Primusflex.Mobile.Models;
using System.Collections.Generic;
using Android.Graphics;

namespace Primusflex.Mobile.Common
{
    public class ImageAdapter : BaseAdapter
    {
        private readonly Context context;
        private readonly Bitmap[] bitmaps;

        public ImageAdapter(Context c, Bitmap[] bitmaps)
        {
            this.context = c;
            this.bitmaps = bitmaps;
        }

        public override int Count
        {
            get
            {
                return this.bitmaps.Length;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ImageView imageView;

            if (convertView == null)
            {
                // if it's not recycled, initialize some attributes
                imageView = new ImageView(context);
                imageView.LayoutParameters = new AbsListView.LayoutParams(300, 300);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
                imageView.SetPadding(2, 2, 2, 2);
            }
            else
            {
                imageView = (ImageView)convertView;
            }

            imageView.SetImageBitmap(bitmaps[position]);

            return imageView;
        }
    }
}