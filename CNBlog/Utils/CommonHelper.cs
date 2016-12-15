using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using CNBlogAPI.Model;
using Android.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Android.Graphics;
using System.Linq;
using CN.Sharesdk.Onekeyshare;
using CN.Sharesdk.Framework;
using CNBlogAPI.Service;
using Msg = Sino.Droid.AppMsg;
namespace CNBlog.Droid.Utils
{
    public class CommonHelper
	{
        public static AccessToken token { get; set; }
        public static UserInfo userInfo { get; set; }
        /// <summary>
        /// 存储令牌文件名
        /// </summary>
        public static readonly string TokenFileName = "token.txt";

        /// <summary>
        /// 存储用户信息文件名
        /// </summary>
        public static readonly string UserInfoFileName = "userInfo.txt";

        /// <summary>
        /// 计算两个时间差
        /// </summary>
        /// <param name="DateTime1"></param>
        /// <param name="DateTime2"></param>
        /// <returns></returns>
        public static string DateDiff(DateTime DateTime1, DateTime DateTime2)
		{
			string dateDiff = null;
			TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
			TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
			TimeSpan ts = ts1.Subtract(ts2).Duration();
			if (ts.Days != 0)
			{
				dateDiff = ts.Days.ToString() + "天";
				return dateDiff;
			}
			if (ts.Hours != 0)
			{
				dateDiff += ts.Hours.ToString() + "小时";
				return dateDiff;
			}
			if (ts.Minutes != 0)
			{
				dateDiff += ts.Minutes.ToString() + "分钟";
				return dateDiff;
			}
			dateDiff = ts.Seconds.ToString() + "秒";
			return dateDiff;
			#region note
			//C#中使用TimeSpan计算两个时间的差值
			//可以反加两个日期之间任何一个时间单位。
			//TimeSpan ts = Date1 - Date2;
			//double dDays = ts.TotalDays;//带小数的天数，比如1天12小时结果就是1.5 
			//int nDays = ts.Days;//整数天数，1天12小时或者1天20小时结果都是1  
			#endregion
		}

        /// <summary>
        /// 创建一个等待框
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
		public static Dialog CreateLoadingDialog(Context context, string msg)
		{
			LayoutInflater inflater = LayoutInflater.From(context);
			View v = inflater.Inflate(Resource.Layout.loading_dialog, null);
			LinearLayout layout = v.FindViewById<LinearLayout>(Resource.Id.dialog_view);
			ImageView spaceshipImage = v.FindViewById<ImageView>(Resource.Id.img);
			TextView tipTextView = v.FindViewById<TextView>(Resource.Id.tipTextView);
			Animation hyperspaceJumpAnimation = AnimationUtils.LoadAnimation(context, Resource.Animator.loading_animation);
			spaceshipImage.StartAnimation(hyperspaceJumpAnimation);
			tipTextView.Text = msg;
			Dialog loadingDialog = new Dialog(context, Resource.Style.loading_dialog);
			loadingDialog.SetCancelable(false);
			loadingDialog.SetContentView(layout, new LinearLayout.LayoutParams(
				LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent));
			return loadingDialog;
		}

