namespace CNBlog.Droid.PullableView
{
    /// <summary>
    /// 如需扩展其它View，实现该接口即可
    /// </summary>
    public interface Pullable
	{
        /// <summary>
        /// 判断是否可以下拉，如果不需要下拉功能可以直接return false 
        /// </summary>
        /// <returns></returns>
		bool canPullDown();

        /// <summary>
        /// 判断是否可以上拉，如果不需要上拉功能可以直接return false 
        /// </summary>
        /// <returns></returns>
        bool canPullUp();
	}
}
