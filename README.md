# A WIP native perfomance popup library for xamarin.forms
A popup (floating over) display for xamarin.forms.view

#Motivation

Just like you (or maybe not - who knows ;) ), I'm doing some exprimentions to learn how far we can go with XF. Although the framework look very promissing, it's still lack some basic things for us to make a good app.  A way to float views above others view is one of them. Our requirements are : 
  + This must have the native performance 
  + This must flexible enough to display any complex view.

#Behind the scene

There is a popup in xlab library which use a relative layout to mimic the popup effect, but it's seem too slow for us (about 150~200ms for complex view). Frankly, I think Xamarin should spend more time for the xf's layout mechanism, as too many heavy work be invoked when we add/remove a view to/from a layout. 

There also another option https://github.com/EgorBo/Toasts.Forms.Plugin but it predefined the view's layout of toast. So every time we need a new complex view, we have to scuba dive deep to the native layer, it's blow out all the advantages the XF bring to us.

The most hardest part we found in this project is converting any xamarin's view to a native view, then display this native view as a popup is just an easy cake. After search through the forum I extracted some hints from this thread : https://forums.xamarin.com/discussion/33092/rendererfactory-getrenderer-is-it-bug-child-content to get the renderer. But it's not enough. In this case, decompilation is our friend (I pray Xamarin will never obfuscate their dll cause they using lots of internal - undocumented function). What's I exploited is each complex XF's view need an IPlatform to do their own layout's job, that's why we only get the root layout painted correctly if we only set the renderer. 

So to convert a xf's view to native views, all we need to do is: 
  ```
  public IVisualElementRenderer Convert(Xamarin.Forms.View source, Xamarin.Forms.View valid) {
      IVisualElementRenderer render = (IVisualElementRenderer) source.GetValue(RendererProperty);
      if (render == null)
      {
          render = RendererFactory.GetRenderer(source);
          source.SetValue(RendererProperty, render);
          var p = PlatformProperty.GetValue(valid);
          PlatformProperty.SetValue(source, p);
          IsPlatformEnabledProperty.SetValue(source, true);
      }
      
      return render;
  }
  ```
#What's in the lib
a wip library to display any view as : 
  + modal dialog
  + dropdown view (like android's spinner or combobox)
  + top-down notification (not yet complete)
  + a native loading view

As this is a part of my bigger project, I'll try my best to up it date when I got time. I really hope that someone could help me to finish this

#How to use
Please look in the code, use it for whats you want

#Limitations
A lot ;), some are follows : 
  + The dropdow view is not change the size when the anchor view size changing (eg : the screen rotate from portrait to landscape)
  + the design of dialog look very ugly
  + the function to get screen size is not tested (you can using the xlab device for this)
  + memory leak not tested
