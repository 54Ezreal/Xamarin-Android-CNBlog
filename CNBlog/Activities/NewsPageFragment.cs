using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using CNBlog.Droid.PullableView;
using CNBlogAPI.Model;
using System.Collections.Generic;
using CNBlog.Droid.Utils;
using Android.Util;
using Msg = Sino.Droid.AppMsg;
using CNBlogAPI.Service;
using Android.Content;
using Newtonsoft.Json;

namespace CNBlog.Droid.Activities
{
    public class NewsPageFragment : BaseFragment, OnRefreshListener
    {
        CommonAdapter<NewInfo> comAdaper;
        List<NewInfo> news = new List<NewInfo>();
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.article_fragment_page, container, false);
			InitalComponents();
			plistView = view.FindViewById<PullableListView>(Resource.Id.lv_articles);
			comAdaper = new CommonAdapter<NewInfo>(this.Activity, Resource.Layout.new_list_item, news);
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
			Intent intent = new Intent(this.Activity,typeof(NewDetailActivity));
			intent.PutExtra("newInfo", JsonConvert.SerializeObject(news[e.Position]));
			StartActivity(intent);
		}

        public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, NewInfo item, ViewHolder viewHolder)
        {
				viewHolder.GetView<TextView>(Resource.Id.text_title).Text = item.Title;
				viewHolder.GetView<TextView>(Resource.Id.text_summary).Text = item.Summary;
				viewHolder.GetView<TextView>(Resource.Id.text_good_count).Text = item.DiggCount.ToString();
				viewHolder.GetView<TextView>(Resource.Id.text_watch_count).Text = item.ViewCount.ToString();
				viewHolder.GetView<TextView>(Resource.Id.text_talk_count).Text = item.CommentCount.ToString();
				viewHolder.GetView<TextView>(Resource.Id.text_publish_ago).Text = CommonHelper.DateDiff(DateTime.Now, item.DateAdded) + "前";
				ImageView imgView = viewHolder.GetView<ImageView>(Resource.Id.img_avatar);
				if (imgView != null)
				{
					if (item.TopicIcon.Contains("png") || item.TopicIcon.Contains("jpg") || item.TopicIcon.Contains("gif"))
					{
						imgLoader.DisplayImage(item.TopicIcon, imgView, displayImageOptions);
					}
					else
					{
						Log.Debug("Not Image", item.TopicIcon);
						imgView.SetImageResource(Resource.Drawable.girl);
					}
				}
				return viewHolder.GetConvertView();
        }

		public void onRefresh(PullToRefreshLayout pullToRefreshLayout)
        {
			BaseService.ExeRequest(async () => 
			{ 
				news = await NewsService.GetNews(CommonHelper.token, 1, pageSize);
				comAdaper.RefreshItems(news);
				pullToRefreshLayout.refreshFinish(0);
			}, this.Activity);
        }

        public void onLoadMore(PullToRefreshLayout pullToRefreshLayout)
        {
			BaseService.ExeRequest(async () =>
			{
				int pageIndex = news.Count / pageSize + 1;
				var moreNews = await NewsService.GetNews(CommonHelper.token, pageIndex, pageSize);
				if (moreNews.Count > 0)
				{
					news.AddRange(moreNews);
					comAdaper.RefreshItems(news);
					int index = (news.Count - 15) - 3;
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
            int pageIndex = news.Count / 15 + 1;
            if (pageIndex <= 1)
            {
                Msg.AppMsg.MakeText(this.Activity, "No More Item", Msg.AppMsg.STYLE_INFO).Show();
                return false;
            }
            return true;
        }
    }
}

