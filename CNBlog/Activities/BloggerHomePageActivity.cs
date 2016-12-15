using System;
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
	[Activity(Label = "BloggerHomePageActivity")]
	public class BloggerHomePageActivity : BaseActivity,OnRefreshListener
	{
		CommonAdapter<Article> comAdaper;
		List<Article> articles = new List<Article>();
		Blogger blogger;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.blogger_homepage);
			blogger = JsonConvert.DeserializeObject<Blogger>(Intent.GetStringExtra("blogger"));
			InitalComponents();
			SetPageTitle(blogger.Title);
			bindControls();
		}

		private void bindControls()
		{
			RoundImageView rImgView = FindViewById<RoundImageView>(Resource.Id.img_avator);
			if (blogger.Avatar.Contains("png") || blogger.Avatar.Contains("jpg") || blogger.Avatar.Contains("gif"))
			{
				imgLoader.DisplayImage(blogger.Avatar, rImgView, displayImageOptions);
			}
			TextView textHomePage = FindViewById<TextView>(Resource.Id.text_homepage);
			textHomePage.Text = blogger.Link.ToString();
			TextView textBloggerApp = FindViewById<TextView>(Resource.Id.text_blogapp);
			textBloggerApp.Text = blogger.BlogApp;
			TextView textPostCount = FindViewById<TextView>(Resource.Id.text_blog_count);
			textPostCount.Text = "博文数:" + blogger.PostCount.ToString();
			TextView textLastPost = FindViewById<TextView>(Resource.Id.text_lastupdate);
			textLastPost.Text = "最后更新:" + CommonHelper.DateDiff(DateTime.Now, blogger.Updated) + "前";
			comAdaper = new CommonAdapter<Article>(this, Resource.Layout.blogger_article_list_item, articles);
			comAdaper.OnGetView += comAdapter_OnGetView;
			pListView.Adapter = comAdaper;
			pListView.ItemClick += pListView_ItemClick;
			ptrl = FindViewById<PullToRefreshLayout>(Resource.Id.refresh_view);
			ptrl.setOnRefreshListener(this);
			ptrl.AutoRefresh();
		}

		private void pListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			Intent intent = new Intent(this, typeof(ArticleDetailActivity));
			string str = JsonConvert.SerializeObject(articles[e.Position]);
			intent.PutExtra("current", str);
			StartActivity(intent);
		}

		public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, Article item, ViewHolder viewHolder)
		{
			viewHolder.GetView<TextView>(Resource.Id.text_title).Text = item.Title;
			viewHolder.GetView<TextView>(Resource.Id.text_publish_ago).Text = "发布于 " + item.PostDate;
			return viewHolder.GetConvertView();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{
				articles = await BlogService.GetBolggerPosts(CommonHelper.token, blogger.BlogApp, 1);
				comAdaper.RefreshItems(articles);
				pullToRefreshLayout.refreshFinish(0);
			},this);
		}

		public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () =>
			{
				int pageIndex = articles.Count / 25 + 1;
				var moreArticles = await BlogService.GetBolggerPosts(CommonHelper.token, blogger.BlogApp, pageIndex);
				if (moreArticles.Count > 0)
				{
					articles.AddRange(moreArticles);
					comAdaper.RefreshItems(this.articles);
					int index = (articles.Count - 15) - 3;
					pListView.Invalidate();
					pListView.SetSelection(index);
				}
				{ 
					Msg.AppMsg.MakeText(this, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				}
				pullToRefreshLayout.loadmoreFinish(0);
			}, this);
		}

		public bool CanLoadMore()
		{
			int pageIndex = articles.Count / 15 + 1;
			if (pageIndex <= 1)
			{
				Msg.AppMsg.MakeText(this, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				return false;
			}
			return true;
		}

	}
}

