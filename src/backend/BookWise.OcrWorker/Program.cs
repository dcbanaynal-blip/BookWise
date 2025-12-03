using BookWise.Infrastructure.Ocr;
using BookWise.Infrastructure.Receipts;
using BookWise.OcrWorker;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "BookWise OCR Worker";
});
builder.Services.Configure<OcrWorkerOptions>(builder.Configuration.GetSection("Worker"));
builder.Services.Configure<ReceiptBacklogMonitorOptions>(builder.Configuration.GetSection("BacklogMonitoring"));
builder.Services.AddBookWiseDbContext(builder.Configuration);
builder.Services.AddInfrastructureOcr();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
