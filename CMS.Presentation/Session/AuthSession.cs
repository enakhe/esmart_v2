using ESMART.Domain.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Presentation.Session
{
    public class AuthSession
    {
        public static ApplicationUser? CurrentUser { get; set; }
    }
}
