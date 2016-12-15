using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using CNBlogAPI.Model;
using Newtonsoft.Json;
using Msg = Sino.Droid.AppMsg;
using CNBlog.Droid.Utils;

namespace CNBlog.Droid.Activities
{
    [Activity(Label = "LoginActivity")]
	public class LoginActivity : Activity
	{
		Dialog waitDialog;
		EditText textUserName;
		EditText textPwd;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.login);
			FindViewById<TextView>(Resource.Id.head_title).Text = "博客园登录";
			Button btnBack = FindViewById<Button>(Resource.Id.title_bar_back);
			btnBack.Click+=delegate { Finish();};
			textUserName = FindViewById<EditText>(Resource.Id.text_username);
			textPwd = FindViewById<EditText>(Resource.Id.text_pwd);
			Button btnLogin = FindViewById<Button>(Resource.Id.btn_login);
			btnLogin.Click +=async delegate
			{
				string uName = textUserName.Text;
				if (string.IsNullOrWhiteSpace(uName))
				{
					Msg.AppMsg.MakeText(this, "请输入用户名", Msg.AppMsg.STYLE_ALERT).Show();
					return;
				}
				string pwd = textPwd.Text;
				if (string.IsNullOrWhiteSpace(pwd))
				{
					Msg.AppMsg.MakeText(this, "请输入密码", Msg.AppMsg.STYLE_ALERT).Show();
					return;
				}
				waitDialog = CommonHelper.CreateLoadingDialog(this, GetString(Resource.String.login_msg));
				try
				{
					waitDialog.Show();
                    RSACryptoService rsaService = new RSACryptoService(CNBlogAPI.Service.UserService.publicKey);
					AccessToken token = await CNBlogAPI.Service.UserService.Login(rsaService.Encrypt(uName), rsaService.Encrypt(pwd));
					if (token == null)
					{
						Msg.AppMsg.MakeText(this, "登录失败", Msg.AppMsg.STYLE_ALERT);
					}
					else
					{
						CommonHelper.token = token;
						CommonHelper.SaveTextFile(CommonHelper.TokenFileName, this, JsonConvert.SerializeObject(token));
						CommonHelper.userInfo = await CNBlogAPI.Service.UserService.GetCurrentLoginUserInfo(CommonHelper.token);
						CommonHelper.userInfo.UName = textUserName.Text;
						CommonHelper.userInfo.Pwd = textPwd.Text;
						CommonHelper.SaveTextFile(CommonHelper.UserInfoFileName, this, JsonConvert.SerializeObject(CommonHelper.userInfo));
						Msg.AppMsg.MakeText(this, "登录成功", Msg.AppMsg.STYLE_INFO).Show();
						Intent intent = new Intent(this, typeof(PersonalCenterActivity));
						intent.PutExtra("loginSuccessfulFlag", true);
						SetResult(Result.Ok, intent);
                        Finish();
					}
				}
				catch (Exception ex)
				{
					Msg.AppMsg.MakeText(this, "登录错误,请稍候再试", Msg.AppMsg.STYLE_ALERT).Show();
					Android.Util.Log.Debug("error:", ex.Message);
				}
				finally
				{
					waitDialog.Cancel();
				}
			};
		}
	}
}

