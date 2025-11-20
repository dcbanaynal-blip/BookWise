namespace BookWise.OcrWorker;

public class OcrWorkerOptions
{
    public int BatchSize { get; set; } = 5;
    public int IdleDelaySeconds { get; set; } = 5;
}
