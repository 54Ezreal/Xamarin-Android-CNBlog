using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CNBlog.Droid.PullableView;
using CNBlog.Droid.Utils;
using CNBlogAPI.Model;
using CNBlogAPI.Service;
using Newtonsoft.Json;
using Msg = Sino.Droid.AppMsg;
namespace CNBlog.Droid.Activities
{
	[Activity(Label = "BookMarksActivity")]
	public class BookMarksActivity : BaseActivity,OnRefreshListener
	{
		CommonAdapter<BookMark> comAdaper;
		List<BookMark> bookMarks = new List<BookMark>();
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.my_posts);
			InitalComponents();
			SetPageTitle("我的收藏");
			comAdaper = new CommonAdapter<BookMark>(this, Resource.Layout.blogger_article_list_item, bookMarks);
			comAdaper.OnGetView += comAdapter_OnGetView;
			pListView.Adapter = comAdaper;
			pListView.ItemClick +=pListView_ItemClick;
			ptrl = FindViewById<PullToRefreshLayout>(Resource.Id.refresh_view);
			ptrl.setOnRefreshListener(this);
			ptrl.AutoRefresh();
		}

		private void pListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			var bookMark = bookMarks[e.Position];
			Intent intent = new Intent(this, typeof(BookMarkDetailActivity));
			intent.PutExtra("bookMark", JsonConvert.SerializeObject(bookMark));
			StartActivity(intent);
		}

		public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, BookMark item, ViewHolder viewHolder)
		{
			viewHolder.GetView<TextView>(Resource.Id.text_title).Text = item.Title;
			viewHolder.GetView<TextView>(Resource.Id.text_publish_ago).Text = "收藏于 " + item.DateAdded;
			return viewHolder.GetConvertView();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{
				bookMarks = await BookMarkService.GetBookMarks(CommonHelper.token, 1, 15);
				comAdaper.RefreshItems(bookMarks);
				pullToRefreshLayout.refreshFinish(0);
			},this);
		}

		public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () =>
			{
				int pageIndex = bookMarks.Count / 25 + 1;
				var moreBookMarks = await BookMarkService.GetBookMarks(CommonHelper.token, pageIndex, 15);
				if (moreBookMarks.Count > 0)
				{
					bookMarks.AddRange(moreBookMarks);
					comAdaper.RefreshItems(bookMarks);
					int index = (bookMarks.Count - 15) - 3;
					pListView.Invalidate();
					pListView.SetSelection(index);
				}
				else
				{
					Msg.AppMsg.MakeText(this, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				}
				pullToRefreshLayout.loadmoreFinish(0);
			}, this);
		}

		public bool CanLoadMore()
		{
			int pageIndex = bookMarks.Count / 15 + 1;
			if (pageIndex <= 1)
			{
				Msg.AppMsg.MakeText(this, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				return false;
			}
			return true;
		}

	}
}
