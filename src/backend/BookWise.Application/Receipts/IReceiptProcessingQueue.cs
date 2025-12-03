using System.Threading;
using System.Threading.Tasks;

namespace BookWise.Application.Receipts;

public interface IReceiptProcessingQueue
{
    Task EnqueueAsync(int receiptId, CancellationToken cancellationToken);
}
