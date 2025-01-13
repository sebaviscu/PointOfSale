using PointOfSale.Model.Jobs;

namespace PointOfSale.Jobs.Executors
{
    public interface IJobExecutor
    {
        Task ExecuteAsync(DeferredJob job);
    }
}
