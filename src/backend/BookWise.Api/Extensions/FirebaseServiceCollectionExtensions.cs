using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWise.Api.Extensions;

public static class FirebaseServiceCollectionExtensions
{
    public static IServiceCollection AddFirebaseAdmin(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var firebaseSection = configuration.GetSection("Firebase");
        var serviceAccountPath = firebaseSection.GetValue<string>("ServiceAccountKeyPath");

        GoogleCredential credential;

        if (environment.IsDevelopment())
        {
            if (string.IsNullOrWhiteSpace(serviceAccountPath))
            {
                throw new InvalidOperationException(
                    "Firebase:ServiceAccountKeyPath must be configured in development to use a service-account JSON key.");
            }

            credential = GoogleCredential.FromFile(serviceAccountPath);
        }
        else
        {
            credential = GoogleCredential.GetApplicationDefault();
        }

        var app = FirebaseApp.Create(new AppOptions
        {
            Credential = credential
        });

        services.AddSingleton(app);
        services.AddSingleton(FirebaseAuth.GetAuth(app));

        return services;
    }
}
