using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;

namespace CNBlog.Droid.PullableView
{
    public class PullableRecyclerView : RecyclerView,Pullable
    {
        public PullableRecyclerView(Context context)
			:base(context)
		{
        }

        public PullableRecyclerView(Context context, IAttributeSet attrs)
			:base(context,attrs)
		{

        }

        public PullableRecyclerView(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs,defStyle)
		{

        }

        public Func<bool> CanPullDownFunc;

        public Func<bool> CanPullUpFunc;

        public bool canPullDown()
        {
            return CanPullDownFunc();
        }

        public bool canPullUp()
        {
            return CanPullUpFunc();
        }

    }
}