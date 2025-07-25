using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.Data.Repositories.Interfaces
{
    public interface IClassProfessorsRepository : IGenericRepository<ClassProfessors>
    {
        IEnumerable<ClassProfessors> GetAllClassProfessors(int ClassId);
    }
}
