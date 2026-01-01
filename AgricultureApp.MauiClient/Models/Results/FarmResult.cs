using System.Collections.ObjectModel;

namespace AgricultureApp.MauiClient.Models
{
    public class FarmResult : BaseResult
    {
        public Farm? Farm { get; set; }
        public ObservableCollection<Farm>? Farms { get; set; }
    }
}
