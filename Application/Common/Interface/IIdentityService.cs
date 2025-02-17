using ESMART.Application.Common.Models;

namespace ESMART.Application.Common.Interface
{
    public interface IIdentityService
    {
        Task<Result> CreatUserAsync(string userName, string password);
        Task<Result> LoginUserAsync(string userName, string password);
    }
}
