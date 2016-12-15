using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Util;
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
    [Activity(Label = "NewsCommentsActitvity")]
	public class NewsCommentsActitvity : BaseActivity, OnRefreshListener
	{
		Dialog waitDialog;
		Button btnSend;
		EditText textComments;
		List<NewsComment> comments = new List<NewsComment>();
		CommonAdapter<NewsComment> comAdaper;
		NewInfo newsInfo;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.article_comments);
			newsInfo = JsonConvert.DeserializeObject<NewInfo>(Intent.GetStringExtra("newsInfo"));
			InitalComponents();
			SetPageTitle("评论列表");
			bindControls();
		}

		private async void bindControls()
		{
			comAdaper = new CommonAdapter<NewsComment>(this, Resource.Layout.comments_list_item, comments);
			comAdaper.OnGetView += comAdapter_OnGetView;
			pListView.Adapter = comAdaper;
			ptrl.setOnRefreshListener(this);
			btnSend = FindViewById<Button>(Resource.Id.btn_send);
			btnSend.Click += btnSend_Click;
			textComments = FindViewById<EditText>(Resource.Id.text_comment);
            await ptrl.AutoRefresh();
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
				if (await NewsService.AddNewComments(CommonHelper.token, newsInfo.Id, content))
				{
					Msg.AppMsg.MakeText(this, GetString(Resource.String.publish_comments_success), Msg.AppMsg.STYLE_INFO).Show();
					await ptrl.AutoRefresh();
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
		}

		public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, NewsComment item, ViewHolder viewHolder)
		{
			viewHolder.GetView<TextView>(Resource.Id.text_comment).Text = Html.FromHtml(item.CommentContent).ToString();
			ImageView imgView = viewHolder.GetView<ImageView>(Resource.Id.img_head_2);
			if (imgView != null)
			{
				if (item.FaceUrl.Contains("png") || item.FaceUrl.Contains("jpg"))
					imgLoader.DisplayImage(item.FaceUrl, imgView, displayImageOptions);
				else
					Log.Debug("Not Image", item.FaceUrl);
			}
			viewHolder.GetView<TextView>(Resource.Id.text_floor).Text = item.Floor + "楼 " + item.DateAdded;
			viewHolder.GetView<TextView>(Resource.Id.text_commentators).Text = item.UserName;
			return viewHolder.GetConvertView();
		}

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () => 
			{
				comments = (await NewsService.GetNewsComments(CommonHelper.token, newsInfo.Id, 1, 25));
				comAdaper.RefreshItems(comments);
				pullToRefreshLayout.refreshFinish(0);
			},this);
		}

		public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
		{
			BaseService.ExeRequest(async () =>
			{
				int pageIndex = comments.Count / 25 + 1;
				var lastUpdateData = (await NewsService.GetNewsComments(CommonHelper.token, newsInfo.Id, pageIndex, 25));
				if (lastUpdateData.Count > 0)
				{
					comments.AddRange(lastUpdateData);
					comAdaper.RefreshItems(comments);
					int index = (comments.Count - 25) - 3;
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
			int pageIndex = comments.Count / 25 + 1;
			if (pageIndex <= 1)
			{
				Msg.AppMsg.MakeText(this, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
				return false;
			}
			return true;
		}
	}
}
