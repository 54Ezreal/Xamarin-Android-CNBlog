using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using CNBlog.Droid.Utils;

namespace CNBlog.Droid.PullableView
{
    public class PullToRefreshLayout : RelativeLayout
    {
        // 初始状态  
        private const int initStatus = 0;
        // 释放刷新  
        private const int releaseToRefresh = 1;
        // 正在刷新  
        private const int refreshing = 2;
        // 释放加载  
        private const int releaseToLoad = 3;
        // 正在加载  
        private const int loading = 4;
        // 操作完毕  
        private const int complete = 5;
        // 当前状态  
        private int currentStatus = 0;
        // 刷新回调接口  
        private OnRefreshListener mListener;
        // 刷新成功  
        private const int succeed = 0;
        // 刷新失败  
        private const int failed = 1;
        // 按下Y坐标，上一个事件点Y坐标  
        private float downY, lastY;

        // 下拉的距离。注意：pullDownY和pullUpY不可能同时不为0  
        private float pullDownY = 0;
        // 上拉的距离  
        private float pullUpY = 0;

        // 释放刷新的距离  
        private float refreshDist = 200;
        // 释放加载的距离  
        private float loadmoreDist = 200;

        private UIScheduling uScheduling;
        // 回滚速度  
        private float moveSpeed = 8;
        // 第一次执行布局  
        private bool isLayout = false;
        // 在刷新过程中滑动操作  
        private bool isTouch = false;
        // 手指滑动距离与下拉头的滑动距离比，中间会随正切函数变化  
        private float radio = 2;

        // 下拉箭头的转180°动画  
        private RotateAnimation rotateAnimation;
        // 均匀旋转动画  
        private RotateAnimation refreshingAnimation;

        // 下拉头  
        private View refreshView;
        // 下拉的箭头  
        public View pullView;
        // 正在刷新的图标  
        private View refreshingView;
        // 刷新结果图标  
        private View refreshStateImageView;
        // 刷新结果：成功或失败  
        private TextView refreshStateTextView;

        // 上拉头  
        private View loadmoreView;
        // 上拉的箭头  
        public View pullUpView;
        // 正在加载的图标  
        private View loadingView;
        // 加载结果图标  
        private View loadStateImageView;
        // 加载结果：成功或失败  
        private TextView loadStateTextView;
		//请求加载错误View
		private View errorView;
        // 实现了Pullable接口的View  
        private View pullableView;
        // 过滤多点触碰  
        private int mEvents;
        // 这两个变量用来控制pull的方向，如果不加控制，当情况满足可上拉又可下拉时没法下拉  
        private bool canPullDown = true;
        private bool canPullUp = true;
        private Context mContext;
        private Handler updateUIHandler;
        public void setOnRefreshListener(OnRefreshListener listener)
        {
            mListener = listener;
        }

        public PullToRefreshLayout(Context context)
            : base(context)
        {
            initView(context);
        }

        public PullToRefreshLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            initView(context);
        }

        public PullToRefreshLayout(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            initView(context);
        }

        private void initView(Context context)
        {
            mContext = context;
            updateUIHandler = new Handler((Message msg) =>
            {
					// 回弹速度随下拉距离moveDeltaY增大而增大
					moveSpeed = (float)(8 + 5 * Math.Tan(Math.PI / 2 / MeasuredHeight * (pullDownY + Math.Abs(pullUpY))));
					if (!isTouch)
					{
						// 正在刷新，且没有往上推的话则悬停，显示"正在刷新..."
						if (currentStatus == refreshing && pullDownY <= refreshDist)
						{
							pullDownY = refreshDist;
							uScheduling.Cancel();
						}
						else if (currentStatus == loading && -pullUpY <= loadmoreDist)
						{
							pullUpY = -loadmoreDist;
							uScheduling.Cancel();
						}
					}
					if (pullDownY > 0)
						pullDownY -= moveSpeed;
					else if (pullUpY < 0)
						pullUpY += moveSpeed;
					if (pullDownY < 0)
					{
						// 已完成回弹
						pullDownY = 0;
						pullView.ClearAnimation();
						// 隐藏下拉头时有可能还在刷新，只有当前状态不是正在刷新时才改变状态
						if (currentStatus != refreshing && currentStatus != loading)
							changeStatus(initStatus);
						uScheduling.Cancel();
						RequestLayout();
					}
					if (pullUpY > 0)
					{
						// 已完成回弹
						pullUpY = 0;
						pullUpView.ClearAnimation();
						// 隐藏上拉头时有可能还在刷新，只有当前状态不是正在刷新时才改变状态
						if (currentStatus != refreshing && currentStatus != loading)
							changeStatus(initStatus);
						uScheduling.Cancel();
						RequestLayout();
					}
					// 刷新布局,会自动调用onLayout
					RequestLayout();
					// 没有拖拉或者回弹完成
					if (pullDownY + Math.Abs(pullUpY) == 0)
					{
						uScheduling.Cancel();
					}
            });
            uScheduling = new UIScheduling(updateUIHandler);
            rotateAnimation = (RotateAnimation)AnimationUtils.LoadAnimation(
				context, Resource.Animator.reverse_anim);
            refreshingAnimation = (RotateAnimation)AnimationUtils.LoadAnimation(
				context, Resource.Animator.rotating);
            // 添加匀速转动动画
            LinearInterpolator lir = new LinearInterpolator();
            rotateAnimation.Interpolator = lir;
            refreshingAnimation.Interpolator = lir;
        }

