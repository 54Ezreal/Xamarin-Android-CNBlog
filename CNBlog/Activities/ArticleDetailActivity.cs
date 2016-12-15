using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using CNBlog.Droid.PullableView;
using CNBlog.Droid.Utils;
using CNBlogAPI.Model;
using CNBlogAPI.Service;
using Newtonsoft.Json;
using Msg = Sino.Droid.AppMsg;

namespace CNBlog.Droid.Activities
{
    [Activity(Label = "ArticleDetailActivity")]
	public class ArticleDetailActivity : Activity,OnRefreshListener
	{
		PullableWebView webView;
		PullToRefreshLayout ptrl;
		Article article;
		string content;
		Button btnViewComments;
		Button btnWriteComments;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.article_detail);
			article = JsonConvert.DeserializeObject<Article>(Intent.GetStringExtra("current"));
			FindViewById<TextView>(Resource.Id.head_title).Text = "文章详情";
			Button btnBack = FindViewById<Button>(Resource.Id.title_bar_back);
			btnBack.Click+=delegate { Finish();};
			bindControls();
        }
        
        private async void bindControls()
		{
			btnWriteComments = FindViewById<Button>(Resource.Id.footbar_write_comment);
			btnWriteComments.Click+=delegate
			{
				BottomSheetDialog bottomSheetDiaolog = new BottomSheetDialog(this);
				var inflater = GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
				EditText textComments = FindViewById<EditText>(Resource.Id.text_comments);
				View view = inflater.Inflate(Resource.Layout.write_comments, null);
				TextView textCancel = view.FindViewById<TextView>(Resource.Id.text_cancel);
				textCancel.Click+=delegate { bottomSheetDiaolog.Dismiss();};
				TextView textSend = view.FindViewById<TextView>(Resource.Id.text_send_comments);
				textSend.Click+=async delegate 
				{
					string content = textComments.Text;
					if (string.IsNullOrWhiteSpace(content))
					{
						Msg.AppMsg.MakeText(this, "请输入评论内容", Msg.AppMsg.STYLE_INFO).Show();
						return;
					}
					Dialog  waitDialog = CommonHelper.CreateLoadingDialog(this, "正在发送评论数据,请稍后...");
					try
					{
						waitDialog.Show();
						if (await BlogService.AddArticleComments(CommonHelper.token, article.BlogApp, article.Id, content))
						{
							Msg.AppMsg.MakeText(this, GetString(Resource.String.publish_comments_success), Msg.AppMsg.STYLE_INFO).Show();
							bottomSheetDiaolog.Dismiss();
						}
						else
							Msg.AppMsg.MakeText(this, GetString(Resource.String.publish_comments_fail), Msg.AppMsg.STYLE_INFO).Show();
					}
					catch (Exception ex)
					{
						Msg.AppMsg.MakeText(this, GetString(Resource.String.publish_comments_fail), Msg.AppMsg.STYLE_INFO).Show();
						Log.Debug("error:", ex.Message);
					}
					finally
					{
						waitDialog.Cancel();
					}
				};
				bottomSheetDiaolog.SetContentView(view);
				bottomSheetDiaolog.Show();
			};
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
            //webView.Settings.UseWideViewPort = true;//设置此属性，可任意比例缩放
            btnViewComments = FindViewById<Button>(Resource.Id.footbar_comments);	
			btnViewComments.Click+=delegate 
			{
				Intent intent = new Intent(this, typeof(ArticleCommentsActivity));
				intent.PutExtra("current", JsonConvert.SerializeObject(article));
				StartActivity(intent); 
			};
            CommonHelper.InitalShare(this, null, true, article.Author, article.Title, article.Avatar, article.Url);
            CommonHelper.InitalBookMark(this, article.Url, article.Title);
			await ptrl.AutoRefresh();
		}


		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{
				content = await BlogService.GetArticleContent(CommonHelper.token, article.Id);
                content = content.Replace("\\r\\n", "</br>").Replace("\\n","</br>").Replace("\\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("\\", string.Empty);
				content = content.Substring(1, content.Length - 2);
				using (var stream = Assets.Open("articlecontent.html"))
				{
					StreamReader sr = new StreamReader(stream);
					string html = sr.ReadToEnd();
					sr.Close();
					sr.Dispose();
					html = html.Replace("#title#", article.Title)
							   .Replace("#author#", article.Author)
						   .Replace("#time#", article.PostDate.ToShortDateString())
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

		//private void showPopwindow()
		//{
		//	var inflater =GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
		//	View view = inflater.Inflate(Resource.Layout.testpopwindow,null);
		//	PopupWindow window = new PopupWindow(view, 550, 550);
		//	window.Focusable = true;
		//	ColorDrawable dw = new ColorDrawable(Android.Graphics.Color.Blue);
		//	window.SetBackgroundDrawable(dw);
		//	window.AnimationStyle = Resource.Style.mypopwindow_anim_style;
		//	window.ShowAtLocation(btnWriteComments, GravityFlags.Bottom, 0, 0);
		//}

	}


	public class MyWebViewClient : WebViewClient
	{ 
		public override void OnPageFinished(WebView view, string url)
		{
			if (!view.Settings.LoadsImagesAutomatically)
			{
				view.Settings.LoadsImagesAutomatically = true;
			}
		}

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            return false;
        }

    }

}
