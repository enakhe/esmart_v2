using ESMART.Domain.Entities.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Interface
{
    public interface ICardRepository
    {
        Task AddAuthCard(AuthorizationCard card);
        Task<AuthorizationCard> GetAuthorizationCardByComputerName(string computerName);
        Task<AuthorizationCard> GetAuthCardById(string id);
    }
}
