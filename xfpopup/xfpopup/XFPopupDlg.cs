using System;
using Xamarin.Forms;

namespace XFPopup
{
	public delegate void XFPopupDlgResultHandler(XFPopupDlg sender, bool result);


	public class XFPopupDlg : IXFPopupCtrl
	{
		public event XFPopupDlgResultHandler OnResult;

		IXFPopupCtrl dlgNative = null;

		public  View GenDialog(Xamarin.Forms.View _content,  bool _cancelable, String _title, bool _rightClose, String _ok, String _ko) {
			if (_content.WidthRequest <= 0) {
				_content.WidthRequest = 200;
			}

			//build a grid here;
			var grid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition {Height = new GridLength(XFPopupConst.CONTROL_HEIGHT, GridUnitType.Absolute)},
					new RowDefinition {Height = GridLength.Auto},
					new RowDefinition {Height = new GridLength(XFPopupConst.CONTROL_HEIGHT, GridUnitType.Absolute)},
				},

				ColumnDefinitions =
				{
					new ColumnDefinition{Width = new GridLength(XFPopupConst.CONTROL_HEIGHT, GridUnitType.Absolute)},
					new ColumnDefinition{Width = new GridLength((_content.WidthRequest - XFPopupConst.CONTROL_HEIGHT)/2, GridUnitType.Absolute)},
					new ColumnDefinition{Width = new GridLength((_content.WidthRequest - XFPopupConst.CONTROL_HEIGHT)/2, GridUnitType.Absolute)},
					new ColumnDefinition{Width = new GridLength(XFPopupConst.CONTROL_HEIGHT + 2, GridUnitType.Absolute)}
				}
				};

			//header
			var box = new BoxView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.Aqua
			};

			grid.Children.Add(box, 0, 4, 0, 1);


			var logo = new Image
			{
				Source = ImageSource.FromResource("xfpopup.Images.monkey.png"),
				HeightRequest = XFPopupConst.CONTROL_HEIGHT - 2,
				WidthRequest = XFPopupConst.CONTROL_HEIGHT - 2,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center

			};

			grid.Children.Add(logo, 0, 1, 0, 1);

			var title = new Label
			{
				Text = _title,
				YAlign = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};

			grid.Children.Add(title, 1, 3, 0, 1);


			var imgClose = new Image
			{
				HeightRequest = XFPopupConst.CONTROL_HEIGHT - 5,
				WidthRequest = XFPopupConst.CONTROL_HEIGHT - 5,
				Source = ImageSource.FromResource("xfpopup.Images.close.png"),
				Aspect = Aspect.AspectFit,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center

			};

			var tapClose = new TapGestureRecognizer();
			tapClose.Tapped += (object sender, EventArgs e) => {
				if (dlgNative != null) { dlgNative.Hide(); }

			};

			imgClose.GestureRecognizers.Add(tapClose);

			grid.Children.Add(imgClose, 3, 4, 0, 1);

			grid.Children.Add(_content, 0, 4, 1, 2);

			if (_ok != null)
			{
				var ok = new Button
				{
					Text = _ok,
					BackgroundColor = Color.Red
				};

				ok.Clicked += (s, e) => {
					if (OnResult != null) {
						OnResult(this, true);
					}
				};

				if (_ko != null)
				{
					grid.Children.Add(ok, 2, 4, 2, 3);
				}
				else
				{
					grid.Children.Add(ok, 0, 4, 2, 3);
				}

			}

			if (_ko != null)
			{
				var ko = new Button
				{
					Text = _ko,
					BackgroundColor = Color.Blue
				};

				ko.Clicked += (s, e) =>
				{
					if (OnResult != null) {
						OnResult(this, false);
					}
				};

				grid.Children.Add(ko, 0, 2, 2, 3);
			}

			return grid;

		}
		public XFPopupDlg(Page _parent, Xamarin.Forms.View _content,  bool _cancelable, String _title, bool _rightClose, String _ok, String _ko) {
			var g = GenDialog (_content, _cancelable, _title, _rightClose, _ok, _ko);

			var svr = DependencyService.Get<IXFPopupSrvc> ();
			dlgNative = svr.CreateDialog(_parent, g, _cancelable);

		}

		public void Hide()
		{
			if (dlgNative != null) {
				dlgNative.Hide();
			}
		}

		public void Show()
		{
			if (dlgNative != null) {
				dlgNative.Show();
			}
		}
	}
}


