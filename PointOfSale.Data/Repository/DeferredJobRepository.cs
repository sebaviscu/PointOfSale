using PointOfSale.Data.DBContext;
using PointOfSale.Model.Jobs;

namespace PointOfSale.Data.Repository
{
    public class DeferredJobRepository
    {
        private readonly POINTOFSALEContext _context;

        public DeferredJobRepository(POINTOFSALEContext context)
        {
            _context = context;
        }

        public async Task<List<DeferredJob>> GetPendingJobsAsync()
        {
            return await Task.Run(() =>
                _context.DeferredJobs
                        .Where(j => j.IsActive && !j.IsCompleted && j.ScheduleTime <= DateTime.Now)
                        .ToList()
            );
        }

        public async Task MarkJobAsCompletedAsync(int jobId)
        {
            var job = await _context.DeferredJobs.FindAsync(jobId);
            if (job != null)
            {
                job.IsCompleted = true;
                job.LastExecutionTime = DateTime.Now;
                job.Result = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkJobAsErrorAsync(int jobId, string error)
        {
            var job = await _context.DeferredJobs.FindAsync(jobId);
            if (job != null)
            {
                job.IsCompleted = true;
                job.LastExecutionTime = DateTime.Now;
                job.Result = false;
                job.Error = error;
                await _context.SaveChangesAsync();
            }
        }
    }
}
