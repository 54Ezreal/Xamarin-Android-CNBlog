using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Msg = Sino.Droid.AppMsg;
using Android.Widget;
using CNBlog.Droid.Utils;
using CNBlogAPI;
using Com.Nostra13.Universalimageloader.Core;
namespace CNBlog.Droid.Activities
{
    [Activity(Label = "PersonalCenterActivity")]
	public class PersonalCenterActivity : Activity
	{
		ImageLoader imgLoader;
		DisplayImageOptions displayImageOptions;
		Button btnBack;
		Dialog waitDialog;
		TextView textName;
		TextView textAge;
		LinearLayout myBlog;
		LinearLayout myCollect;
		LinearLayout setting;
		Button btnLoginOut;
		RoundImageView rImgView;
		bool loginFlag = false;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.personal_center);
			FindViewById<TextView>(Resource.Id.head_title).Text = "博客园";
			bindControls();
			initalUserInfo();
		}

		private void bindControls()
		{
			BlogApplication.InitImageLoader(this);
			imgLoader = ImageLoader.Instance;
			displayImageOptions = new DisplayImageOptions.Builder()
														.ShowImageForEmptyUri(Resource.Drawable.girl)
														 .ShowImageOnFail(Resource.Drawable.girl)
														 .ShowImageOnLoading(Resource.Drawable.girl)
														 .CacheInMemory(true)
														 .CacheOnDisk(true)
														 .ResetViewBeforeLoading()
														 .Build();
			btnBack = FindViewById<Button>(Resource.Id.title_bar_back);
			btnBack .Click+=delegate { Finish();};
			textName = FindViewById<TextView>(Resource.Id.text_nickname);
			textAge = FindViewById<TextView>(Resource.Id.text_age);
			myBlog = FindViewById<LinearLayout>(Resource.Id.my_blog);
			myBlog.Click+=delegate
			{
				if (CommonHelper.userInfo == null)
				{
					Msg.AppMsg.MakeText(this, "请先登录", Msg.AppMsg.STYLE_INFO).Show();
					return;
				}
				StartActivity(new Intent(this, typeof(MyPostsActivity)));
			};
			myCollect = FindViewById<LinearLayout>(Resource.Id.my_collect);
			myCollect.Click+=delegate
			{
				if (CommonHelper.userInfo == null)
				{
					Msg.AppMsg.MakeText(this, "请先登录", Msg.AppMsg.STYLE_INFO).Show();
					return;
				}
				StartActivity(new Intent(this, typeof(BookMarksActivity)));
			};
			setting = FindViewById<LinearLayout>(Resource.Id.setting);
			setting.Click+=delegate 
			{
				AlertDialog.Builder builder = new AlertDialog.Builder(this);
				builder.SetMessage("APP作者:胡帅。一个不出名的帅小伙，只想安安静静写个代码。");
				builder.SetPositiveButton("我知道啦", (object sender, DialogClickEventArgs e) => 
				{
					AlertDialog dialog = sender as AlertDialog;
					dialog.Dismiss();
				});
				builder.SetTitle("提示");
				builder.Show();
			};
			btnLoginOut = FindViewById<Button>(Resource.Id.btn_loginout);
			btnLoginOut.Click += btnLoginOut_Click;
			rImgView = FindViewById<RoundImageView>(Resource.Id.img_headPic);
		}

		private async void btnLoginOut_Click(object sender,EventArgs e)
		{
			if (loginFlag)
			{
				//添加注销销逻辑
			    await loginOut();
			}
			else
			{
				//添加登录逻辑
				StartActivityForResult(new Intent(this, typeof(LoginActivity)), 200);
			}
		}

		private async Task loginOut()
		{
			waitDialog = CommonHelper.CreateLoadingDialog(this, GetString(Resource.String.loginout_msg));
			try
			{
				waitDialog.Show();
                //清空缓存信息
                CommonHelper.userInfo = null;
                CommonHelper.token = null;
                CommonHelper.SaveTextFile(CommonHelper.TokenFileName, this, "");
                CommonHelper.SaveTextFile(CommonHelper.UserInfoFileName, this, "");
                OAuthClient client = new OAuthClient();
                CommonHelper.token = await client.GetAccessToken();
                textAge.Visibility = ViewStates.Gone;
                rImgView.SetImageResource(Resource.Drawable.dog);
				textName.Text = "账号登录";
				btnLoginOut.Text = "登录博客园账号";
                loginFlag = false;
                Msg.AppMsg.MakeText(this, "注销成功", Msg.AppMsg.STYLE_INFO).Show();
			}
			catch (Exception ex)
			{
				Msg.AppMsg.MakeText(this, "注销错误", Msg.AppMsg.STYLE_ALERT).Show();
				Android.Util.Log.Debug("error:", ex.Message);
			}
			finally
			{
				waitDialog.Cancel();
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == 200 && data !=null)
			{
				loginFlag =data.GetBooleanExtra("loginSuccessfulFlag", false);
				if (loginFlag)
				{
					btnLoginOut.Text = "退出当前账号";
					if (CommonHelper.userInfo.Avatar.Contains("png") || CommonHelper.userInfo.Avatar.Contains("jpg"))
						//判断用户是否设置了头像
						imgLoader.DisplayImage(CommonHelper.userInfo.Avatar, rImgView, displayImageOptions);
					textAge.Visibility = ViewStates.Visible;
					textAge.Text = "园龄:" + CommonHelper.userInfo.Seniority;
					textName.Text = CommonHelper.userInfo.DisplayName;
				}
			}
			base.OnActivityResult(requestCode, resultCode, data);
		}

		private void initalUserInfo()
		{
			if (CommonHelper.userInfo != null)
			{
				loginFlag = true;
				textName.Text = CommonHelper.userInfo.DisplayName;
				textAge.Text = "园龄:" + CommonHelper.userInfo.Seniority;
				if (CommonHelper.userInfo.Avatar.Contains("png") || CommonHelper.userInfo.Avatar.Contains("jpg"))
					//判断用户是否设置了头像
					imgLoader.DisplayImage(CommonHelper.userInfo.Avatar, rImgView, displayImageOptions);
			}
			else
			{
				textAge.Visibility = ViewStates.Gone;
				textName.Text = "账号登录";
				btnLoginOut.Text = "登录博客园账号";
				loginFlag = false;
			}
		}

	}
}
