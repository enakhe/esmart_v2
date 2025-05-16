using ESMART.Domain.Entities.Configuration;

namespace ESMART.Application.Common.Interface
{
    public interface ICardRepository
    {
        Task AddAuthCard(AuthorizationCard card);
        Task<AuthorizationCard> GetAuthorizationCardByComputerName(string computerName);
        Task<AuthorizationCard> GetAuthCardById(string id);
    }
}
