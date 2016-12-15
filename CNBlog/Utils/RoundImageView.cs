using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;

namespace CNBlog.Droid.Utils
{
    public class RoundImageView : ImageView
	{
		private int mBorderWidth = 10;

		private Bitmap mask;
		private Paint paint;
		private Color mBorderColor = Color.ParseColor("#FFFFFF");
		private Context mContext;

		public RoundImageView(Context context) : base(context)
		{
			mContext = context;
		}

		public RoundImageView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			mContext = context;
			GetAttributes(context, attrs);
		}

		public RoundImageView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			mContext = context;
			GetAttributes(context, attrs);
		}

		/// <summary>
		/// 获取自定义属性
		/// </summary>
		/// <param name="context"></param>
		/// <param name="attrs"></param>
		private void GetAttributes(Context context, IAttributeSet attrs)
		{
			TypedArray t_attrs = context.ObtainStyledAttributes(attrs, Resource.Styleable.RoundImageView);
			mBorderColor = t_attrs.GetColor(Resource.Styleable.RoundImageView_border_color, mBorderColor);
			int defalut = (int)(2 * context.Resources.DisplayMetrics.Density + 0.5f);
			mBorderWidth = t_attrs.GetDimensionPixelOffset(Resource.Styleable.RoundImageView_border_width, defalut);
			t_attrs.Recycle();
		}

		protected override void OnDraw(Canvas canvas)
		{
			Drawable localDrawable = Drawable;
			if (localDrawable == null)
				return;
			if (localDrawable is NinePatchDrawable)
				return;

			if (this.paint == null)
			{
				PorterDuff.Mode localMode = PorterDuff.Mode.DstIn;

				Paint localPaint = new Paint();
				localPaint.FilterBitmap = false;
				localPaint.AntiAlias = true;
				localPaint.SetXfermode(new PorterDuffXfermode(localMode));
				this.paint = localPaint;
			}

			int width = Width;
			int height = Height;
			/** 保存layer */
			int layer = canvas.SaveLayer(0.0F, 0.0F, width, height, null, SaveFlags.All);
			/** 设置drawable的大小 */
			localDrawable.SetBounds(0, 0, width, height);
			/** 将drawable绑定到bitmap(this.mask)上面（drawable只能通过bitmap显示出来） */
			localDrawable.Draw(canvas);
			if ((this.mask == null) || (this.mask.IsRecycled))
			{
				this.mask = CreateOvalBitmap(width, height);
			}
			/** 将bitmap画到canvas上面 */
			canvas.DrawBitmap(this.mask, 0.0F, 0.0F, this.paint);
			/** 将画布复制到layer上 */
			canvas.RestoreToCount(layer);
			DrawBorder(canvas, width, height);
		}

		private void DrawBorder(Canvas canvas, int width, int height)
		{
			if (mBorderWidth == 0)
				return;

			Paint mBorderPaint = new Paint();
			mBorderPaint.SetStyle(Paint.Style.Stroke);
			mBorderPaint.AntiAlias = true;
			mBorderPaint.Color = mBorderColor;
			mBorderPaint.StrokeWidth = mBorderWidth;
			/** 
             * 坐标x：view宽度的一般 坐标y：view高度的一般 半径r：因为是view的宽度-border的一半 
             */
			canvas.DrawCircle(width >> 1, height >> 1, (width - mBorderWidth) >> 1, mBorderPaint);
			canvas = null;
		}

		/// <summary>
		/// 获取一个bitmap，目的是用来承载drawable; 
		/// 将这个bitmap放在canvas上面承载，并在其上面画一个椭圆(其实也是一个圆，因为width=height)来固定显示区域 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public Bitmap CreateOvalBitmap(int width, int height)
		{
			Bitmap.Config localConfig = Bitmap.Config.Argb8888;
			Bitmap localBitmap = Bitmap.CreateBitmap(width, height, localConfig);
			Canvas localCanvas = new Canvas(localBitmap);
			Paint localPaint = new Paint();
			int padding = mBorderWidth - 3;
			/** 
             * 设置椭圆的大小(因为椭圆的最外边会和border的最外边重合的，如果图片最外边的颜色很深，有看出有棱边的效果，所以为了让体验更加好， 
             * 让其缩进padding px) 
             */
			RectF localRectF = new RectF(padding, padding, width - padding, height - padding);
			localCanvas.DrawOval(localRectF, localPaint);
			return localBitmap;
		}

	}
}

