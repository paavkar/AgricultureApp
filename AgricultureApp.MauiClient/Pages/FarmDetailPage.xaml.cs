namespace AgricultureApp.MauiClient.Pages;

public partial class FarmDetailPage : ContentPage
{
    public FarmDetailPage(FarmDetailPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}