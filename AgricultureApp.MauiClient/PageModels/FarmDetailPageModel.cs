using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class FarmDetailPageModel : ObservableObject, IQueryAttributable, IFarmPageModel
    {
        private readonly FarmRepository _farmRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly ILogger<FarmDetailPageModel> _logger;

        [ObservableProperty]
        Farm _farm;

        [ObservableProperty]
        FarmPerson _owner;

        [ObservableProperty]
        ObservableCollection<FarmManager> _managers;

        [ObservableProperty]
        ObservableCollection<Field> _fields;

        [ObservableProperty]
        ObservableCollection<Field> _ownedFields;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        string _pageTitle;

        [ObservableProperty]
        private FarmManager selectedManager;

        [ObservableProperty]
        private Field selectedField;

        public FarmDetailPageModel(
            FarmRepository farmRepository,
            ModalErrorHandler errorHandler,
            ILogger<FarmDetailPageModel> logger)
        {
            _farmRepository = farmRepository;
            _errorHandler = errorHandler;
            _logger = logger;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var value))
            {
                var id = value.ToString();
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
            else if (query.ContainsKey("refresh"))
            {
                RefreshData().FireAndForgetSafeAsync(_errorHandler);
            }
            else
            {
            }
        }

        private async Task RefreshData()
        {
            if (Farm is not null)
            {
                await LoadData(Farm.Id);
            }
        }

        private async Task LoadData(string id)
        {
            try
            {
                IsBusy = true;

                FarmResult result = await _farmRepository.GetFarmAsync(id);

                if (result.Succeeded)
                {
                    Farm = result.Farm;
                    Owner = Farm.Owner;
                    Fields = Farm.Fields;
                    OwnedFields = Farm.OwnedFields;
                    Managers = Farm.Managers;
                }

                if (Farm is null)
                {
                    await AppShell.Current.GoToAsync("//MainPage");
                    _errorHandler.HandleError(new Exception($"Farm with id {id} could not be found."));
                    return;
                }
                PageTitle = Farm.Name;
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
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
                await LoadData(Farm.Id);
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
    }
}