        private void initView()
        {
            // 初始化下拉布局
            pullView = refreshView.FindViewById<View>(Resource.Id.pull_icon);
            refreshStateTextView = refreshView.FindViewById<TextView>(Resource.Id.state_tv);
            refreshingView = refreshView.FindViewById<View>(Resource.Id.refreshing_icon);
            refreshStateImageView = refreshView.FindViewById<View>(Resource.Id.state_iv);
            // 初始化上拉布局
            pullUpView = loadmoreView.FindViewById<View>(Resource.Id.pullup_icon);
            loadStateTextView = loadmoreView.FindViewById<TextView>(Resource.Id.loadstate_tv);
            loadingView = loadmoreView.FindViewById<View>(Resource.Id.loading_icon);
            loadStateImageView = loadmoreView.FindViewById<View>(Resource.Id.loadstate_iv);
        }

        /// <summary>
        /// 完成刷新操作，显示刷新结果。注意：刷新完成后一定要调用这个方法
        /// </summary>
        /// <param name="refreshResult">succeed代表成功, failed代表失败</param>
        public void refreshFinish(int refreshResult)
        {
            refreshingView.ClearAnimation();
            refreshingView.Visibility = ViewStates.Gone;
            switch (refreshResult)
            {
                case 0:
                    // 刷新成功
                    refreshStateImageView.Visibility = ViewStates.Visible;
                    refreshStateTextView.Text = "刷新成功";
					refreshStateImageView.SetBackgroundResource(Resource.Mipmap.refresh_succeed);
                    break;
                case 1:
                default:
                    // 刷新失败
                    refreshStateImageView.Visibility = ViewStates.Visible;
                    refreshStateTextView.Text = "刷新失败";
					refreshStateImageView.SetBackgroundResource(Resource.Mipmap.refresh_failed);
                    break;
            }
            if (pullDownY > 0)
            {
                // 刷新结果停留1秒
                new Handler((Message msg) =>
                {
					changeStatus(complete);
                    hide();
                }).SendEmptyMessageDelayed(0, 1000);
            }
            else
            {
                changeStatus(complete);
                hide();
            }
        }

        /// <summary>
        /// 加载完毕，显示加载结果。注意：刷新完成后一定要调用这个方法
        /// </summary>
        /// <param name="refreshResult">succeed代表成功, failed代表失败</param>
        public void loadmoreFinish(int refreshResult)
        {
            loadingView.ClearAnimation();
            loadingView.Visibility = ViewStates.Gone;
            switch (refreshResult)
            {
                case 0:
                    // 加载成功
                    loadStateImageView.Visibility = ViewStates.Visible;
                    loadStateTextView.Text = "加载成功";
					loadStateImageView.SetBackgroundResource(Resource.Mipmap.load_succeed);
					break;
                case 1:
                default:
                    // 加载失败
                    loadStateImageView.Visibility = ViewStates.Visible;
                    loadStateTextView.Text = "加载失败";
					loadStateImageView.SetBackgroundResource(Resource.Mipmap.load_failed);
					pullableView.Visibility = ViewStates.Gone;
                    break;
            }
            if (pullUpY < 0)
            {
                // 刷新结果停留1秒
                new Handler((Message msg) =>
                {
                    changeStatus(complete);
                    hide();
                }).SendEmptyMessageDelayed(0, 1000);
            }
            else
            {
                changeStatus(complete);
                hide();
            }
        }

