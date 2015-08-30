using System;
using Xamarin.Forms;
using System.Reflection;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using CoreGraphics;
using xfpopup.iOS;
using System.Drawing;
using XFPopup;

[assembly: Dependency(typeof(IosXFPopupSrvc))]
namespace xfpopup.iOS
{
	public class IosXFPopupSrvc : IXFPopupSrvc
    {
		private static nfloat padding = 2.0f;

		private static Type _platformType = Type.GetType("Xamarin.Forms.Platform.iOS.Platform, Xamarin.Forms.Platform.iOS", true);
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


		public IosXFPopupSrvc ()
		{
			CGRect screen = UIScreen.MainScreen.Bounds;
            XFPopupConst.SCREEN_WIDTH  = (float)screen.Width;
            XFPopupConst.SCREEN_HEIGHT = (float)screen.Height;
		}

        public IVisualElementRenderer Convert(Xamarin.Forms.View source, Xamarin.Forms.VisualElement valid) {
            IVisualElementRenderer render = (IVisualElementRenderer) source.GetValue(RendererProperty);
            if (render == null)
            {
                render = RendererFactory.GetRenderer(source);
                source.SetValue(RendererProperty, render);
                if (valid != null) {
                    var p = PlatformProperty.GetValue(valid);
                    if (p != null) {
                        PlatformProperty.SetValue(source, p);
                        IsPlatformEnabledProperty.SetValue(source, true);
                    }
                }
            }
            
            return render;
        }


