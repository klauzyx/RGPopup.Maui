using CoreGraphics;
using Foundation;
using RGPopup.Maui.IOS.Renderers;
using RGPopup.Maui.Pages;
using UIKit;

namespace RGPopup.Maui.IOS.Platform
{
    [Preserve(AllMembers = true)]
    [Register("RgPopupWindow")]
    internal class PopupWindow : UIWindow
    {
        public PopupWindow(IntPtr handle) : base(handle)
        {
            // Fix #307
        }

        public PopupWindow()
        {

        }

        public PopupWindow(UIWindowScene uiWindowScene) : base(uiWindowScene)
        {

        }

        public override UIView HitTest(CGPoint point, UIEvent? uievent)
        {
            var platformRenderer = (PopupPageRenderer?)RootViewController;
            var pageHandler = platformRenderer?.Handler;
            var hitTestResult = base.HitTest(point, uievent);

            var formsElement = platformRenderer?.CurrentElement;
            if (formsElement == null)
                return hitTestResult;

            if (formsElement.InputTransparent)
                return null!;

            // The typed PlatformView getter throws "PlatformView cannot be null here" when a touch arrives while the
            // popup is being torn down (upstream issue #27, PR #28). The untyped IElementHandler one returns null instead.
            var nativeView = (pageHandler as IElementHandler)?.PlatformView as UIView;
            if (nativeView == null)
                return hitTestResult;

            var scrollView = formsElement.WrappedContent?.Handler?.PlatformView as UIView;
            var contentView = formsElement.CoreContent?.Handler?.PlatformView as UIView;
            var safePadding = formsElement.SafePadding;
            if ((formsElement.BackgroundClickedCommand != null || formsElement.BackgroundInputTransparent || formsElement.CloseWhenBackgroundIsClicked)
                && Math.Max(SafeAreaInsets.Left, safePadding.Left) < point.X && point.X < (Bounds.Width-Math.Max(SafeAreaInsets.Right, safePadding.Right))
                && Math.Max(SafeAreaInsets.Top, safePadding.Top) < point.Y && point.Y < (Bounds.Height-Math.Max(SafeAreaInsets.Bottom, safePadding.Bottom))
                && (hitTestResult.Equals(nativeView) || hitTestResult.Equals(contentView) || hitTestResult.Equals(scrollView) || hitTestResult.Equals(scrollView?.Subviews?.FirstOrDefault())))
            {
                _ = formsElement.SendBackgroundClick();
                if (formsElement.BackgroundInputTransparent)
                {
                    return null!;
                }
            }

            return hitTestResult;
        }
    }
}
