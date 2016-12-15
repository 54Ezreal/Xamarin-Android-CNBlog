using Android.App;
using Android.Widget;
using CNBlog.Droid.PullableView;
using CNBlog.Droid.Utils;
using Com.Nostra13.Universalimageloader.Core;

namespace CNBlog.Droid.Activities
{
    [Activity(Label = "BaseActivity")]
	public class BaseActivity : Activity
	{
		protected ImageLoader imgLoader;
		protected DisplayImageOptions displayImageOptions;
		protected Button btnBack;
		protected TextView textPageTitle;
		protected PullableListView pListView;
		protected PullToRefreshLayout ptrl;
		protected void InitalComponents()
		{
			BlogApplication.InitImageLoader(this);
			imgLoader = ImageLoader.Instance;
			displayImageOptions = new DisplayImageOptions.Builder()
														.ShowImageForEmptyUri(Resource.Drawable.girl)
														 .ShowImageOnFail(Resource.Drawable.girl)
														 .ShowImageOnLoading(Resource.Drawable.girl)
														 .CacheInMemory(true)
														 .CacheOnDisk(true)
														 .ResetViewBeforeLoading()
														 .Build();
			btnBack = FindViewById<Button>(Resource.Id.title_bar_back);
			btnBack.Click += delegate { Finish();};
			textPageTitle = FindViewById<TextView>(Resource.Id.head_title);
			ptrl = FindViewById<PullToRefreshLayout>(Resource.Id.refresh_view);
			pListView = FindViewById<PullableListView>(Resource.Id.pListView);
		}

		protected void SetPageTitle(string title)
		{
			textPageTitle.Text = title;
		}


	}
}
