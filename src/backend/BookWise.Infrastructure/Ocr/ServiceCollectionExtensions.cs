using BookWise.Infrastructure.Persistence;
using BookWise.Infrastructure.Receipts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookWise.Infrastructure.Ocr;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureOcr(this IServiceCollection services)
    {
        services.AddTransient<IReceiptImagePreprocessor, MagickReceiptImagePreprocessor>();
        services.AddSingleton<ITesseractEngineFactory, TesseractEngineFactory>();
        services.AddScoped<IReceiptOcrPipeline, ReceiptOcrPipeline>();
        services.AddScoped<IAutoCategorizationRuleRefresher, AutoCategorizationRuleRefresher>();
        services.AddScoped<IReceiptBacklogMonitor, ReceiptBacklogMonitor>();
        return services;
    }

    public static IServiceCollection AddBookWiseDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookWiseDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));
        return services;
    }
}
