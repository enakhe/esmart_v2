using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Repositories.Configuration
{
    public class CardRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : ICardRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        // Add card
        public async Task AddAuthCard(AuthorizationCard card)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.AuthorizationCards.AddAsync(card);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding auth card. " + ex.Message);
            }
        }

        //Get auth card by computer name
        public async Task<AuthorizationCard> GetAuthorizationCardByComputerName(string computerName)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var card = await context.AuthorizationCards.FirstOrDefaultAsync(aC => aC.ComputerName == computerName);

                return card!;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting auth card by computer name. " + ex.Message);
            }
        }

        public async Task<AuthorizationCard> GetAuthCardById(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var card = await context.AuthorizationCards.FirstOrDefaultAsync(aC => aC.Id == id);

                return card!;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting auth card by id. " + ex.Message);
            }
        }
    }
}
