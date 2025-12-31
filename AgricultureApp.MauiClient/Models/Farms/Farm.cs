using AgricultureApp.MauiClient.Resources.Strings;
using System.Collections.ObjectModel;

namespace AgricultureApp.MauiClient.Models
{
    public class Farm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string OwnerId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public ObservableCollection<FarmManager> Managers { get; set; } = [];
        public FarmPerson Owner { get; set; }
        public ObservableCollection<Field> Fields { get; set; } = [];
        public ObservableCollection<Field> OwnedFields { get; set; } = [];

        public string ManagersCount =>
            string.Format(AppResources.ManagerCount, Managers.Count);
        public string FieldsCount =>
            string.Format(AppResources.CultFieldsCount, Fields.Count);
        public string OwnedFieldsCount =>
            string.Format(AppResources.OwnedFieldsCount, OwnedFields.Count);
    }
}
