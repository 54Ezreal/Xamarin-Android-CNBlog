using Android.App;
using Android.Util;
using System;
using System.Net;
using Msg = Sino.Droid.AppMsg;
using CNBlogAPI;

namespace CNBlog.Droid.Utils
{
    public class BaseService
	{

		public static async void ExeRequest(Action action,Activity context)
		{
			try
			{
				//每次检测令牌是否过期
				if (CommonHelper.token == null || CommonHelper.token.CheckTokenIsOverdue())
				{
					OAuthClient client = new OAuthClient();
					if (CommonHelper.token != null && CommonHelper.userInfo != null)
					{
						RSACryptoService rsaService = new RSACryptoService(CNBlogAPI.Service.UserService.publicKey);
						CommonHelper.token =await CNBlogAPI.Service
							.UserService
							.Login(rsaService.Encrypt(CommonHelper.userInfo.UName), (rsaService.Encrypt(CommonHelper.userInfo.Pwd)));
					}
					else
					{
						CommonHelper.token =await client.GetAccessToken();
					}
				}
				action();
			}
            catch (WebException ex)
            {
                Msg.AppMsg.MakeText(context, context.GetString(Resource.String.check_network), Msg.AppMsg.STYLE_ALERT).Show();
                Log.Debug("Error", ex.Message);
            }
            catch (Exception ex)
			{
                Msg.AppMsg.MakeText(context,context.GetString(Resource.String.request_error), Msg.AppMsg.STYLE_ALERT).Show();
                Log.Debug("Error", ex.Message);
            }
		}

	}
}
