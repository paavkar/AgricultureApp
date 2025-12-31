using CommunityToolkit.Mvvm.ComponentModel;

namespace AgricultureApp.MauiClient.PageModels
{
    public partial class FarmDetailPageModel : ObservableObject, IQueryAttributable, IFarmPageModel
    {
        private Farm? _farm;
        private readonly FarmRepository _farmRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        string _pageTitle;

        public FarmDetailPageModel(FarmRepository farmRepository, ModalErrorHandler errorHandler)
        {
            _farmRepository = farmRepository;
            _errorHandler = errorHandler;
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
            if (_farm is not null)
            {
                await LoadData(_farm.Id);
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
                    _farm = result.Farm;
                }

                if (_farm is null)
                {
                    _errorHandler.HandleError(new Exception($"Farm with id {id} could not be found."));
                    return;
                }
                PageTitle = _farm.Name;
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
    }
}
