#nullable disable

namespace ESMART.Domain.ViewModels.Data
{
    public class ApplicationRoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int NoOfUser { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }
    }
}