		/// <summary>
		/// 检查网络是否可用
		/// </summary>
		/// <returns><c>true</c>, if network available was checked, <c>false</c> otherwise.</returns>
		/// <param name="activity">Activity.</param>
		public static bool CheckNetworkAvailable(Activity activity)
		{
			Context context = activity.ApplicationContext;
			// 获取手机所有连接管理对象（包括对wi-fi,net等连接的管理）
			ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
			if (connectivityManager == null)
			{
				return false;
			}
			else
			{
				// 获取NetworkInfo对象
				NetworkInfo[] networkInfo = connectivityManager.GetAllNetworkInfo();
				if (networkInfo != null && networkInfo.Length > 0)
				{
					for (int i = 0; i < networkInfo.Length; i++)
					{
						// 判断当前网络状态是否为连接状态
						if (networkInfo[i].GetState() == NetworkInfo.State.Connected)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

        /// <summary>
        /// 缩放图片
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
		public static Bitmap ZoomBitmap(Bitmap bitmap, int width, int height)
		{
			int w = bitmap.Width;
			int h = bitmap.Height;
			Matrix matrix = new Matrix();
			float scaleWidth = ((float)width / w);
			float scaleHeight = ((float)height / h);
			matrix.PostScale(scaleWidth, scaleHeight);
			Bitmap newbmp = Bitmap.CreateBitmap(bitmap, 0, 0, w, h, matrix, true);
			return newbmp;
		}

		public static void SaveTextFile(string fileName ,Activity context,string content)
		{
			using (var outStream = context.OpenFileOutput(fileName, FileCreationMode.Private))
			{
				byte[] buffer = Encoding.UTF8.GetBytes(content);
				outStream.Write(buffer, 0, buffer.Length);
			}
		}

		public static T ReadTextFile<T>(string filePath, Activity context)
		{
			string content = "";
			if (!context.FileList().Any(p => p == filePath))
			{
				return default(T);
			}
			using (var inStream = context.OpenFileInput(filePath))
			{
				StreamReader sr = new StreamReader(inStream);
				content = sr.ReadToEnd();
			}
			if (string.IsNullOrWhiteSpace(content))
				return default(T);
			return JsonConvert.DeserializeObject<T>(content);
		}

		public static void SaveBinaryFile(byte [] buffer,string fileName)
		{
			string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			string fileFullPath =System.IO.Path.Combine(path, fileName);
			using (FileStream fs = new FileStream(fileFullPath, FileMode.Create))
			{
				fs.Write(buffer, 0, buffer.Length);
			}
		}

        /**
        * 演示调用ShareSDK执行分享
        * @param context
        * @param platformToShare  指定直接分享平台名称（一旦设置了平台名称，则九宫格将不会显示）
        * @param showContentEdit  是否显示编辑页
        */
        public static void InitalShare(Activity activity,  string platformToShare, bool showContentEdit, string title, string shareContent, string imageUrl, string articleUrl)
        {
            ShareSDK.InitSDK(activity); // 初始化ShareSDK
            Button btnShare = activity.FindViewById<Button>(Resource.Id.footbar_share);
            btnShare.Click += delegate 
            {
                OnekeyShare _oneKeyShare = new OnekeyShare();
                _oneKeyShare.SetSilent(!showContentEdit);
                if (platformToShare != null)
                {
                    _oneKeyShare.SetPlatform(platformToShare);
                }
                //ShareSDK快捷分享提供两个界面第一个是九宫格 CLASSIC  第二个是SKYBLUE
                _oneKeyShare.SetTheme(OnekeyShareTheme.Classic);
                // 令编辑页面显示为Dialog模式
                _oneKeyShare.SetDialogMode();
                // 在自动授权时可以禁用SSO方式
                _oneKeyShare.DisableSSOWhenAuthorize();
                //_oneKeyShare.setAddress("12345678901"); //分享短信的号码和邮件的地址
                _oneKeyShare.SetTitle(title);
                _oneKeyShare.SetTitleUrl(articleUrl);
                //_oneKeyShare.SetText("ShareSDK--文本");
                _oneKeyShare.Text = shareContent;
                //_oneKeyShare.setImagePath("/sdcard/test-pic.jpg");  //分享sdcard目录下的图片
                _oneKeyShare.SetImageUrl(imageUrl);
                _oneKeyShare.SetUrl("http://www.cnblogs.com/CallMeUncle/"); //微信不绕过审核分享链接
                                                                            //_oneKeyShare.setFilePath("/sdcard/test-pic.jpg");  //filePath是待分享应用程序的本地路劲，仅在微信（易信）好友和Dropbox中使用，否则可以不提供
                _oneKeyShare.SetComment("分享"); //我对这条分享的评论，仅在人人网和QQ空间使用，否则可以不提供
                _oneKeyShare.SetSite("博客园-IT胡小帅");  //QZone分享完之后返回应用时提示框上显示的名称
                _oneKeyShare.SetSiteUrl(articleUrl);//QZone分享参数
                _oneKeyShare.SetVenueName("博客园-IT胡小帅");
                _oneKeyShare.SetVenueDescription("APP作者:胡帅。一个不出名的帅小伙，只想安安静静写个代码。");
                // 将快捷分享的操作结果将通过OneKeyShareCallback回调
                //_oneKeyShare.setCallback(new OneKeyShareCallback());
                // 去自定义不同平台的字段内容
                //_oneKeyShare.setShareContentCustomizeCallback(new ShareContentCustomizeDemo());
                // 在九宫格设置自定义的图标
                //Bitmap logo = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.ic_launcher);
                //String label = "ShareSDK";
                //var listener = new CustomOnClickListener();
                //_oneKeyShare.SetCustomerLogo(logo, label, listener);
                // 为EditPage设置一个背景的View
                //_oneKeyShare.setEditPageBackground(getPage());
                // 隐藏九宫格中的新浪微博,短信,邮件,微信收藏
                _oneKeyShare.AddHiddenPlatform("SinaWeibo");
                _oneKeyShare.AddHiddenPlatform("ShortMessage");
                _oneKeyShare.AddHiddenPlatform("Email");
                _oneKeyShare.AddHiddenPlatform("WechatFavorite");
                // String[] AVATARS = {
                // "http://99touxiang.com/public/upload/nvsheng/125/27-011820_433.jpg",
                // "http://img1.2345.com/duoteimg/qqTxImg/2012/04/09/13339485237265.jpg",
                // "http://diy.qqjay.com/u/files/2012/0523/f466c38e1c6c99ee2d6cd7746207a97a.jpg",
                // "http://diy.qqjay.com/u2/2013/0422/fadc08459b1ef5fc1ea6b5b8d22e44b4.jpg",
                // "http://img1.2345.com/duoteimg/qqTxImg/2012/04/09/13339510584349.jpg",
                // "http://diy.qqjay.com/u2/2013/0401/4355c29b30d295b26da6f242a65bcaad.jpg" };
                // oks.setImageArray(AVATARS);//腾讯微博和twitter用此方法分享多张图片，其他平台不可以
                // 启动分享
                _oneKeyShare.Show(activity);
            };
        }

        public static void InitalBookMark(Activity activity,string url,string title)
        {
            bool isCollected = false;
            Button btnCollect = activity. FindViewById<Button>(Resource.Id.footbar_collect);
            BaseService.ExeRequest(async () =>
            {
                if (userInfo == null)
                {
                    Msg.AppMsg.MakeText(activity, activity.GetString(Resource.String.please_login), Msg.AppMsg.STYLE_INFO).Show();
                    return;
                }
                if (await BookMarkService.CheckExistsBookMark(token, url))
                {
                    btnCollect.Background = activity.GetDrawable(Resource.Drawable.collected);
                    isCollected = true;
                }
            }, activity);
            btnCollect.Click += delegate
            {
                if (isCollected)
                {
                    //如果收藏就取消
                    BaseService.ExeRequest(async () =>
                    {
                        if (await BookMarkService.DeleteBookMark(token, Java.Net.URLEncoder.Encode(url)))
                        {
                            isCollected = false;
                            btnCollect.Background =activity.GetDrawable(Resource.Drawable.collect_v1);
                            Msg.AppMsg.MakeText(activity, "取消收藏成功", Msg.AppMsg.STYLE_INFO).Show();
                        }
                        else
                        {
                            Msg.AppMsg.MakeText(activity, "取消收藏失败,请稍候再试", Msg.AppMsg.STYLE_INFO).Show();
                        }
                    }, activity);
                }
                else
                {
                    //未被收藏则收藏
                    BaseService.ExeRequest(async () =>
                    {
                        if (await BookMarkService.AddBookMark(token, title, url))
                        {
                            isCollected = true;
                            btnCollect.Background = activity.GetDrawable(Resource.Drawable.collected);
                            Msg.AppMsg.MakeText(activity, activity.GetString(Resource.String.mark_success), Msg.AppMsg.STYLE_INFO).Show();
                        }
                        else
                        {
                            Msg.AppMsg.MakeText(activity, activity.GetString(Resource.String.mark_fail), Msg.AppMsg.STYLE_INFO).Show();
                        }
                    }, activity);
                }
            };
        }

    }
}

