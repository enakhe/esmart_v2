using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Repositories.Configuration
{
    public class LicenceRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : ILicenceRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        // Add lincence Info
        public async Task AddLicenceAsync(LicenceInformation licence)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var existingLicenses = context.LicenceInformation.ToList();

                if (existingLicenses.Count != 0)
                {
                    context.LicenceInformation.RemoveRange(existingLicenses);
                    context.SaveChanges();
                }

                await context.LicenceInformation.AddAsync(licence);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding licence information. " + ex.Message);
            }
        }

        public async Task<LicenceInformation?> GetLicenceAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                return await context.LicenceInformation
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting licence information. " + ex.Message);
            }
        }

        public async Task UpdateLicenceAsync(LicenceInformation licence)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.LicenceInformation.Update(licence);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating licence information. " + ex.Message);
            }
        }
    }
}