        /// <summary>
        /// 改变界面布局状态
        /// </summary>
        /// <param name="status"></param>
		private void changeStatus(int status)
        {
            currentStatus = status;
            Log.Debug("status:", status.ToString());
            switch (currentStatus)
            {
                case 0:
                    // 下拉布局初始状态
                    refreshStateImageView.Visibility = ViewStates.Gone;
                    refreshStateTextView.Text = Context.GetString(Resource.String.pull_to_refresh);
                    pullView.ClearAnimation();
                    pullView.Visibility = ViewStates.Visible;
                    // 上拉布局初始状态
                    loadStateImageView.Visibility = ViewStates.Gone;
                    loadStateTextView.Text = Context.GetString(Resource.String.pullup_to_load);
                    pullUpView.ClearAnimation();
                    pullUpView.Visibility = ViewStates.Visible;
                    break;
                case 1:
                    // 释放刷新状态
                    refreshStateTextView.Text = Context.GetString(Resource.String.release_to_refresh);
                    pullView.StartAnimation(rotateAnimation);
                    break;
                case 2:
                    // 正在刷新状态
                    pullView.ClearAnimation();
                    refreshingView.Visibility = ViewStates.Visible;
                    pullView.Visibility = ViewStates.Invisible;
                    refreshingView.StartAnimation(refreshingAnimation);
                    refreshStateTextView.Text = Context.GetString(Resource.String.refreshing);
                    break;
                case 3:
                    // 释放加载状态
                    loadStateTextView.Text = Context.GetString(Resource.String.release_to_load);
                    pullUpView.StartAnimation(rotateAnimation);
                    break;
                case 4:
                    // 正在加载状态
                    pullUpView.ClearAnimation();
                    loadingView.Visibility = ViewStates.Visible;
                    pullUpView.Visibility = ViewStates.Invisible;
                    loadingView.StartAnimation(refreshingAnimation);
                    loadStateTextView.Text = Context.GetString(Resource.String.loading);
                    break;
                case 5:
                    // 刷新或加载完毕，啥都不做
                    break;
            }
        }

        /// <summary>
        /// 不限制上拉或下拉
        /// </summary>
        private void releasePull()
        {
            canPullDown = true;
            canPullUp = true;
        }


