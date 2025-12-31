using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;

        private readonly FarmRepository _farmRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly ILogger<MainPageModel> _logger;

        [ObservableProperty]
        private List<Farm> _ownedFarms = [];

        [ObservableProperty]
        private List<Farm> _managedFarms = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private Farm? selectedOwnedFarm;

        [ObservableProperty]
        private Farm? selectedManagedFarm;

        public MainPageModel(
            FarmRepository farmRepository,
            ModalErrorHandler errorHandler,
            ILogger<MainPageModel> logger)
        {
            _farmRepository = farmRepository;
            _errorHandler = errorHandler;
            _logger = logger;
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                FarmResult result = await _farmRepository.GetOwnedFarmsAsync();
                OwnedFarms = result.Farms;
                FarmResult managedResult = await _farmRepository.GetManagedFarmsAsync();
                ManagedFarms = managedResult.Farms;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadData();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void NavigatedTo() =>
            _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() =>
            _isNavigatedTo = false;

        [RelayCommand]
        private async Task Appearing()
        {
            if (!_dataLoaded)
            {
                _dataLoaded = true;
                await Refresh();
            }
            // This means we are being navigated to
            else if (!_isNavigatedTo)
            {
                await Refresh();
            }
        }

        [RelayCommand]
        private Task? NavigateToFarm(Farm farm)
            => farm is null ? null : Shell.Current.GoToAsync($"farm?id={farm.Id}");
    }
}
