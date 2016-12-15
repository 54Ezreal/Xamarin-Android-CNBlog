using System.IO;
using Android.OS;
using Java.Util;

namespace CNBlog.Droid.Utils
{
    public class UIScheduling
    {
        private Handler handler;
        private Timer timer;
        private UITimerTask mTask;
        public UIScheduling(Handler uiHandler)
        {
            handler = uiHandler;
            timer = new Timer();
        }

        public void Schedule(long period)
        {
			try
			{
				if (mTask != null)
				{
					mTask.Cancel();
					mTask = null;
				}
				mTask = new UITimerTask(handler);
				timer.Schedule(mTask, 0, period);
			}
			catch (System.Exception e)
			{
				string path = Android.OS.Environment.ExternalStorageDirectory.Path;
				string fileName = "/error.txt";
				using (var streamWriter = new StreamWriter(path + fileName, false))
				{
					streamWriter.WriteLine(e.InnerException + "," + e.Message + "," + e.StackTrace + "," + e.Source + "," + e.Data);
				}
			}
        }

        public void Cancel()
        {
            if (mTask != null)
            {
                mTask.Cancel();
                mTask = null;
            }
        }
    }
}