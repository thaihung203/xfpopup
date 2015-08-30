using MVV2.WP.Services.Popup;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;
using XFPopup;

[assembly: Xamarin.Forms.Dependency(typeof(WPXFPopupSrvc))]
namespace MVV2.WP.Services.Popup
{
    class WPXFPopupSrvc : IXFPopupSrvc
    {

        private static PropertyInfo _isplatformenabledprop;
        public static PropertyInfo IsPlatformEnabledProperty
        {
            get
            {
                return _isplatformenabledprop ?? (
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

        public static System.Windows.Size DisplayResolution
        {
            get
            {
                if (Environment.OSVersion.Version.Major < 8)
                    return new System.Windows.Size(480, 800);
                int scaleFactor = (int)GetProperty(System.Windows.Application.Current.Host.Content, "ScaleFactor");
                switch (scaleFactor)
                {
                    case 100:
                        return new System.Windows.Size(480, 800);
                    case 150:
                        return new System.Windows.Size(720, 1280);
                    case 160:
                        return new System.Windows.Size(768, 1280);
                }
                return new System.Windows.Size(480, 800);
            }
        }

        private static object GetProperty(object instance, string name)
        {
            var getMethod = instance.GetType().GetProperty(name).GetGetMethod();
            return getMethod.Invoke(instance, null);
        }

        public WPXFPopupSrvc(){
            var size = DisplayResolution;
            XFPopupConst.SCREEN_WIDTH = (float)size.Width;
            XFPopupConst.SCREEN_HEIGHT = (float)size.Height;

        }

        public IVisualElementRenderer Convert(Xamarin.Forms.View source, Xamarin.Forms.VisualElement valid)
        {
            //only wp allow us to get renderer directly
            IVisualElementRenderer render = source.GetRenderer();
            if (render == null)
            {
                render = RendererFactory.GetRenderer(source);
                source.SetRenderer(render);
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


        public IXFPopupCtrl CreateDialog(Xamarin.Forms.Page page, Xamarin.Forms.View content, bool cancelable)
        {
            CustomDialog dlg = null;

            if (content != null)
            {

                var render = Convert(content, page);

                if (render != null)
                {

                    var size = content.GetSizeRequest(XFPopupConst.SCREEN_WIDTH, XFPopupConst.SCREEN_HEIGHT);

                    var width = (int)size.Request.Width;
                    var height = (int)size.Request.Height;


                    if (width > XFPopupConst.SCREEN_WIDTH)
                    {
                        width = (int)(XFPopupConst.SCREEN_WIDTH * 3 / 4);
                    }

                    if (height > (XFPopupConst.SCREEN_HEIGHT * 3 / 4))
                    {
                        height = (int)(XFPopupConst.SCREEN_HEIGHT * 3 / 4);
                    }

                    content.WidthRequest = width;
                    content.HeightRequest = height;

                    //important
                    content.Layout(new Rectangle(0, 0, width, height));

                    dlg = new CustomDialog(content, cancelable);

                }



            }
            return dlg;

        }



        public IXFPopupCtrl CreateLoading(string loading = "Loading...")
        {
            return new CustomLoading(loading);
        }

        public IXFPopupCtrl CreateDropDown(View anchor, View drop)
        {
            CustomDropDown dlg = null;

            var render = Convert(drop, anchor);

            if (render != null)
            {
                var nanchor = anchor.GetRenderer() as Canvas;
                drop.WidthRequest = nanchor.ActualWidth;

                if (drop.HeightRequest <= 0)
                {
                    var size = drop.GetSizeRequest(nanchor.ActualWidth, XFPopupConst.SCREEN_HEIGHT / 2);
                    if (size.Request.Height > XFPopupConst.SCREEN_HEIGHT / 2)
                    {
                        drop.HeightRequest = XFPopupConst.SCREEN_HEIGHT / 2;
                    }
                    else
                    {
                        drop.HeightRequest = size.Request.Height;
                    }
                }

                drop.Layout(new Rectangle(0, 0, drop.WidthRequest, drop.HeightRequest));

                dlg = new CustomDropDown(nanchor, render as Canvas, drop.HeightRequest);
            }



            
            return dlg;
        }

        public void ShowTopNoti(Xamarin.Forms.Page p, View noti, int msTTL = 1500)
        {

            var render = Convert(noti, p);

            if (render != null)
            {
                var nanchor = p.GetRenderer() as Canvas;
                p.WidthRequest = nanchor.ActualWidth - 10;

                if (noti.HeightRequest <= 0)
                {
                    var size = noti.GetSizeRequest(nanchor.ActualWidth - 10, XFPopupConst.SCREEN_HEIGHT / 2);
                    if (size.Request.Height > XFPopupConst.SCREEN_HEIGHT / 2)
                    {
                        noti.HeightRequest = XFPopupConst.SCREEN_HEIGHT / 2;
                    }
                    else
                    {
                        noti.HeightRequest = size.Request.Height;
                    }
                }

                noti.Layout(new Rectangle(0, 0, noti.WidthRequest, noti.HeightRequest));

                var nativePopup = new System.Windows.Controls.Primitives.Popup();
                nativePopup.VerticalOffset = 0;
                nativePopup.HorizontalOffset = 0;
                
                var boder = new Border();
                boder.BorderThickness = new System.Windows.Thickness(1);
                boder.Padding = new System.Windows.Thickness(5);
                boder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 50, 50));
                boder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));

                boder.VerticalAlignment = VerticalAlignment.Top;
                boder.HorizontalAlignment = HorizontalAlignment.Stretch;

                boder.Width = noti.WidthRequest + 10;
                boder.Height = noti.HeightRequest + 10;
                boder.CornerRadius = new CornerRadius(5);

                var elm = (render as Panel);
                elm.VerticalAlignment = VerticalAlignment.Top;
                elm.HorizontalAlignment = HorizontalAlignment.Left;
                boder.Child = elm;

                nativePopup.Child = boder;
                nativePopup.IsOpen = true;

                //
                byte count = 0;
                var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += (object sender, EventArgs e) => {
                    count++;
                    if (count >= 10) {
                        dispatcherTimer.Stop();
                        nativePopup.IsOpen = false;

                    }

                    boder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(255 - count * 25), 255, 255, 255));
                };
                 
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0,msTTL/10);
                dispatcherTimer.Start();
            }
        }
    }

    class CustomDropDown : IXFPopupCtrl
    {
        WeakReference<Canvas> wAnchor;
        private System.Windows.Controls.Primitives.Popup nativePopup;
        private Border nativeBorder;
        private double dropHeight;

        public CustomDropDown(Canvas anchor, Canvas drop, double _dropHeight)
        {
            wAnchor = new WeakReference<Canvas>(anchor);
            nativePopup = new System.Windows.Controls.Primitives.Popup();
            dropHeight = _dropHeight;

            nativePopup.VerticalOffset = 0;
            nativePopup.HorizontalOffset = 0;

            drop.HorizontalAlignment = HorizontalAlignment.Left;
            drop.VerticalAlignment = VerticalAlignment.Top;


            nativeBorder = new Border();
            nativeBorder.BorderThickness = new System.Windows.Thickness(1);
            nativeBorder.Padding = new System.Windows.Thickness(5);
            nativeBorder.CornerRadius = new CornerRadius(3);
            nativeBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 50, 50));
            nativeBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            nativeBorder.VerticalAlignment = VerticalAlignment.Center;
            nativeBorder.HorizontalAlignment = HorizontalAlignment.Center;

            nativeBorder.Child = drop;
            nativeBorder.InvalidateArrange();

            Canvas canvas = new Canvas();
            canvas.Width = XFPopupConst.SCREEN_WIDTH;
            canvas.Height = XFPopupConst.SCREEN_HEIGHT;
            canvas.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255, 255, 255));



            canvas.Tap += (c, a) => {
                Hide();
            };

            canvas.Children.Add(nativeBorder);


            nativePopup.Child = canvas;
        }
        public void Hide()
        {
            //do some position
            nativePopup.IsOpen = false;
        }

        public void Show()
        {
            var root = System.Windows.Application.Current.RootVisual;

            var tran = root.RenderTransform;
            Canvas anchor = null;
            if (wAnchor.TryGetTarget(out anchor))
            {
                var aboluteTransform = anchor.TransformToVisual(root);
                var offset = aboluteTransform.Transform(new System.Windows.Point(0, 0));

                nativeBorder.SetValue(Canvas.LeftProperty, offset.X);

                nativeBorder.Width = anchor.ActualWidth;

                if (offset.Y < (XFPopupConst.SCREEN_HEIGHT * 2 / 3 - anchor.ActualHeight))
                {
                    nativeBorder.SetValue(Canvas.TopProperty, offset.Y + anchor.ActualHeight + 2);
                }
                else
                {
                    //padding calculate
                    nativeBorder.SetValue(Canvas.TopProperty, (offset.Y - dropHeight - 15));
                }

                //todo do some position here
                nativePopup.IsOpen = true;


            }
        }
    }

    class CustomLoading : IXFPopupCtrl
    {
        private System.Windows.Controls.Primitives.Popup nativePopup;

        public CustomLoading(string loading = "loading")
        {
            nativePopup = new System.Windows.Controls.Primitives.Popup();
            nativePopup.VerticalOffset = 0;
            nativePopup.HorizontalOffset = 0;


            System.Windows.Controls.ProgressBar bar = new System.Windows.Controls.ProgressBar();
            bar.Width = XFPopupConst.SCREEN_WIDTH * 2 / 3;
            bar.IsIndeterminate = true;

            System.Windows.Controls.TextBlock txt = new TextBlock();
            txt.Text = loading;
            
            StackPanel stack = new StackPanel();
            stack.Children.Add(bar);
            stack.Children.Add(txt);


            System.Windows.Controls.Grid wrap = new System.Windows.Controls.Grid();
            //var ScreenWidth = System.Windows.Application.Current.Host.Content.ActualWidth;
            //var ScreenHeight = System.Windows.Application.Current.Host.Content.ActualHeight;
            wrap.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 50, 50, 50));
            wrap.Height = XFPopupConst.SCREEN_HEIGHT;
            wrap.Width = XFPopupConst.SCREEN_WIDTH;

            var border = new Border();
            border.BorderThickness = new System.Windows.Thickness(1);
            border.Padding = new System.Windows.Thickness(5);
            border.CornerRadius = new CornerRadius(3);
            border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 50, 50));
            border.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255, 255, 255));

            border.VerticalAlignment = VerticalAlignment.Center;
            border.HorizontalAlignment = HorizontalAlignment.Center;

            border.Child = stack;

            wrap.Children.Add(border);

            nativePopup.Child = wrap;
        }

        public void Hide()
        {
            nativePopup.IsOpen = false;
        }

        public void Show()
        {
            nativePopup.IsOpen = true;
        }
    }

    class CustomDialog : IXFPopupCtrl
    {
        private bool cancelable = false;

        private System.Windows.Controls.Primitives.Popup nativePopup;

        public CustomDialog(Xamarin.Forms.View content, bool _cancelable = false)
        {
            cancelable = _cancelable;
            nativePopup = new System.Windows.Controls.Primitives.Popup();
            nativePopup.VerticalOffset = 0;
            nativePopup.HorizontalOffset = 0;

            System.Windows.Controls.Grid wrap = new System.Windows.Controls.Grid();
            var ScreenWidth = System.Windows.Application.Current.Host.Content.ActualWidth;
            var ScreenHeight = System.Windows.Application.Current.Host.Content.ActualHeight;
            wrap.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 50, 50, 50));

            wrap.Height = XFPopupConst.SCREEN_HEIGHT;
            wrap.Width = XFPopupConst.SCREEN_WIDTH;

            wrap.MouseLeftButtonUp += (s, e) => {
                if (cancelable)
                {
                    nativePopup.IsOpen = false;
                }
            };

            var boder = new Border();
            boder.BorderThickness = new System.Windows.Thickness(1);
            boder.Padding = new System.Windows.Thickness(5);
            boder.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 50, 50));
            boder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));

            boder.VerticalAlignment = VerticalAlignment.Center;
            boder.HorizontalAlignment = HorizontalAlignment.Center;

            boder.Width = content.WidthRequest + 10;
            boder.Height = content.HeightRequest + 10;
            boder.CornerRadius = new CornerRadius(5);

            var elm = (content.GetRenderer() as Panel);
            elm.VerticalAlignment = VerticalAlignment.Top;
            elm.HorizontalAlignment = HorizontalAlignment.Left;
            boder.Child = elm;

            wrap.Children.Add(boder);
            nativePopup.Child = wrap;
            nativePopup.IsOpen = false;
        }


        public void Hide()
        {
            nativePopup.IsOpen = false;
        }

        public void Show()
        {
            nativePopup.IsOpen = true;
        }
    }

}
