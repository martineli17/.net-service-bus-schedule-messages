using Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Database.Repositories
{
    public class SchedulingRepository
    {
        private readonly DatabaseContext _context;
        public SchedulingRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Scheduling scheduling, CancellationToken cancellationToken)
        {
            await _context.AddAsync(scheduling, cancellationToken);
        }

        public IAsyncEnumerable<Scheduling> GetAllAsync()
        {
           return _context.Set<Scheduling>().AsAsyncEnumerable();
        }

        public async Task<Scheduling> GetByMessageNumberAsync(long messageNumber, CancellationToken cancellationToken)
        {
            return await _context.Set<Scheduling>()
                        .Where(x => x.MessageNumber == messageNumber)
                        .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
