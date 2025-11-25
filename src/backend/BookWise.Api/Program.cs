using BookWise.Api.Authentication;
using BookWise.Api.Extensions;
using BookWise.Infrastructure.Ocr;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddBookWiseDbContext(builder.Configuration);
builder.Services.AddFirebaseAdmin(builder.Configuration, builder.Environment);
builder.Services
    .AddAuthentication(FirebaseAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(
        FirebaseAuthenticationDefaults.AuthenticationScheme,
        options => { });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
