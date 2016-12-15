using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CNBlog.Droid.Utils;
using CNBlogAPI.Service;
using CNBlogAPI.Model;
using System.Threading.Tasks;
using Android.Util;
using Com.Nostra13.Universalimageloader.Core;
using Newtonsoft.Json;

namespace CNBlog.Droid.Activities
{
	[Activity(Label = "BloggerSearchActivity")]
	public class BloggerSearchActivity :Activity
	{
		ImageLoader imgLoader;
		DisplayImageOptions displayImageOptions;
		Button btnBack;
		ImageButton ibSearch;
		ListView lvBloggers;
		EditText textKeyWord;
		Dialog waitDialog;
		CommonAdapter<Blogger> comAdaper;
		List<Blogger> bloggers = new List<Blogger>();
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.blogger_search);
			bindControls();
		}

		private void bindControls()
		{
			imgLoader = ImageLoader.Instance;
			displayImageOptions = new DisplayImageOptions.Builder()
														.ShowImageForEmptyUri(Resource.Drawable.girl)
														 .ShowImageOnFail(Resource.Drawable.girl)
														 .ShowImageOnLoading(Resource.Drawable.girl)
														 .CacheInMemory(true)
														 .CacheOnDisk(true)
														 .ResetViewBeforeLoading()
														 .Build();
			FindViewById<TextView>(Resource.Id.head_title).Text = "博主搜索";
			btnBack = FindViewById<Button>(Resource.Id.title_bar_back);
			btnBack.Click += delegate { Finish();};
			ibSearch = FindViewById<ImageButton>(Resource.Id.ib_search);
			ibSearch.Click+=delegate 
			{
				string keyWord = textKeyWord.Text;
				if (string.IsNullOrWhiteSpace(keyWord)){
					return;
				}
				search(keyWord);
			};
			lvBloggers = FindViewById<ListView>(Resource.Id.lv_blogger);
			lvBloggers.ItemClick+=delegate(object sender, ListView.ItemClickEventArgs e)
			{
				Intent intent = new Intent(this, typeof(BloggerHomePageActivity));
				intent.PutExtra("blogger", JsonConvert.SerializeObject(bloggers[e.Position]));
				StartActivity(intent);	
			};
			comAdaper = new CommonAdapter<Blogger>(this, Resource.Layout.blogger_search_item, bloggers);
            comAdaper.OnGetView += comAdapter_OnGetView;
            lvBloggers.Adapter = comAdaper;
            textKeyWord = FindViewById<EditText>(Resource.Id.text_keyword);
		}

        public View comAdapter_OnGetView(int position, View convertView, ViewGroup parent, Blogger item, ViewHolder viewHolder)
        {
            viewHolder.GetView<TextView>(Resource.Id.text_author).Text = item.Title;
            viewHolder.GetView<TextView>(Resource.Id.text_homepage).Text = item.Id.ToString();
            viewHolder.GetView<TextView>(Resource.Id.text_blog_count).Text = "博客数:" + item.PostCount;
            viewHolder.GetView<TextView>(Resource.Id.text_lastupdate).Text = "最后更新:" + item.Updated;
			ImageView imgView = viewHolder.GetView<ImageView>(Resource.Id.img_avator);
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

        private async Task search(string keyWord)
		{
			waitDialog = CommonHelper.CreateLoadingDialog(this, GetString(Resource.String.query_waiting_msg));
			try
			{
				waitDialog.Show();
                bloggers = await BlogService.SearchBloggerAsync(keyWord);
				comAdaper.RefreshItems(bloggers);
            }
			catch (Exception)
			{
				
			}
			finally
			{
				waitDialog.Cancel();
			}
		}

	}
}

