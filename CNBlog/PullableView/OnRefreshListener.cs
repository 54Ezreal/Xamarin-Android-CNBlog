namespace CNBlog.Droid.PullableView
{
    public interface OnRefreshListener
    {
        /// <summary>
        /// ˢ�²���
        /// </summary>
        /// <param name="pullToRefreshLayout"></param>
		void onRefresh(PullToRefreshLayout pullToRefreshLayout);

        /// <summary>
        /// ���ظ���
        /// </summary>
        /// <param name="pullToRefreshLayout"></param>
		void onLoadMore(PullToRefreshLayout pullToRefreshLayout);

		/// <summary>
		/// �Ƿ���Լ��ظ���
		/// </summary>
		/// <returns><c>true</c>, if load more was caned, <c>false</c> otherwise.</returns>
		bool CanLoadMore();
    }
}