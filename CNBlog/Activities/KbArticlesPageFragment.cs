using System.Collections.Generic;
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
	public class RecommendPageFragment : BaseFragment, OnRefreshListener
	{
		CommonAdapter<KbArticle> comAdaper;
		List<KbArticle> articles = new List<KbArticle>();
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.article_fragment_page, container, false);
			InitalComponents();
			plistView = view.FindViewById<PullableListView>(Resource.Id.lv_articles);
			comAdaper = new CommonAdapter<KbArticle>(this.Activity, Resource.Layout.kbarticle_list_item, articles);
			comAdaper.OnGetView += comAdapter_OnGetView;
			plistView.Adapter = comAdaper;
			plistView.ItemClick += plistView_ItemClick;
			ptrl = view.FindViewById<PullToRefreshLayout>(Resource.Id.refresh_view);
			ptrl.setOnRefreshListener(this);
			ptrl.AutoRefresh();
			return view;
		}

		private void plistView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			Intent intent = new Intent(this.Activity, typeof(KbArticleDetailActivity));
			string str = JsonConvert.SerializeObject(articles[e.Position]);
			intent.PutExtra("current", str);
			StartActivity(intent);
		}

		public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, KbArticle item, ViewHolder viewHolder)
		{
			viewHolder.GetView<TextView>(Resource.Id.text_title).Text = item.Title;
			viewHolder.GetView<TextView>(Resource.Id.text_summary).Text = item.Summary;
			viewHolder.GetView<TextView>(Resource.Id.text_good_count).Text = item.DiggCount.ToString();
			viewHolder.GetView<TextView>(Resource.Id.text_author).Text ="作者 "+ item.Author;
			viewHolder.GetView<TextView>(Resource.Id.text_watch_count).Text = item.ViewCount.ToString();
			return viewHolder.GetConvertView();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{ 
				articles = await BlogService.GetKBArticles(CommonHelper.token, 1, pageSize);
				comAdaper.RefreshItems(this.articles);
				pullToRefreshLayout.refreshFinish(0);
			},this.Activity);
		}

		public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () =>
			{
				int pageIndex = this.articles.Count / pageSize + 1;
				var moreArticles = await BlogService.GetKBArticles(CommonHelper.token, pageIndex, pageSize);
				if (moreArticles.Count > 0)
				{
					articles.AddRange(moreArticles);
					comAdaper.RefreshItems(this.articles);
					int index = (articles.Count - 15) - 3;
					plistView.Invalidate();
					plistView.SetSelection(index);
				}
				else
				{
					Msg.AppMsg.MakeText(this.Activity, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				}
				pullToRefreshLayout.refreshFinish(0);
			}, this.Activity);
		}

		public bool CanLoadMore()
		{
			int pageIndex = articles.Count / 15 + 1;
			if (pageIndex <= 1)
			{
				Msg.AppMsg.MakeText(this.Activity, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				return false;
			}
			return true;
		}
	}
}

