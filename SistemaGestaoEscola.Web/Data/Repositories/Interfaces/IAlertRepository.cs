using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

public interface IAlertRepository : IGenericRepository<Alert>
{
    Task<int> CountUnreadAsync(string userId);
    Task<IEnumerable<Alert>> GetForUserAsync(string userId);
}
