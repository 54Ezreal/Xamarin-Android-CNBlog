using System;
using Android.Content;
using Android.Util;
using Android.Webkit;

namespace CNBlog.Droid.PullableView
{
	public class PullableWebView :WebView,Pullable
	{
		public PullableWebView(Context context)
			:base(context)
		{
			
		}

		public PullableWebView(Context context, IAttributeSet attrs)
			:base(context,attrs)
		{
		}

		public PullableWebView(Context context, IAttributeSet attrs, int defStyle)
			:base(context,attrs,defStyle)
		{
			
		}

		public bool canPullDown()
		{
			if (ScrollY == 0)
				return true;
			else
				return false;
		}

		public bool canPullUp()
		{
			//if (ScrollY >=ContentHeight * Scale
			//    - MeasuredHeight)
			//	return true;
			//else
			return false;
		}
	}
}
