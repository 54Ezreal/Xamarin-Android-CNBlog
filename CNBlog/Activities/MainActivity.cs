using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using Android.Graphics;
using CNBlog.Droid.Utils;

namespace CNBlog.Droid.Activities
{
	[Activity(Label = "ThirdActivity")]
	public class MainActivity : FragmentActivity
	{
		private SimpleFragmentPagerAdapter pagerAdapter;
		private ViewPager viewPager;
		private TabLayout tabLayout;
		private Button btnBloggerSearch;
		private Button btn_menu;
		private DateTime? lastBackKeyDownTime;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.main);
			btnBloggerSearch = FindViewById<Button>(Resource.Id.btn_blogger_search);
			btnBloggerSearch.Click += delegate
			{
				StartActivity(new Intent(this, typeof(BloggerSearchActivity)));
			};
			pagerAdapter = new SimpleFragmentPagerAdapter(SupportFragmentManager, this);
			viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
			//设置ViewPager缓存Fragment数量
			//viewPager.OffscreenPageLimit = 4;
			viewPager.Adapter = pagerAdapter;
			tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
			tabLayout.SetupWithViewPager(viewPager);
			tabLayout.TabMode = TabLayout.ModeFixed;
			for (int i = 0; i < tabLayout.TabCount; i++)
			{
				TabLayout.Tab tab = tabLayout.GetTabAt(i);
				tab.SetCustomView(pagerAdapter.GetTabView(i));
			}
			btn_menu = FindViewById<Button>(Resource.Id.title_bar_left_menu);
			btn_menu.Click += delegate
			{
				StartActivity(new Intent(this, typeof(PersonalCenterActivity)));
			};
			//slideMenu();
			//bindMenu();
		}

		public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
			{
				if (!lastBackKeyDownTime.HasValue || DateTime.Now - lastBackKeyDownTime.Value > new TimeSpan(0, 0, 2))
				{
					Toast.MakeText(this.ApplicationContext, "再按一次退出程序", ToastLength.Short).Show();
					lastBackKeyDownTime = DateTime.Now;
				}
				else
				{
					Intent intent = new Intent();
					intent.SetClass(this, typeof(WelComeActivity));
					intent.SetFlags(ActivityFlags.ClearTop);
					StartActivity(intent);
				}
				return true;
			}
			return base.OnKeyDown(keyCode, e);
		}

		private void bindMenu()
		{
			LinearLayout setting = FindViewById<LinearLayout>(Resource.Id.slide_menu_setting);
			setting.Click += delegate
			{
				//StartActivity(new Intent(this, typeof(SettingActivity)));
			};
			if (CommonHelper.userInfo != null)
			{
				string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
				string fileFullPath = System.IO.Path.Combine(path, "head.png");
				Bitmap bmp = BitmapFactory.DecodeFile(fileFullPath);
				FindViewById<RoundImageView>(Resource.Id.img_head).SetImageBitmap(bmp);
			}
		}

		/// <summary>
		/// 侧滑菜单
		/// </summary>
		protected void slideMenu()
		{
			//var sm = new SlidingMenu(this);
			//设置菜单从右滑出
			//sm.Mode = MenuMode.Left;
			//sm.SetMenu(Resource.Layout.menu);
			//指定主界面显示的宽度
			Point point = new Point();
			this.WindowManager.DefaultDisplay.GetSize(point);
			//sm.BehindOffset = (point.X / 2) - 220;
			//设置滑动强度
			//sm.BehindScrollScale = 0f;
			//sm.FadeEnabled = true;
			//sm.FadeDegree = 1f;
			//sm.ShadowWidth = 15;
			//sm.ShadowDrawableRes = Resource.Drawable.shadow;

			//sm.TouchModeAbove = TouchMode.None;

			//FindViewById<Button>(Resource.Id.title_bar_left_menu).Click += (object sender, EventArgs e) =>
			//{
				//sm.ShowMenu();
			//};
			//sm.Opened += delegate
			 // {
				//侧滑菜单滚出事件
			//};
			//sm.AttachToActivity(this, SlideStyle.Content);
		}

		//public override bool DispatchTouchEvent(MotionEvent ev)
		//{
		//	return resideMenu.DispatchTouchEvent(ev);
		//}

	}
}