namespace AgricultureApp.MauiClient.Models
{
    public class FieldUpdate
    {
        public string FieldId { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public string SizeUnit { get; set; }
        public FieldStatus Status { get; set; }
        public SoilType SoilType { get; set; }

        public string OwnerFarmId { get; set; }
    }
}
