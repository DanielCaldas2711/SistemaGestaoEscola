using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.Data.Repositories.Interfaces
{
    public interface IClassProfessorsRepository : IGenericRepository<ClassProfessors>
    {
        Task<IEnumerable<ClassProfessors>> GetAllClassProfessors(int ClassId);
    }
}
