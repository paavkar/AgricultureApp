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

        public List<FarmManager> Managers { get; set; } = [];
        public FarmPerson Owner { get; set; }
        public List<Field> Fields { get; set; } = [];
        public List<Field> OwnedFields { get; set; } = [];
    }
}
