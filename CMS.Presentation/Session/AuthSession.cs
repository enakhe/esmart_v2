using ESMART.Domain.Entities.Data;

namespace ESMART.Presentation.Session
{
    public class AuthSession
    {
        public static ApplicationUser? CurrentUser { get; set; }
    }
}
