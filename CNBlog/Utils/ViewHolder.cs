using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace CNBlog.Droid.Utils
{
    public class ViewHolder : Java.Lang.Object
    {
        private SparseArray<View> views;
        int position;
        View ConvertView;

        public ViewHolder(Activity context,ViewGroup parent,int layoutID,int postion)
        {
            this.position = postion;
            this.views = new SparseArray<View>();
            ConvertView = context.LayoutInflater.Inflate(layoutID, null);
			ConvertView.Tag = this;
        }

        //单例模式获取对象实例  
        public static ViewHolder Get(Activity context, View convertView, ViewGroup parent, int layoutID, int position)  
        {  
            if (convertView == null)  
                return new ViewHolder(context, parent, layoutID, position);  
            else  
            {  
                ViewHolder holder = (ViewHolder)convertView.Tag;  
                holder.position = position;  
                return holder;  
            }  

        } 

        public View GetConvertView()  
        {  
            return ConvertView;  
        }  


        /// <summary>  
        /// 通过ViewID获取控件  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="viewId"></param>  
        /// <returns></returns>  
        public T GetView<T>(int viewId) where T : View  
        {  
            View view = views.Get(viewId);  
            if (view == null)  
            {  
                view = ConvertView.FindViewById<T>(viewId);  
                views.Put(viewId, view);  
            }  
            return (T)view;  
        }

		public ViewHolder SetOnClickListener<T>(int viewId, T widget, View.IOnClickListener listener)
			where T:View
		{
			View view = GetView<T>(viewId);
			view.SetOnClickListener(listener);
			return this;
		}

    }
}