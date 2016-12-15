using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CNBlog.Droid.PullableView
{
	public class PullableListView :ListView,Pullable
	{
		private View errorLayout;

		public PullableListView(Context context)
			:base(context)
		{
		}

		public PullableListView(Context context, IAttributeSet attrs)
			:base(context,attrs)
		{
			
		}

		public PullableListView(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs,defStyle)
		{
			
		}

		public bool canPullDown()
		{
			if (Count == 0)
				{
					// 没有item的时候也可以下拉刷新
					return true;
				}
			else if (FirstVisiblePosition == 0
			         && GetChildAt(0).Top >= 0)
				{
					// 滑到ListView的顶部了
					return true;
				}
				else
					return false;
		}

		public bool canPullUp()
		{

			if (Count == 0)
			{
				// 没有item的时候也可以上拉加载
				return true;
			}
			else if (LastVisiblePosition == (Count - 1))
			{
				// 滑到底部了
				if (GetChildAt(LastVisiblePosition - FirstVisiblePosition) != null
						&& GetChildAt(
					    LastVisiblePosition
					    - FirstVisiblePosition).Bottom <= MeasuredHeight)
					return true;
			}
			return false;

		}
		public void SetErrorLayout()
		{
			if (errorLayout == null)
				errorLayout = LayoutInflater.From(Context).Inflate(Resource.Layout.error_page, null);
		    RemoveHeaderView(errorLayout);
			AddHeaderView(errorLayout,null,false);
			SetHeaderDividersEnabled(false);
		}
		
	}
}
