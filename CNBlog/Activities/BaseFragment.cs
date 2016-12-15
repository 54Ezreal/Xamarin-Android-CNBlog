using CNBlog.Droid.PullableView;
using CNBlog.Droid.Utils;
using Com.Nostra13.Universalimageloader.Core;

namespace CNBlog.Droid.Activities
{
	public class BaseFragment :Android.Support.V4.App.Fragment
	{
		protected int pageSize = 15;
		protected ImageLoader imgLoader;
		protected DisplayImageOptions displayImageOptions;
		protected PullableListView plistView;
		protected PullToRefreshLayout ptrl;
		protected void InitalComponents()
		{
			BlogApplication.InitImageLoader(this.Context);
			imgLoader = ImageLoader.Instance;
			displayImageOptions = new DisplayImageOptions.Builder()
														.ShowImageForEmptyUri(Resource.Drawable.girl)
														 .ShowImageOnFail(Resource.Drawable.girl)
														 .ShowImageOnLoading(Resource.Drawable.girl)
														 .CacheInMemory(true)
														 .CacheOnDisk(true)
														 .ResetViewBeforeLoading()
														 .Build();
		}

	}
}
