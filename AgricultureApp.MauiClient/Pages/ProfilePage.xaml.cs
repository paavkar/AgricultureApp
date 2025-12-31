namespace AgricultureApp.MauiClient.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfilePageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}