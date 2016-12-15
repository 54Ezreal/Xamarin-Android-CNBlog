using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Support.V4.App;
using Android.Text;
using CNBlog.Droid.Activities;

namespace CNBlog.Droid
{
    public class SimpleFragmentPagerAdapter : FragmentPagerAdapter
    {
        int PAGE_COUNT = 4;
        private string [] tabTitles = new string[] { "首页","精华", "新闻", "知识库"};
        private Context context;
        public SimpleFragmentPagerAdapter(Android.Support.V4.App.FragmentManager fm, Context context)
            :base(fm)
        {
            this.context = context;
            
        }

        public override int Count
        {
            get
            {
                return PAGE_COUNT;
            }
        }

		public override Android.Support.V4.App.Fragment GetItem(int position)
		{
			switch (position)
			{
				case 0:
					return new IndexPageFragment();
				case 1:
					return new EssencePageFragement();
				case 2:
					return new NewsPageFragment();
				case 3:
					return new RecommendPageFragment();
				default:
					return new NewsPageFragment();
			}
		}

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            SpannableString sb = new SpannableString(tabTitles[position]);
            return sb;
        }

        public View GetTabView(int position)
        {
            View view = LayoutInflater.From(context).Inflate(Resource.Layout.tab_item, null);
            TextView tv = (TextView)view.FindViewById(Resource.Id.textView);
            tv.Text = tabTitles[position];
            //ImageView img = (ImageView)view.FindViewById(Resource.Id.imageView);
            //img.SetImageResource(Resource.Mipmap.Icon);
			return view;
        }

    }
}