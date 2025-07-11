using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.Data.Repositories.Interfaces
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {

        Task<Subject> GetByCodeAsync(int code);

    }
}
