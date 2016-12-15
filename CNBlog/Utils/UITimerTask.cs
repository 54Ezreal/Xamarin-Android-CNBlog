using Android.OS;
using Java.Util;

namespace CNBlog.Droid.Utils
{
    public class UITimerTask: TimerTask
    {

        private Handler handler;
        public UITimerTask(Handler uihandler)
        {
            handler = uihandler;
        }

        public override void Run()
        {
            handler.ObtainMessage().SendToTarget();
        }

    }
}