using System;
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
	[Activity(Label = "BookMarkDetailActivity")]
	public class BookMarkDetailActivity : Activity, OnRefreshListener
	{
		PullableWebView webView;
		PullToRefreshLayout ptrl;
		BookMark bookMark;
		string content;
		Button btnBack;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.article_detail);
            FindViewById<TextView>(Resource.Id.head_title).Text = "收藏详情";
            bookMark = JsonConvert.DeserializeObject<BookMark>(Intent.GetStringExtra("bookMark"));
			bindControls();
		}

		private async void bindControls()
		{
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
			await ptrl.AutoRefresh();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
            int id = -1, markType = 1;
            try
            {
                //通过收藏URL判断书签类型
                if (bookMark.LinkUrl.Contains("https://news"))
                {
                    //新闻
                    int lastIndex = bookMark.LinkUrl.LastIndexOf("/");
                    if (lastIndex <= 27)
                        id = Convert.ToInt32(bookMark.LinkUrl.Substring(27, bookMark.LinkUrl.Length - 27));
                    else
                        id = Convert.ToInt32(bookMark.LinkUrl.Substring(27, bookMark.LinkUrl.Length - 28));
                    markType = 1;
                    CommonHelper.InitalShare(this, null, true, "博客园新闻分享", bookMark.Title, "", bookMark.LinkUrl);
                }
                else if (bookMark.LinkUrl.Contains("http://kb."))
                {
                    //知识库
                    int lastIndex = bookMark.LinkUrl.LastIndexOf("/");
                    if (lastIndex <= 27)
                        id = Convert.ToInt32(bookMark.LinkUrl.Substring(27, bookMark.LinkUrl.Length - 27));
                    else
                        id = Convert.ToInt32(bookMark.LinkUrl.Substring(27, bookMark.LinkUrl.Length - 28));
                    markType = 2;
                    Button btnViewComments = FindViewById<Button>(Resource.Id.footbar_comments);
                    btnViewComments.Visibility = ViewStates.Gone;
                    LinearLayout linearWriteComments = FindViewById<LinearLayout>(Resource.Id.footbar_write_comment);
                    linearWriteComments.Visibility = ViewStates.Gone;
                    CommonHelper.InitalShare(this, null, true, "知识库分享", bookMark.Title, "", bookMark.LinkUrl);
                }
                else
                {
                    //默认博客类型
                    markType = 3;
                    int startindex = bookMark.LinkUrl.LastIndexOf("/");
                    //bookMark.WzLinkId
                    id = Convert.ToInt32(bookMark.LinkUrl.Substring(startindex + 1, bookMark.LinkUrl.Length - startindex - 6));
                    CommonHelper.InitalShare(this, null, true, "博客园文章分享", bookMark.Title, "", bookMark.LinkUrl);
                }
            }
            catch (Exception)
            {
                
            }
            if (id == -1)
            {
                using (var stream = Assets.Open("articlecontent.html"))
                {
                    StreamReader sr = new StreamReader(stream);
                    string html = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();
                    html = html.Replace("#title#", bookMark.Title)
                               .Replace("#author#", "")
                               .Replace("#time#", "收藏于 " + bookMark.DateAdded.ToString("yyyy/MM/dd hh:mm:ss"))
                               .Replace("#content#", "尴尬，这篇文章有点儿小问题，等会儿再看吧");
                    webView.LoadDataWithBaseURL("file:///android_asset/", html, "text/html", "utf-8", null);
                }
                pullToRefreshLayout.refreshFinish(0);
            }
            else
            {
                CommonHelper.InitalBookMark(this, bookMark.LinkUrl, bookMark.Title);
                BaseService.ExeRequest(async () =>
                {
                    switch (markType)
                    {
                        case 1:
                            content = await NewsService.GetNewInfo(CommonHelper.token, id);
                            break;
                        case 2:
                            content = await BlogService.GetKbArticleContent(CommonHelper.token, id);
                            break;
                        case 3:
                            content = await BlogService.GetArticleContent(CommonHelper.token, id);
                            break;
                    }
                    content = content.Replace("\\r\\n", "</br>").Replace("\\n", "</br>").Replace("\\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("\\", string.Empty);
                    content = content.Substring(1, content.Length - 2);
                    using (var stream = Assets.Open("articlecontent.html"))
                    {
                        StreamReader sr = new StreamReader(stream);
                        string html = sr.ReadToEnd();
                        sr.Close();
                        sr.Dispose();
                        html = html.Replace("#title#", bookMark.Title)
                                   .Replace("#author#", "")
                                   .Replace("#time#", "收藏于 " + bookMark.DateAdded.ToString("yyyy/MM/dd hh:mm:ss"))
                                   .Replace("#content#", content);
                        webView.LoadDataWithBaseURL("file:///android_asset/", html, "text/html", "utf-8", null);
                    }
                    pullToRefreshLayout.refreshFinish(0);
                }, this);
            }
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
