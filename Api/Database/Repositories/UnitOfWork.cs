using System.Threading;
using System.Threading.Tasks;

namespace Api.Database.Repositories
{
    public class UnitOfWork
    {
        private readonly DatabaseContext _context;
        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
