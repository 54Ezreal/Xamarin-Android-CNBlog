using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using Android.App;

namespace CNBlog.Droid.Utils
{
    public class CommonAdapter<T> : BaseAdapter<T>
    {
        //要绑定的数据
        List<T> items;
        //页面上下文
        Activity context;
        //布局Id
        int layoutId;

        public delegate View GetViewEvent(int position, View convertView, ViewGroup parent, T item, ViewHolder viewHolder);

        /// <summary>
        /// 加载item的View事件
        /// </summary>
        public event GetViewEvent OnGetView;


        public CommonAdapter(Activity context, int layoutId, List<T> items)
            : base()
        {
            this.context = context;
            this.layoutId = layoutId;
            this.items = items;
        }

        public void RefreshItems(List<T> items)
        {
            this.items = items;
            NotifyDataSetChanged();
        }


		#region implemented abstract members of BaseAdapter

		public override long GetItemId(int position) => position;

		public override View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
        {
			var item = items[position];
			ViewHolder viewHolder = ViewHolder.Get(context, convertView, parent, layoutId, position);
			if (OnGetView != null)
				return OnGetView(position, convertView, parent, item, viewHolder);
            return convertView;  
        }

		public override int Count => items.Count;

		#endregion

		#region implemented abstract members of BaseAdapter

		public override T this[int position] => items[position];

		#endregion
	}
}

