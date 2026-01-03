namespace AgricultureApp.MauiClient.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}