using Android.App;
using Android.Content;
using Android.OS;
using CNBlogAPI;
using System.Threading.Tasks;
using Msg = Sino.Droid.AppMsg;
using System;
using CNBlogAPI.Model;
using Newtonsoft.Json;
using CNBlog.Droid.Utils;
using Android.Content.PM;
using System.IO;

namespace CNBlog.Droid.Activities
{
    [Activity(Label = "博客园" , MainLauncher = true,LaunchMode = LaunchMode.SingleTop)]
	public class WelComeActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.welcome);
			loading();   
		}

		private async void loading()
		{
			try
			{
				await Task.Delay(500);
				if (!CommonHelper.CheckNetworkAvailable(this))
					throw new Exception("请打开网络连接");
				//1.检查是否已有缓存登录令牌
				CommonHelper.token = CommonHelper.ReadTextFile<AccessToken>("token.txt", this);
				//2.检查是否已有缓存用户信息
				CommonHelper.userInfo = CommonHelper.ReadTextFile<UserInfo>("userInfo.txt", this);
				if (CommonHelper.token == null || CommonHelper.token.CheckTokenIsOverdue())
				{
					OAuthClient client = new OAuthClient();
					if (CommonHelper.token != null && CommonHelper.userInfo != null)
					{
						RSACryptoService rsaService = new RSACryptoService(CNBlogAPI.Service.UserService.publicKey);
						CommonHelper.token = await CNBlogAPI.Service
							.UserService
							.Login(rsaService.Encrypt(CommonHelper.userInfo.UName), (rsaService.Encrypt(CommonHelper.userInfo.Pwd)));
					}
					else
					{
						CommonHelper.token = await client.GetAccessToken();
					}
					//保存令牌供下次使用
					CommonHelper.SaveTextFile("token.txt", this, JsonConvert.SerializeObject(CommonHelper.token));
				}
				StartActivity(new Intent(this, typeof(MainActivity)));
			}
			catch (Exception ex)
			{
				string path = Android.OS.Environment.ExternalStorageDirectory.Path;
				string fileName = "/error.txt";
				using (var streamWriter = new StreamWriter(path + fileName, false))
				{
					streamWriter.WriteLine(ex.InnerException + "," + ex.Message + "," + ex.StackTrace + "," + ex.Source + "," + ex.Data);
				}
				Msg.AppMsg.MakeText(this, ex.Message, Msg.AppMsg.STYLE_ALERT).Show();
				await Task.Delay(1000);
				Android.Util.Log.Debug("error:", ex.Message);
				Finish();
			}
		}

		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
			if ((ActivityFlags.ClearTop & intent.Flags) != 0)
			{
				Finish();
			}
		}
	}
}

