using System.Collections;

namespace AgricultureApp.MauiClient.Pages;

public partial class FarmChatPage : ContentPage
{
    public FarmChatPage(FarmChatPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        vm.ScrollToBottomRequested += () =>
        {
            if (MessagesCollectionView.ItemsSource is IList items && items.Count > 0)
            {
                MessagesCollectionView.ScrollTo(items[^1], position: ScrollToPosition.End, animate: true);
            }
        };

        vm.CloseRequested += async () =>
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
                await Shell.Current.Navigation.PopModalAsync();
            else
                await Shell.Current.GoToAsync("..");
        };
    }
}