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
using Android.Support.Design.Widget;
using Msg = Sino.Droid.AppMsg;
using System;
using Android.Util;

namespace CNBlog.Droid.Activities
{
    [Activity(Label = "NewDetailActivity")]
	public class NewDetailActivity : Activity, OnRefreshListener
	{
		PullableWebView webView;
		PullToRefreshLayout ptrl;
		NewInfo newsInfo;
		string content;
		Button btnBack;
		Button btnViewComments;
        Button btnWriteComments;
        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.article_detail);
			newsInfo = JsonConvert.DeserializeObject<NewInfo>(Intent.GetStringExtra("newInfo"));
			bindControls();
		}

		private async void bindControls()
		{
            FindViewById<TextView>(Resource.Id.head_title).Text = "新闻详情";
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
			btnViewComments = FindViewById<Button>(Resource.Id.footbar_comments);
			btnViewComments.Click += delegate
			  {
				  Intent intent = new Intent(this, typeof(NewsCommentsActitvity));
				  intent.PutExtra("newsInfo", JsonConvert.SerializeObject(newsInfo));
				  StartActivity(intent);
			  };
            CommonHelper.InitalShare(this, null, true,"博客园新闻分享", newsInfo.Title, newsInfo.TopicIcon, string.Format("https://news.cnblogs.com/n/{0}/",newsInfo.Id));
            CommonHelper.InitalBookMark(this, string.Format("https://news.cnblogs.com/n/{0}/", newsInfo.Id), newsInfo.Title);
            btnWriteComments = FindViewById<Button>(Resource.Id.footbar_write_comment);
            btnWriteComments.Click += delegate
            {
                BottomSheetDialog bottomSheetDiaolog = new BottomSheetDialog(this);
                var inflater = GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                EditText textComments = FindViewById<EditText>(Resource.Id.text_comments);
                View view = inflater.Inflate(Resource.Layout.write_comments, null);
                TextView textCancel = view.FindViewById<TextView>(Resource.Id.text_cancel);
                textCancel.Click += delegate { bottomSheetDiaolog.Dismiss(); };
                TextView textSend = view.FindViewById<TextView>(Resource.Id.text_send_comments);
                textSend.Click += async delegate
                {
                    string content = textComments.Text;
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Msg.AppMsg.MakeText(this, "请输入评论内容", Msg.AppMsg.STYLE_INFO).Show();
                        return;
                    }
                    Dialog waitDialog = CommonHelper.CreateLoadingDialog(this, "正在发送评论数据,请稍后...");
                    try
                    {
                        waitDialog.Show();
                        if (await NewsService.AddNewComments(CommonHelper.token, newsInfo.Id, content))
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
            await ptrl.AutoRefresh();
        }

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{
				content = await NewsService.GetNewInfo(CommonHelper.token, newsInfo.Id);
                content = content.Replace("\\r\\n", "</br>").Replace("\\n", "</br>").Replace("\\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace("\\", string.Empty);
                content = content.Substring(1, content.Length - 2);
				using (var stream = Assets.Open("articlecontent.html"))
				{
					StreamReader sr = new StreamReader(stream);
					string html = sr.ReadToEnd();
					sr.Close();
					sr.Dispose();
					html = html.Replace("#title#", newsInfo.Title)
							   .Replace("#author#", "")
							   .Replace("#time#", newsInfo.DateAdded.ToShortDateString())
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
