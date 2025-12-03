using System;
using Microsoft.Extensions.Configuration;
using Tesseract;

namespace BookWise.Infrastructure.Ocr;

public interface ITesseractEngineFactory
{
    TesseractEngine Create();
}

public sealed class TesseractEngineFactory : ITesseractEngineFactory
{
    private readonly string _dataPath;
    private readonly string _languages;

    public TesseractEngineFactory(IConfiguration configuration)
    {
        _dataPath = configuration["Ocr:TessDataPath"] ?? AppContext.BaseDirectory;
        _languages = configuration["Ocr:Languages"] ?? "eng";
    }

    public TesseractEngine Create()
    {
        return new TesseractEngine(_dataPath, _languages, EngineMode.Default);
    }
}
