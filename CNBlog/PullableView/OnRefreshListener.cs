namespace CNBlog.Droid.PullableView
{
    public interface OnRefreshListener
    {
        /// <summary>
        /// 刷新操作
        /// </summary>
        /// <param name="pullToRefreshLayout"></param>
		void onRefresh(PullToRefreshLayout pullToRefreshLayout);

        /// <summary>
        /// 加载更多
        /// </summary>
        /// <param name="pullToRefreshLayout"></param>
		void onLoadMore(PullToRefreshLayout pullToRefreshLayout);

		/// <summary>
		/// 是否可以加载更多
		/// </summary>
		/// <returns><c>true</c>, if load more was caned, <c>false</c> otherwise.</returns>
		bool CanLoadMore();
    }
}