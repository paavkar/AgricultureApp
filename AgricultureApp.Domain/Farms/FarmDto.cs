namespace AgricultureApp.Domain.Farms
{
    public class FarmDto : FarmBase
    {
        public IEnumerable<FarmManagerDto> Managers { get; set; } = [];
        public FarmPerson Owner { get; set; }
        public IEnumerable<Field> Fields { get; set; } = [];
        public IEnumerable<Field> OwnedFields { get; set; } = [];
    }
}
