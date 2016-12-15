using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Runtime;
using Com.Nostra13.Universalimageloader.Cache.Disc.Naming;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Assist;
using Java.Lang;

namespace CNBlog.Droid.Utils
{

	public class BlogApplication : Application
	{
		public override void OnCreate()
		{
			base.OnCreate();
			AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
		}

		private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
		{
			string path = Android.OS.Environment.ExternalStorageDirectory.Path;
			string fileName = "/error.txt";
			using (var streamWriter = new StreamWriter(path + fileName, false))
			{
				streamWriter.WriteLine(e.Exception.InnerException+"," + e.Exception.Message + "," + e.Exception.StackTrace + "," + e.Exception.Source+","+e.Exception.Data);
			}
		}

		public BlogApplication(System.IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
            : base(handle,transfer)
        {
		}


		public static void InitImageLoader(Context context)
		{
			var config = new ImageLoaderConfiguration.Builder(context)
			       .ThreadPriority(Thread.NormPriority - 2)
				   .DenyCacheImageMultipleSizesInMemory()
				   .DiskCacheFileNameGenerator(new Md5FileNameGenerator())
				   .TasksProcessingOrder(QueueProcessingType.Lifo)
				   .WriteDebugLogs()
				   .Build();
			ImageLoader.Instance.Init(config);
		}
	}
}
