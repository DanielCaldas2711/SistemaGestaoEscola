using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories;

public class AlertRepository : GenericRepository<Alert>, IAlertRepository
{
    private readonly DataContext _context;

    public AlertRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<int> CountUnreadAsync(string userId)
    {
        return await _context.Alerts
            .Where(a => a.ToUserId == userId && !a.IsRead)
            .CountAsync();
    }

    public async Task<IEnumerable<Alert>> GetForUserAsync(string userId)
    {
        return await _context.Alerts
            .Where(a => a.ToUserId == userId)
            .Include(a => a.FromUser)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
