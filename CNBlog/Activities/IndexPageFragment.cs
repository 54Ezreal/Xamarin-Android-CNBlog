using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using CNBlogAPI.Model;
using CNBlogAPI.Service;
using CNBlog.Droid.Utils;
using CNBlog.Droid.PullableView;
using Msg = Sino.Droid.AppMsg;
namespace CNBlog.Droid.Activities
{
    public class IndexPageFragment : BaseFragment, OnRefreshListener
    {
		CommonAdapter<Article> comAdaper;
		List<Article> articles = new List<Article>();
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
			View view = inflater.Inflate(Resource.Layout.article_fragment_page, container, false);
				InitalComponents();
				plistView = view.FindViewById<PullableListView>(Resource.Id.lv_articles);
				comAdaper = new CommonAdapter<Article>(this.Activity, Resource.Layout.article_list_item, articles);
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
			Intent intent = new Intent(this.Activity, typeof(ArticleDetailActivity));
			string str = JsonConvert.SerializeObject(articles[e.Position]);
			intent.PutExtra("current", str);
			StartActivity(intent);
		}

		public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, Article item, ViewHolder viewHolder)
		{
			viewHolder.GetView<TextView>(Resource.Id.text_title).Text = item.Title;
			viewHolder.GetView<TextView>(Resource.Id.text_summary).Text = item.Description;
			viewHolder.GetView<TextView>(Resource.Id.text_good_count).Text = item.DiggCount.ToString();
			viewHolder.GetView<TextView>(Resource.Id.text_author).Text = item.Author;
			viewHolder.GetView<TextView>(Resource.Id.text_watch_count).Text = item.ViewCount.ToString();
			viewHolder.GetView<TextView>(Resource.Id.text_talk_count).Text = item.CommentCount.ToString();
			viewHolder.GetView<TextView>(Resource.Id.text_publish_ago).Text = CommonHelper.DateDiff(DateTime.Now, item.PostDate) + "前";
			ImageView imgView = viewHolder.GetView<ImageView>(Resource.Id.img_avatar);
			if (imgView != null)
			{
				if (item.Avatar.Contains("png") || item.Avatar.Contains("jpg") || item.Avatar.Contains("gif"))
				{
					imgLoader.DisplayImage(item.Avatar, imgView, displayImageOptions);
				}
				else
				{
					Log.Debug("Not Image", item.Avatar);
					imgView.SetImageResource(Resource.Drawable.girl);
				}
			}
			return viewHolder.GetConvertView();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
        {
            BaseService.ExeRequest(async () => 
            {
                articles = await BlogService.GetHomeArticles(CommonHelper.token, 1, pageSize);
                comAdaper.RefreshItems(articles);
                pullToRefreshLayout.refreshFinish(0);
            }, this.Activity);
        }

		public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
        {
			BaseService.ExeRequest(async () =>
			{
				int pageIndex = articles.Count / pageSize + 1;
				var moreArticles = await BlogService.GetHomeArticles(CommonHelper.token, pageIndex, pageSize);
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
				pullToRefreshLayout.loadmoreFinish(0);
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