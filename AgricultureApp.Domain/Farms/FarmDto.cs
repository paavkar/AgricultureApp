namespace AgricultureApp.Domain.Farms
{
    public class FarmDto : FarmBase
    {
        public List<FarmManagerDto> Managers { get; set; } = [];
        public FarmPerson Owner { get; set; }
        public List<FieldDto> Fields { get; set; } = [];
        public List<FieldDto> OwnedFields { get; set; } = [];
    }
}
