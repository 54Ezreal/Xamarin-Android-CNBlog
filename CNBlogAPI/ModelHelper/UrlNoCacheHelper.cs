using System;

namespace CNBlogAPI.ModelHelper
{
    internal static class UrlNoCacheHelper
    {
        internal static string WithCache(this string url)
        {
            if (url.Contains("?"))
            {
                return url + "&t=" + DateTime.Now.Ticks;
            }
            else
            {
                return url + "?t=" + DateTime.Now.Ticks;
            }
        }
    }
}
