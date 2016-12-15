using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using CNBlog.Droid.PullableView;
using CNBlog.Droid.Utils;
using CNBlogAPI.Model;
using CNBlogAPI.Service;
using Newtonsoft.Json;

namespace CNBlog.Droid.Activities
{
	[Activity(Label = "KbArticleDetailActivity")]
	public class KbArticleDetailActivity : Activity, OnRefreshListener
	{
		PullableWebView webView;
		PullToRefreshLayout ptrl;
		KbArticle article;
		string content;
		Button btnBack;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.article_detail);
			article = JsonConvert.DeserializeObject<KbArticle>(Intent.GetStringExtra("current"));
			bindControls();
		}

		private void bindControls()
		{
			FindViewById<TextView>(Resource.Id.head_title).Text = "文章详情";
			webView = FindViewById<PullableWebView>(Resource.Id.webview);
			ptrl = FindViewById<PullToRefreshLayout>(Resource.Id.refresh_view);
			ptrl.setOnRefreshListener(this);
			webView.Settings.DefaultTextEncodingName = "utf-8";
			webView.Settings.LoadsImagesAutomatically = true;
			webView.SetWebViewClient(new MyWebViewClient());
			webView.ScrollBarStyle = ScrollbarStyles.InsideOverlay;
			webView.Settings.JavaScriptEnabled = false;
			webView.Settings.SetSupportZoom(false);
			webView.Settings.BuiltInZoomControls = false;
			webView.Settings.CacheMode = CacheModes.CacheElseNetwork;
			webView.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.SingleColumn);
			btnBack = FindViewById<Button>(Resource.Id.title_bar_back);
			btnBack.Click += delegate { Finish(); };
			Button btnViewComments = FindViewById<Button>(Resource.Id.footbar_comments);
			btnViewComments.Visibility = ViewStates.Gone;
            Button btnWriteComments = FindViewById<Button>(Resource.Id.footbar_write_comment);
            btnWriteComments.Visibility = ViewStates.Gone;
            CommonHelper.InitalBookMark(this, string.Format(@"http://kb.cnblogs.com/page/{0}/", article.Id), article.Title);
            CommonHelper.InitalShare(this, null, true, article.Author, article.Title, "", string.Format(@"http://kb.cnblogs.com/page/{0}/",article.Id));
			ptrl.AutoRefresh();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{
				content = await BlogService.GetKbArticleContent(CommonHelper.token, article.Id);
                content = content.Replace("\\r\\n", "</br>").Replace("\\n", "</br>").Replace("\\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("\\", string.Empty);
                content = content.Substring(1, content.Length - 2);
				using (var stream = Assets.Open("articlecontent.html"))
				{
					StreamReader sr = new StreamReader(stream);
					string html = sr.ReadToEnd();
					sr.Close();
					sr.Dispose();
					html = html.Replace("#title#", article.Title)
							   .Replace("#author#", article.Author)
							   .Replace("#time#", article.DateAdded.ToShortDateString())
							   .Replace("#content#", content);
					webView.LoadDataWithBaseURL("file:///android_asset/", html, "text/html", "utf-8", null);
				}
				pullToRefreshLayout.refreshFinish(0);
			},this);
		}

		public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
		{

		}

		public bool CanLoadMore()
		{
			return false;
		}

	}
}
