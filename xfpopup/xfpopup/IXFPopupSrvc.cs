using System;
using Xamarin.Forms;

namespace XFPopup
{
	public interface IXFPopupSrvc
	{

		IXFPopupCtrl CreateDialog(Page p, View v,bool cancelable);
		IXFPopupCtrl CreateLoading(String loading = "Loading...");
		IXFPopupCtrl CreateDropDown(View anchor, View drop);
		void ShowTopNoti(Page p,View noti,int msTTL = 1500);

	}
}
