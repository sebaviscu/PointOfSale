using PointOfSale.Jobs.Executors;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Jobs.Factories
{
    public interface IJobExecutorFactory
    {
        IJobExecutor GetJobExecutor(JobTypeEnum jobType);
    }
}
