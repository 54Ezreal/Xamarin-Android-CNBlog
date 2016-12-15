using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using CNBlogAPI.Model;
using CNBlogAPI.Service;
using Newtonsoft.Json;
using Msg = Sino.Droid.AppMsg;
using CNBlog.Droid.Utils;
using CNBlog.Droid.PullableView;

namespace CNBlog.Droid.Activities
{
    [Activity()]
	public class ArticleCommentsActivity : BaseActivity, OnRefreshListener
	{
		Dialog waitDialog;
		Button btnSend;
		EditText textComments;
		List<ArticleComment> comments = new List<ArticleComment>();
		CommonAdapter<ArticleComment> comAdaper;
		Article currentArticle;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.article_comments);
			currentArticle = JsonConvert.DeserializeObject<Article>(Intent.GetStringExtra("current"));
			InitalComponents();
			SetPageTitle("评论列表");
			bindControls();
			ptrl.AutoRefresh();
		}

		private void bindControls()
		{
			comAdaper = new CommonAdapter<ArticleComment>(this, Resource.Layout.comments_list_item, comments);
			comAdaper.OnGetView += comAdapter_OnGetView;
			pListView.Adapter = comAdaper;
			ptrl.setOnRefreshListener(this);
			btnSend = FindViewById<Button>(Resource.Id.btn_send);
			btnSend.Click += btnSend_Click;
			textComments = FindViewById<EditText>(Resource.Id.text_comment);
		}

		private async void btnSend_Click(object sender, EventArgs e)
		{
			string content = textComments.Text;
			if (string.IsNullOrWhiteSpace(content))
			{
				Msg.AppMsg.MakeText(this, "请输入评论内容", Msg.AppMsg.STYLE_INFO).Show();
				return;
			}
			waitDialog = CommonHelper.CreateLoadingDialog(this, "正在发送评论数据,请稍后...");
			try
			{
				waitDialog.Show();
				if (await BlogService.AddArticleComments(CommonHelper.token, currentArticle.BlogApp, currentArticle.Id, content))
				{
					Msg.AppMsg.MakeText(this, GetString(Resource.String.publish_comments_success), Msg.AppMsg.STYLE_INFO).Show();
					ptrl.AutoRefresh();
				}
				else 
				{
					Msg.AppMsg.MakeText(this, GetString(Resource.String.publish_comments_fail), Msg.AppMsg.STYLE_INFO).Show();
				}
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
		}

		public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, ArticleComment item, ViewHolder viewHolder)
		{
			viewHolder.GetView<TextView>(Resource.Id.text_comment).Text = Html.FromHtml(item.Body).ToString();
			ImageView imgView = viewHolder.GetView<ImageView>(Resource.Id.img_head_2);
			if (imgView != null)
			{
				if (item.FaceUrl.Contains("png") || item.FaceUrl.Contains("jpg"))
					imgLoader.DisplayImage(item.FaceUrl, imgView, displayImageOptions);
				else
					Log.Debug("Not Image", item.FaceUrl);
			}
			viewHolder.GetView<TextView>(Resource.Id.text_floor).Text = item.Floor + "楼 " + item.DateAdded;
			viewHolder.GetView<TextView>(Resource.Id.text_commentators).Text = item.Author;
			return viewHolder.GetConvertView();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{
				comments = (await BlogService.GetArticleComments(CommonHelper.token, currentArticle.BlogApp, currentArticle.Id, 1, 15));
				comAdaper.RefreshItems(comments);
				pullToRefreshLayout.refreshFinish(0);
			},this);
		}

		public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () =>
			{
				int pageIndex = comments.Count / 15 + 1;
				var lastUpdateData = (await BlogService.GetArticleComments(CommonHelper.token, currentArticle.BlogApp, currentArticle.Id, pageIndex, 15));
				if (lastUpdateData.Count > 0)
				{
					comments.AddRange(lastUpdateData);
					comAdaper.RefreshItems(comments);
					int index = (comments.Count - 15) - 3;
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
			int pageIndex = comments.Count / 15 + 1;
			if (pageIndex <= 1)
			{
				Msg.AppMsg.MakeText(this, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				return false;
			}
			return true;
		}
	}
}

