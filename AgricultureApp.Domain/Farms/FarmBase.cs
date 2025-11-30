namespace AgricultureApp.Domain.Farms
{
    public class FarmBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string OwnerId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
