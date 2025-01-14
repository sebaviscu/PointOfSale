using PointOfSale.Jobs.Executors;
using static PointOfSale.Model.Enum;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Configuration;

namespace PointOfSale.Jobs.Factories
{
    public class JobExecutorFactory : IJobExecutorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public JobExecutorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJobExecutor GetJobExecutor(JobTypeEnum jobType)
        {
            return jobType switch
            {
                JobTypeEnum.GenerateEmbeddings => ActivatorUtilities.CreateInstance<GenerateEmbeddingsJobExecutor>(_serviceProvider),
                _ => throw new ArgumentException("Tipo de job no soportado")
            };
        }
    }
}
