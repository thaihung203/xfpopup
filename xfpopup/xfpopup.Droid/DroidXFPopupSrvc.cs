using System;

using Android.App;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using System.Reflection;
using Android.Graphics.Drawables;
using xfpopup.Droid;
using XFPopup;
using Android.Content;

[assembly: Dependency(typeof(DroidXFPopupSrvc))]
namespace xfpopup.Droid
{
    public class DroidXFPopupSrvc : IXFPopupSrvc
    {
        private static Type _platformType = Type.GetType("Xamarin.Forms.Platform.Android.Platform, Xamarin.Forms.Platform.Android", true);
       
        private static BindableProperty _rendererProperty;
        public static BindableProperty RendererProperty
        {
            get { return _rendererProperty ?? (_rendererProperty = (BindableProperty)_platformType.GetField("RendererProperty", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).GetValue(null)); }
        }

        private static PropertyInfo _isplatformenabledprop;
        public static PropertyInfo IsPlatformEnabledProperty {
            get { return _isplatformenabledprop ?? (
                _isplatformenabledprop = typeof(VisualElement).GetProperty("IsPlatformEnabled", BindingFlags.NonPublic | BindingFlags.Instance));
            }
        }

        private static PropertyInfo _platform;
        public static PropertyInfo PlatformProperty
        {
            get
            {
                return _platform ?? (
              _platform = typeof(VisualElement).GetProperty("Platform", BindingFlags.NonPublic | BindingFlags.Instance));
            }
        }

        public DroidXFPopupSrvc() {
            var metrics = Forms.Context.Resources.DisplayMetrics;
            //convert to xamarin unit
            XFPopupConst.SCREEN_WIDTH = metrics.WidthPixels / metrics.Density;
            XFPopupConst.SCREEN_HEIGHT = metrics.HeightPixels/metrics.Density;
        }



        public IVisualElementRenderer Convert(Xamarin.Forms.View source, Xamarin.Forms.VisualElement valid)
        {
            IVisualElementRenderer render = (IVisualElementRenderer)source.GetValue(RendererProperty);
            if (render == null)
            {
                render = RendererFactory.GetRenderer(source);
                source.SetValue(RendererProperty, render);
                if (valid != null)
                {
                    var p = PlatformProperty.GetValue(valid);
                    if (p != null)
                    {
                        PlatformProperty.SetValue(source, p);
                        IsPlatformEnabledProperty.SetValue(source, true);
                    }
                }
            }

            return render;
        }



        public IXFPopupCtrl CreateDialog(Page page, Xamarin.Forms.View content,bool cancelable)
        {
            CustomDialog dlg = null;
            //first try to get the PopupHolderRenderer

            if (content != null) {


                var render = Convert(content, page);


                if (render != null) {

                    var size = content.GetSizeRequest(XFPopupConst.SCREEN_WIDTH, XFPopupConst.SCREEN_HEIGHT);

                    var width = (int)size.Request.Width;
                    var height = (int)size.Request.Height;
                    

                    if (width > XFPopupConst.SCREEN_WIDTH)
                    {
                        width = (int)(XFPopupConst.SCREEN_WIDTH * 3 / 4);
                    }

                    if (height > (XFPopupConst.SCREEN_HEIGHT * 3/ 4))
                    {
                        height = (int)(XFPopupConst.SCREEN_HEIGHT * 3/ 4);
                    }

                    //important
                    content.Layout(new Rectangle(0, 0, width, height));

                    
                    
                    float density = Forms.Context.Resources.DisplayMetrics.Density;
                    width = (int)(width * density);
                    height = (int)(height * density);


                    var native = render as Android.Views.View;
                    native.LayoutParameters = new ViewGroup.LayoutParams(width, height);
                    
                    LinearLayout ll = new LinearLayout(Forms.Context);
                    ll.SetBackgroundResource(Resource.Layout.rounded_rectangle_bgr);
                    ll.LayoutParameters = new ViewGroup.LayoutParams(width + 4, height + 4);
                    ll.AddView(native);
                    
                    dlg = new CustomDialog(ll,cancelable);
                    
                }
                
                

            }
            return dlg;
            
        }

