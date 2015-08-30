using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XFPopup;

namespace xfpopup
{
    public class App : Application
    {
        Button btnTest;
        Button btnDrop;
        Button btnFloat;

        IXFPopupCtrl popCtrl;
        IXFPopupCtrl fltCtrl;

        IXFPopupSrvc svr = DependencyService.Get<IXFPopupSrvc>();
        Page page;

        async void StartTimer()
        {
            while (true)
            {
                await Task.Delay(5000);
                svr.ShowTopNoti(page, new StackLayout
                {
                    Padding = new Thickness(0),
                    Spacing = 0,
                    Children = {
                        new Label {
                            Text = "Becarefull Im a noti ;)",
                            BackgroundColor = Color.Blue

                        }
                    }
                });
            }
        }

        public App()
        {

            btnTest = new Button
            {
                Text = "TEST",
                BackgroundColor = Color.Green
            };


            btnDrop = new Button
            {
                Text = "Drop",
                BackgroundColor = Color.Aqua
            };

            

            btnDrop.Clicked += (object sender, EventArgs e) =>
            {

                if (popCtrl == null)
                {
                    

                    
                    popCtrl = svr.CreateDropDown(
                        btnDrop,
                        new StackLayout
                        {
                            Children = {
                                new Label {
                                    Text = "1-2-3-4-5-6-7-8-9-0"
                                },
                                new Label {
                                    Text = "qwerty"
                                },
                                btnTest
                            }
                        }
                    );
                }

                if (popCtrl != null)
                {
                    popCtrl.Show();
                }

            };

            var content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(5, 5, 5, 5),
                Children = {
                    new Label {
                        XAlign = TextAlignment.Center,
                        Text = "Welcome to Xamarin Forms!"
                    },
                    new Label {
                        XAlign = TextAlignment.Center,
                        Text = "Welcome to Xamarin Forms!"
                    },
                    new Label {
                        XAlign = TextAlignment.Center,
                        Text = "Welcome to Xamarin Forms!"
                    },
                    new Label {
                        XAlign = TextAlignment.Center,
                        Text = "Welcome to Xamarin Forms!"
                    },
                    new Label {
                        XAlign = TextAlignment.Center,
                        Text = "Welcome to Xamarin Forms!",
						//HeightRequest = 400
					},
                    btnDrop
                }
            };

            // The root page of your application
            page = new ContentPage
            {
                Content = content
            };


            btnTest.Clicked += async (object sender, EventArgs e) =>
            {
                //svr.ShowTopNoti(page, new Label { Text = "This is a notification" });

                var loading = svr.CreateLoading("Processing, please wait...");
                loading.Show();
                await Task.Delay(1000);
                loading.Hide();


                var anotherDropButton = new Button
                {
                    Text = "Drop in dialog"
                };

                IXFPopupCtrl anotherDropCtrl = null;

                anotherDropButton.Clicked += (b, c) =>
                {
                    if (anotherDropCtrl == null) {
                        anotherDropCtrl = svr.CreateDropDown(
                            anotherDropButton, 
                            new StackLayout
                            {
                                Children = {
                                    new Label {
                                        Text = "There is a popup in xlab library which use a relative layout to mimic the popup effect, but it's seem too slow for us (about 150~200ms for complex view). Frankly, I think Xamarin should spend more time for the layout mechanism, because of too many heavy work must invoke when we add/remove a view to/from a layout."
                                    }
                                }
                            }
                        );
                    }

                    anotherDropCtrl.Show();
                };

                var dlg = new XFPopupDlg(
                    page,
                    new StackLayout
                    {
                        Children = {
                            new Label{
                                Text = "POPOPOPOPO"
                            },
                            anotherDropButton
                        }
                    },
                    true,
                    "TEST",
                    true,
                    "OK",
                    "KO");




                dlg.OnResult += (XFPopupDlg dialog, bool result) =>
                {
                    if (!result)
                    {
                        dlg.Hide();
                    }
                    else
                    {

                        var newdlg = new XFPopupDlg(
                            page,
                            new StackLayout
                            {
                                Children = {
                                    new Label{
                                        Text = "Another overlap popup"
                                    }
                                }
                            },
                            true,
                            "NEW",
                            true,
                            "NEW OK",
                            null);


                        newdlg.OnResult += (XFPopupDlg ndlg, bool nresult) =>
                        {
                            ndlg.Hide();
                        };

                        newdlg.Show();

                    }

                    //dlg.Hide();
                };

                dlg.Show();

            };


            MainPage = page;

            StartTimer();
        }
        
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
