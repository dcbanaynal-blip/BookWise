using System.Threading;
using System.Threading.Tasks;

namespace BookWise.Application.Receipts;

public interface IReceiptFileStorage
{
    Task<ReceiptFileSaveResult> SaveAsync(ReceiptFilePayload payload, CancellationToken cancellationToken);
}

public sealed record ReceiptFilePayload(string FileName, string ContentType, byte[] Data);

public sealed record ReceiptFileSaveResult(byte[] Data, string ContentType);