        /// <summary>
        /// 由父控件决定是否分发事件，防止事件冲突
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool DispatchTouchEvent(MotionEvent e)
        {
            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    downY = e.GetY();
                    lastY = downY;
                    uScheduling.Cancel();
                    mEvents = 0;
                    releasePull();
                    break;
                case MotionEventActions.PointerDown:
                case MotionEventActions.PointerUp:
                    // 过滤多点触碰
                    mEvents = -1;
                    break;
                case MotionEventActions.Move:
                    if (mEvents == 0)
                    {
                        if (pullDownY > 0
                                || (((Pullable)pullableView).canPullDown()
                                        && canPullDown && currentStatus != loading))
                        {
                            // 可以下拉，正在加载时不能下拉
                            // 对实际滑动距离做缩小，造成用力拉的感觉
                            pullDownY = pullDownY + (e.GetY() - lastY) / radio;
                            if (pullDownY < 0)
                            {
                                pullDownY = 0;
                                canPullDown = false;
                                canPullUp = true;
                            }
                            if (pullDownY > this.MeasuredHeight)
                                pullDownY = this.MeasuredHeight;
                            if (currentStatus == refreshing)
                            {
                                // 正在刷新的时候触摸移动
                                isTouch = true;
                            }
                        }
                        else if (pullUpY < 0
                              || (((Pullable)pullableView).canPullUp() && canPullUp && currentStatus != refreshing))
                        {
                            // 可以上拉，正在刷新时不能上拉
                            pullUpY = pullUpY + (e.GetY() - lastY) / radio;
                            if (pullUpY > 0)
                            {
                                pullUpY = 0;
                                canPullDown = true;
                                canPullUp = false;
                            }
                            if (pullUpY < -this.MeasuredHeight)
                                pullUpY = -this.MeasuredHeight;
                            if (currentStatus == loading)
                            {
                                // 正在加载的时候触摸移动
                                isTouch = true;
                            }
                        }
                        else
                            releasePull();
                    }
                    else
                        mEvents = 0;
                    lastY = e.GetY();
                    // 根据下拉距离改变比例
                    radio = (float)(2 + 2 * Math.Tan(Math.PI / 2 / this.MeasuredHeight
                                                     * (pullDownY + Math.Abs(pullUpY))));
                    if (pullDownY > 0 || pullUpY < 0)
                        RequestLayout();
                    if (pullDownY > 0)
                    {
                        if (pullDownY <= refreshDist
                                && (currentStatus == releaseToRefresh || currentStatus == complete))
                        {
                            // 如果下拉距离没达到刷新的距离且当前状态是释放刷新，改变状态为下拉刷新
                            changeStatus(initStatus);
                        }
                        if (pullDownY >= refreshDist && currentStatus == initStatus)
                        {
                            // 如果下拉距离达到刷新的距离且当前状态是初始状态刷新，改变状态为释放刷新
                            changeStatus(releaseToRefresh);
                        }
                    }
                    else if (pullUpY < 0)
                    {
                        // 下面是判断上拉加载的，同上，注意pullUpY是负值
                        if (-pullUpY <= loadmoreDist
						    && (currentStatus == releaseToLoad || currentStatus == complete) && mListener.CanLoadMore() )
                        {
                            changeStatus(initStatus);
                        }
                        // 上拉操作
						if (-pullUpY >= loadmoreDist && currentStatus == initStatus && mListener.CanLoadMore() )
                        {
                            changeStatus(releaseToLoad);
                        }

                    }
                    // 因为刷新和加载操作不能同时进行，所以pullDownY和pullUpY不会同时不为0，因此这里用(pullDownY +
                    // Math.Abs(pullUpY))就可以不对当前状态作区分了
                    if ((pullDownY + Math.Abs(pullUpY)) > 8)
                    {
                        // 防止下拉过程中误触发长按事件和点击事件
                        e.Action = MotionEventActions.Cancel;
                    }
                    break;
                case MotionEventActions.Up:
                    if (pullDownY > refreshDist || -pullUpY > loadmoreDist)
                    // 正在刷新时往下拉（正在加载时往上拉），释放后下拉头（上拉头）不隐藏
                    {
                        isTouch = false;
                    }
                    if (currentStatus == releaseToRefresh)
                    {
                        changeStatus(refreshing);
                        // 刷新操作
                        if (mListener != null)
                            mListener.onRefresh(this);
                    }
                    else if (currentStatus == releaseToLoad)
                    {
                        changeStatus(loading);
                        // 加载操作
						if (mListener != null)
                            mListener.onLoadMore(this);
                    }
                    hide();
                    break;
                default:
                    break;
            }
            base.DispatchTouchEvent(e);
            return true;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            if (!isLayout)
            {
                // 这里是第一次进来的时候做一些初始化
                refreshView = GetChildAt(0);
                pullableView = GetChildAt(1);
                loadmoreView = GetChildAt(2);
                isLayout = true;
                initView();
                refreshDist = ((ViewGroup)refreshView).GetChildAt(0)
                        .MeasuredHeight;
                loadmoreDist = ((ViewGroup)loadmoreView).GetChildAt(0)
                        .MeasuredHeight;
            }
            // 改变子控件的布局，这里直接用(pullDownY + pullUpY)作为偏移量，这样就可以不对当前状态作区分
            refreshView.Layout(0,
                    (int)(pullDownY + pullUpY) - refreshView.MeasuredHeight,
                    refreshView.MeasuredWidth, (int)(pullDownY + pullUpY));
            pullableView.Layout(0, (int)(pullDownY + pullUpY),
                    pullableView.MeasuredWidth, (int)(pullDownY + pullUpY)
                            + pullableView.MeasuredHeight);
            loadmoreView.Layout(0,
                    (int)(pullDownY + pullUpY) + pullableView.MeasuredHeight,
                    loadmoreView.MeasuredWidth,
                    (int)(pullDownY + pullUpY) + pullableView.MeasuredHeight
                            + loadmoreView.MeasuredHeight);
        }

        /// <summary>
        /// 隐藏刷新UI界面
        /// </summary>
        private void hide()
        {
            uScheduling.Schedule(5);
        }

		public async Task AutoRefresh()
		{
			while (pullDownY < 4 / 3 * refreshDist)
			{
				pullDownY += moveSpeed;
				if (pullDownY > refreshDist)
					changeStatus(releaseToRefresh);
				RequestLayout();
				await Task.Delay(20);
			}
			changeStatus(refreshing);
			if(mListener!=null)
				mListener.onRefresh(this);
			hide();
		}
    }
}
