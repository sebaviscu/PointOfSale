using PointOfSale.Jobs.Factories;
using PointOfSale.Data.Repository;
using System;
using System.Threading.Tasks;

namespace PointOfSale.Jobs.Services
{
    public class DeferredJobService
    {
        private readonly DeferredJobRepository _repository;
        private readonly IJobExecutorFactory _jobExecutorFactory;

        public DeferredJobService(DeferredJobRepository repository, IJobExecutorFactory jobExecutorFactory)
        {
            _repository = repository;
            _jobExecutorFactory = jobExecutorFactory;
        }

        public async Task ProcessJobsAsync()
        {
            var pendingJobs = await _repository.GetPendingJobsAsync();

            if (pendingJobs.Count == 0)
            {
                Console.WriteLine("No hay trabajos pendientes para procesar.");
                return;
            }

            foreach (var job in pendingJobs)
            {
                try
                {
                    Console.WriteLine($"Procesando job: {job.JobName}");

                    var executor = _jobExecutorFactory.GetJobExecutor(job.JobType);
                    await executor.ExecuteAsync(job);

                    await _repository.MarkJobAsCompletedAsync(job.JobId);
                    Console.WriteLine($"JobId: {job.JobId}, procesado con exito.");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Se ha producido un ERROR en JobId: {job.JobId}: {e.Message}");
                    await _repository.MarkJobAsErrorAsync(job.JobId, e.Message);
                    Console.ResetColor();
                }
            }

            Console.WriteLine("Todos los trabajos pendientes han sido procesados.");
        }
    }
}
