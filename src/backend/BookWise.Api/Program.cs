using System;
using BookWise.Api.Authentication;
using BookWise.Api.Extensions;
using BookWise.Application.Accounts;
using BookWise.Application.Users;
using BookWise.Application.Receipts;
using BookWise.Infrastructure.Accounts;
using BookWise.Infrastructure.Ocr;
using BookWise.Infrastructure.Persistence;
using BookWise.Infrastructure.Users;
using BookWise.Infrastructure.Receipts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddOpenApi();

builder.Services.AddBookWiseDbContext(builder.Configuration);
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IReceiptsService, ReceiptsService>();
builder.Services.AddFirebaseAdmin(builder.Configuration, builder.Environment);
builder.Services
    .AddAuthentication(FirebaseAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(
        FirebaseAuthenticationDefaults.AuthenticationScheme,
        options => { });

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await BookWiseDbContextSeeder.SeedAsync(app.Services);

app.Run();
