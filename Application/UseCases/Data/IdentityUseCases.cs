using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Models;

namespace ESMART.Application.UseCases.Data
{
    public class IdentityUseCases
    {
        private readonly IIdentityService _identityService;

        public IdentityUseCases(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public Task<Result> CreateUser(string username, string password)
        {
            return _identityService.CreatUserAsync(username, password);
        }

        public Task<Result> LoginUser(string username, string password)
        {
            return _identityService.LoginUserAsync(username, password);
        }
    }
}
