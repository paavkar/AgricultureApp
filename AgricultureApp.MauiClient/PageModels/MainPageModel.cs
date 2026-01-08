using AgricultureApp.MauiClient.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class MainPageModel : ObservableObject, IFarmPageModel, INotifyPropertyChanged
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;

        private readonly FarmRepository _farmRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly ILogger<MainPageModel> _logger;
        private readonly IFarmHubClient _farmHubClient;

        [ObservableProperty]
        private ObservableCollection<Farm> _ownedFarms = [];

        [ObservableProperty]
        private ObservableCollection<Farm> _managedFarms = [];

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
            ILogger<MainPageModel> logger,
            IFarmHubClient farmHubClient)
        {
            _farmRepository = farmRepository;
            _errorHandler = errorHandler;
            _logger = logger;
            _farmHubClient = farmHubClient;

            _farmHubClient.UserAddedToFarm += OnUserAddedToFarm;
            _farmHubClient.UserRemovedFromFarm += OnUserRemovedFromFarm;

            //_ = _farmHubClient.ConnectAsync();
        }

        private async void OnUserAddedToFarm(object? sender, string farmId)
        {
            FarmResult result = await _farmRepository.GetFarmAsync(farmId);

            if (result.Succeeded)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    Farm farm = result.Farm!;
                    ManagedFarms.Add(farm);
                    await AppShell.DisplaySnackbarAsync(
                        string.Format(AppResources.AddedAsFarmManager, farm.Name));
                    await _farmHubClient.JoinFarmAsync(farm.Id);
                });
            }
        }

        private async void OnUserRemovedFromFarm(object? sender, string farmId)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                Farm? farm = ManagedFarms.FirstOrDefault(f => farmId == f.Id);
                if (farm is null)
                    return;

                ManagedFarms.Remove(farm);
                await AppShell.DisplaySnackbarAsync(
                        string.Format(AppResources.RemovedAsFarmManager, farm!.Name));
                await _farmHubClient.LeaveGroupAsync(farm.Id);
            });
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                //if (_farmHubClient.ConnectionState != Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
                //{
                //    _logger.LogInformation("Waiting a second before retrying.");
                //    await Task.Delay(TimeSpan.FromSeconds(1));
                //    await LoadData();
                //}

                FarmResult result = await _farmRepository.GetOwnedFarmsAsync();
                OwnedFarms = result.Farms;
                //foreach (Farm farm in OwnedFarms)
                //{
                //    await _farmHubClient.JoinGroupAsync(farm.Id);
                //}
                FarmResult managedResult = await _farmRepository.GetManagedFarmsAsync();
                ManagedFarms = managedResult.Farms;

                //foreach (Farm farm in ManagedFarms)
                //{
                //    await _farmHubClient.JoinFarmAsync(farm.Id);
                //}
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

        public void Dispose()
        {
            _farmHubClient.UserAddedToFarm -= OnUserAddedToFarm;
            _farmHubClient.UserRemovedFromFarm -= OnUserRemovedFromFarm;
        }
    }
}