		public IXFPopupCtrl CreateDialog(Page page, Xamarin.Forms.View content,bool cancelable)
		{

			CustomPopup dlg = null;

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
					content.Layout(new Xamarin.Forms.Rectangle(0, 0, width, height));

					var native = render as UIKit.UIView;

					native.Frame = new CoreGraphics.CGRect (padding, padding, width, height);

					dlg = new CustomPopup(native,cancelable,width + 2*padding,height + 2*padding,ShowType.Dialog);

				}

			}
			return dlg;

		}


		public IXFPopupCtrl CreateLoading(string loading = "Loading...")
		{
			var vwMain = new UIView();

			//get the center
			vwMain.BackgroundColor = UIColor.White;
			vwMain.AutoresizingMask = UIViewAutoresizing.All;

			var activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
			vwMain.Frame = new CGRect (0, 0, XFPopupConst.SCREEN_WIDTH * 0.75f, activitySpinner.Frame.Height*2);

			activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
			activitySpinner.Center = new CGPoint (activitySpinner.Frame.Width, vwMain.Frame.Height/2);
				
			vwMain.AddSubview (activitySpinner);
			activitySpinner.StartAnimating ();

			// create and configure the "Loading Data" label
			var loadingLabel = new UILabel(new CGRect (
				activitySpinner.Frame.Width * 2,
				0 ,
				(XFPopupConst.SCREEN_WIDTH * 3/4 - activitySpinner.Frame.Width * 2),
				activitySpinner.Frame.Height * 2
			));

			loadingLabel.TextColor = UIColor.Black;
			loadingLabel.Text = loading;
			loadingLabel.TextAlignment = UITextAlignment.Center;
			loadingLabel.AutoresizingMask = UIViewAutoresizing.All;
			vwMain.Frame = new CGRect(padding, padding, XFPopupConst.SCREEN_WIDTH * 0.75f, activitySpinner.Frame.Width * 2);
			vwMain.AddSubview (loadingLabel);

			CustomPopup dlg = new CustomPopup(vwMain,false,XFPopupConst.SCREEN_WIDTH*0.75f + 2*padding,activitySpinner.Frame.Width * 2 + 2*padding,ShowType.Dialog);
			return dlg;
		}


		public IXFPopupCtrl CreateDropDown(Xamarin.Forms.View anchor, Xamarin.Forms.View drop)
		{
			CustomPopup dlg = null;
			//first try to get the PopupHolderRenderer
			//first try to get the PopupHolderRenderer
			if(anchor == null || drop == null){
				return null;
			}

			var anchorRender = anchor.GetValue(RendererProperty) as UIView;
			if (anchorRender == null) {
				return null;
			}

            var render = Convert(drop, anchor);

			if (render == null) {
				return null;
			}

			if (render != null) {

				var size = drop.GetSizeRequest(anchorRender.Bounds.Width, XFPopupConst.SCREEN_HEIGHT);
				var width = anchorRender.Bounds.Width;
				var height = (int)size.Request.Height;
				if (height > (XFPopupConst.SCREEN_HEIGHT * 3/ 4))
				{
					height = (int)(XFPopupConst.SCREEN_HEIGHT * 3/ 4);
				}

				//important
				drop.Layout(new Xamarin.Forms.Rectangle(0, 0, width - 2*padding, height));

				var native = render as UIKit.UIView;
				native.Frame = new CoreGraphics.CGRect (padding, padding, width - 2*padding, height);

				dlg = new CustomPopup(native, true, width, height + 2*padding, ShowType.DropDown,anchorRender);

			}
			return dlg;
		}

		public void ShowTopNoti(Xamarin.Forms.Page page, Xamarin.Forms.View noti, int ttl = 1500){
			
			if (noti != null && page != null) {

                var render = Convert(noti, page);


				if (render != null) {
					nfloat width = XFPopupConst.SCREEN_WIDTH;
					var size = noti.GetSizeRequest(XFPopupConst.SCREEN_WIDTH - 4, XFPopupConst.SCREEN_HEIGHT * 0.2f);
					nfloat height = (int)size.Request.Height;

					if (height > (XFPopupConst.SCREEN_HEIGHT * 0.2f))
					{
						height = (int)(XFPopupConst.SCREEN_HEIGHT * 0.2f);
					}

					//important
					noti.Layout(new Xamarin.Forms.Rectangle(0, 0, width, height));

					var win = new UIWindow(new CGRect(0,0,width,height));
					win.WindowLevel = UIKit.UIWindowLevel.Alert;
					var native = render as UIKit.UIView;
					win.AddSubview (native);
					win.Alpha = 0;
					win.MakeKeyAndVisible ();

					native.Frame = new CGRect(1, 0 , width - 2, 0) ;

					win.Alpha = 1;
					UIView.Animate (
						duration: 1f,
						delay: 0f,
						options: UIViewAnimationOptions.TransitionNone,
						animation: () => { 
							native.Frame = new CoreGraphics.CGRect (1, 20, width - 2, height - 2);
						},
						completion: () => {
							UIView.Animate(
								duration: ttl/1000,
								delay: 0f, 
								options: UIViewAnimationOptions.CurveEaseInOut,
								animation: () => { win.Alpha = 0; },
								completion: () => { 
									native.RemoveFromSuperview();
									native = null;
									win.RemoveFromSuperview();
									win = null;
								}
							);
						}
					);




				}

			}
		}

	}


	public interface INotifyCloseDone {
		void OnChildClosed();
	}

	public enum ShowType{
		Dialog,
		DropDown
	}

	public class CustomWindow : UIWindow,IXFPopupCtrl {
		private UIView wrap;
		private UIView anchor;
		private UIView content;
		private bool cancelable;
		private ShowType showType;
		private bool topdown;

		public INotifyCloseDone CustomParent {get;set;}



		public CustomWindow(INotifyCloseDone _parent,  UIView _content,bool _cancelable, nfloat _width, nfloat _height, ShowType _showType, UIView _anchor = null) : base(UIScreen.MainScreen.Bounds){
			
			nfloat left = 0; 
		 	nfloat top = 0;


			cancelable = _cancelable;
			content = _content;
			anchor = _anchor;
			showType = _showType;
			CustomParent = _parent;
			nfloat width = _width + 4;
			nfloat height = _height + 4;

			UIColor bgrdColor = UIColor.FromRGBA (100, 100, 100, 100);

			//todo show at top bar notification
			switch(showType)
			{


				case ShowType.Dialog:
					left =  (XFPopupConst.SCREEN_WIDTH  - width) * 0.5f;
					top =  (XFPopupConst.SCREEN_HEIGHT - height) * 0.5f;
					bgrdColor = UIColor.FromRGBA (100, 100, 100, 100);

					break;

				case ShowType.DropDown:
					CGPoint topleft = ConvertPointFromView (new CGPoint (0, 0), anchor);
					//ConvertPointToWindow(new CGPoint(anchor.Frame.Left , anchor.Frame.Top),this);

					left =  topleft.X;
					if (topleft.Y + anchor.Frame.Height + height + 2 < this.Bounds.Height) {
						top = topleft.Y + anchor.Frame.Height + 2;
						topdown = true;
					} else {
						top = topleft.Y - height - 2; 
						topdown = false;
					}
					bgrdColor = UIColor.FromRGBA (0, 0, 0, 0);

					break;
				default:
					break;
			}

			wrap = new UIView (content.Bounds);
			wrap.BackgroundColor = UIColor.White;
			wrap.Layer.CornerRadius = 6f;
			wrap.Layer.ShadowOffset = SizeF.Empty;
			wrap.Layer.ShadowRadius = 3f;
			wrap.Layer.ShadowOpacity = 0.5f;
			wrap.Frame = new CGRect(left,top, _width,_height);
			wrap.AddSubview (content);

			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			Opaque = false;

			BackgroundColor = bgrdColor;

			WindowLevel = UIKit.UIWindowLevel.Normal;
			AddSubview (wrap);

		}


		public override void SendEvent (UIEvent evt)
		{
			base.SendEvent (evt);
			if (!cancelable) {
				return;
			} else {
			
				var allTouches = evt.AllTouches;
				if (allTouches.Count > 0) {
					UITouch touch = ((UITouch)allTouches.AnyObject); 
					var phase = touch.Phase;
					if (phase == UITouchPhase.Ended) {
						var point = touch.LocationInView (this);
						Console.WriteLine ("X " + point.X + " Y " + point.Y);
						if(wrap != null && !wrap.Frame.Contains (point))
						{
							Console.WriteLine ("X " + point.X + " Y " + point.Y + " CANCEL") ;
							this.Hide ();	
						}
					}		
				}
			}
		}

		public void Show(){
			MakeKeyAndVisible ();
			if (showType == ShowType.Dialog) {
				Alpha = 0f;

				UIView.Animate(
						duration: 0.3f,
						delay: 0f, options: UIViewAnimationOptions.CurveEaseInOut,
						animation: () => { Alpha = 1; },
						completion: () => { });
					
			}

			if (showType == ShowType.DropDown) {
				
				var orgnRect = this.wrap.Frame;  
				if (topdown) {
					var rect = new RectangleF ((float)orgnRect.Location.X, (float)orgnRect.Location.Y, (float)orgnRect.Size.Width, 0);
					this.wrap.Frame = rect;
					Alpha = 0;
					UIView.Animate (
						duration: 0.3f,
						delay: 0f,
						options: UIViewAnimationOptions.TransitionNone,
						animation: () => { 
							Alpha = 1;
							wrap.Frame = orgnRect; 
						},
						completion: () => {
						});

				} else {
					var rect = new RectangleF ((float)orgnRect.Location.X, (float) (orgnRect.Location.Y + orgnRect.Height), (float)orgnRect.Size.Width, 0);
					this.wrap.Frame = rect;
					Alpha = 0;
					UIView.Animate (
						duration: 0.3f,
						delay: 0f,
						options: UIViewAnimationOptions.TransitionNone,
						animation: () => { 
							Alpha = 1;
							wrap.Frame = orgnRect; 
						},
						completion: () => {
						});
				}
			
			}
		}

		public void Hide(){
			UIView.Animate(
				duration: 0.3f,
				delay: 0f,
				options: UIViewAnimationOptions.CurveEaseInOut,
				animation: () => { Alpha = 0; },
				completion: () => {
					if(content != null){
						content.RemoveFromSuperview ();
					}

					if(wrap != null){
						wrap.RemoveFromSuperview();
						wrap = null;
					}

					RemoveFromSuperview();
					CustomParent.OnChildClosed();
				});
		}

	}

	public class CustomPopup : IXFPopupCtrl,INotifyCloseDone{
		private CustomWindow window;
		private UIView content;
		private UIView anchor;
		private bool cancelable;
		private nfloat width;
		private nfloat height;
		private ShowType showType;

		public CustomPopup( UIView _content,bool _cancelable, nfloat _width, nfloat _height, ShowType _showType, UIView _anchor = null){
			cancelable = _cancelable;
			content = _content;
			width = _width;
			height = _height;
			anchor = _anchor;
			showType = _showType;
			window = new CustomWindow (this, content, cancelable, width, height, showType, anchor);
		}

		public void Show(){
			
			if (window == null) {
				window = new CustomWindow (this,  content, cancelable, width, height, showType, anchor);
			}

			window.Show ();
		}

		public void OnChildClosed(){
			window.CustomParent = null;
			window = null;
		}


		public void Hide(){
			window.Hide();
		}

		 
	}




}

