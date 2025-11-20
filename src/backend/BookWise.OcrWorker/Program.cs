using BookWise.Infrastructure.Persistence;
using BookWise.OcrWorker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<BookWiseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