        public IXFPopupCtrl CreateLoading(string loading = "Loading...")
        {
            return new CustomLoading(loading);
        }

        public IXFPopupCtrl CreateDropDown(Xamarin.Forms.View anchor, Xamarin.Forms.View drop)
        {
            CustomDropDown dropctr = null;
            //get the renderer of anchor
            if (anchor != null) {
                var ar = anchor.GetValue(RendererProperty);
                if (ar != null) {
                    var dropView = Convert(drop, anchor);

                    if (dropView == null) {
                        return null;
                    }
                    
                    double w = (int)anchor.Width;
                    double h = XFPopupConst.SCREEN_HEIGHT/2;
                    drop.WidthRequest = w;
                    var size = drop.GetSizeRequest(w, XFPopupConst.SCREEN_HEIGHT / 2);
                    if (size.Request.Height < h) {
                        h = size.Request.Height;
                    }


                    drop.Layout(new Rectangle(0, 0, w, h));

                    float density = Forms.Context.Resources.DisplayMetrics.Density;
                    w = w * density;
                    h = h * density;


                    var native = dropView as Android.Views.View;
                    native.LayoutParameters = new ViewGroup.LayoutParams((int)w, (int)h);
                    
                    dropctr = new CustomDropDown(ar as Android.Views.View, native, (int)w + 4, (int)h+10);
                    
                }

            }

            return dropctr;

        }

        public void ShowTopNoti(Page p, Xamarin.Forms.View noti, int msTTL = 1500)
        {
            var dropView = Convert(noti, p);

            if (dropView == null) {
                return;
            }

            //we using toast
            Toast toast = new Toast(Forms.Context);
            toast.SetGravity(GravityFlags.Top | GravityFlags.Left | GravityFlags.FillHorizontal, 0, 0);
            toast.Duration = ToastLength.Short;
            toast.View = dropView as Android.Views.View;
            toast.Show();

        }
    }

    class CustomDropDown : PopupWindow, IXFPopupCtrl {
        private Android.Views.View anchorView;

        class TouchListerner : Android.Views.View.IOnTouchListener
        {
            public IntPtr Handle
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool OnTouch(Android.Views.View v, MotionEvent e)
            {
                throw new NotImplementedException();
            }
        }

        public CustomDropDown(Android.Views.View anchor, Android.Views.View content, int width, int height):base(width,height) {

            anchorView = anchor;
            Drawable shape = Forms.Context.Resources.GetDrawable(Resource.Layout.rounded_rectangle_bgr);
            this.SetBackgroundDrawable(shape);
            this.ContentView = content;
            OutsideTouchable = true;
            this.Focusable = true;
            //TouchIntercepted += CustomDropDown_TouchIntercepted;
            
        }

        private void CustomDropDown_TouchIntercepted(object sender, Android.Views.View.TouchEventArgs e)
        {
            //if(e.Event.)
            throw new NotImplementedException();
        }

        
        public void Hide()
        {
            this.Dismiss();
        }

        public void Show()
        {
            this.ShowAsDropDown(anchorView,0,2);
        }
    }

    class CustomDialog : Dialog, IXFPopupCtrl
    {
        public CustomDialog(Android.Views.View v, bool cancelable = false) : base(Forms.Context)
        {
            this.RequestWindowFeature((int)WindowFeatures.NoTitle);
            this.SetCancelable(cancelable);
            this.SetContentView(v);
        }
    }


    //todo convert to toast
    class CustomLoading : ProgressDialog, IXFPopupCtrl {
        public CustomLoading(String loading):base(Forms.Context){
            this.SetTitle("");
            this.SetMessage(loading);
        }
    }
    
}