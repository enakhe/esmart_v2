using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.Data;
using Microsoft.AspNetCore.Identity;

namespace ESMART.Infrastructure.Identity
{
    public static class IdentityResultExtensions
    {
        public static Result ToApplicationResult(this IdentityResult result, ApplicationUser user)
        {
            return result.Succeeded
                ? Result.Success(user)
                : Result.Failure(result.Errors.Select(e => e.Description));
        }
    }
}
