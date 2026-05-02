using POS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace POS.Infrastructure.Persistence
{
    public static class FeatureSeeder
    {
        public static async Task SeedFeaturesAsync(ApplicationDbContext context)
        {
            var featuresToSeed = new List<(string Name, string Description)>
            {
                ("Starter", "Perfect for small businesses starting out. Single register, basic inventory, and daily sales reports."),
                ("Pro", "For growing businesses with multiple locations. Unlimited registers, advanced analytics, and multi-location support."),
                ("Enterprise", "Tailored solutions for large franchises. Dedicated account manager, custom integrations, and unlimited locations.")
            };

            foreach (var (name, description) in featuresToSeed)
            {
                var exists = await context.Features.AnyAsync(f => f.FeatureName == name);
                if (!exists)
                {
                    context.Features.Add(new Feature
                    {
                        FeatureName = name,
                        Description = description,
                        IsActive = true
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
