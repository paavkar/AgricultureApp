namespace AgricultureApp.Domain.Farms
{
    public class FieldBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public string SizeUnit { get; set; }
        public FieldStatus Status { get; set; }
        public SoilType SoilType { get; set; }
        public string FarmId { get; set; }
        public string OwnerFarmId { get; set; }
    }
}
