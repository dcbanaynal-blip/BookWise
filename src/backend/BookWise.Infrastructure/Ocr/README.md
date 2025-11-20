## Tesseract Assets

- Drop `.traineddata` language files (e.g., `eng.traineddata`) into `Ocr/tessdata`.
- The `Tesseract` NuGet package ships the native binaries; at runtime configure `TesseractEngine` to point to this folder:

```csharp
var tessDataPath = Path.Combine(AppContext.BaseDirectory, "Ocr", "tessdata");
using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
```

> Keep language files out of source control if they are large; optionally fetch them during CI via a script.
