using AgricultureApp.MauiClient.Resources.Strings;

namespace AgricultureApp.MauiClient.Models
{
    public class Field
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public string SizeUnit { get; set; }
        public FieldStatus Status { get; set; }
        public SoilType SoilType { get; set; }
        public string FarmId { get; set; }
        public string OwnerFarmId { get; set; }

        public List<FieldCultivation> Cultivations { get; set; } = [];
        public Farm CurrentFarm { get; set; }
        public Farm OwnerFarm { get; set; }

        public string SizeString =>
            string.Format(AppResources.FieldSize, Size, SizeUnit);
    }

    public enum FieldStatus
    {
        Active = 0,
        Inactive,
        UnderMaintenance
    }

    public enum SoilType
    {
        Sandy = 0,
        Clay,
        Silty,
        Peaty,
        Chalky,
        Loamy,
        Other,
        Generic
    }
}
