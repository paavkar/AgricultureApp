using AgricultureApp.MauiClient.Resources.Strings;

namespace AgricultureApp.MauiClient.Models
{
    public class FarmManager : FarmPerson
    {
        public DateTimeOffset AssignedAt { get; set; }

        public string AssignedAtString =>
            string.Format(AppResources.ManagerAssignedAt,
                AssignedAt.ToLocalTime().ToString("yyyy-mm-dd HH:mm"));
    }
}
