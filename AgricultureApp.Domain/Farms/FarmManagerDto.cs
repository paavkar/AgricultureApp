namespace AgricultureApp.Domain.Farms
{
    public class FarmManagerDto : FarmPerson
    {
        public DateTimeOffset AssignedAt { get; set; }
    }
}
