namespace AgricultureApp.MauiClient.Pages;

public partial class VerifyTwoFactorPage : ContentPage
{
    public VerifyTwoFactorPage(VerifyTwoFactorPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}