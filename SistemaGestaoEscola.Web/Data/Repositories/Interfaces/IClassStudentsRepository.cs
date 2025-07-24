using SistemaGestaoEscola.Web.Data.Entities;

namespace SistemaGestaoEscola.Web.Data.Repositories.Interfaces
{
    public interface IClassStudentsRepository : IGenericRepository<ClassStudents>
    {
        Task<bool> IsStudentInClass(int ClassId, string StudentId);
    }    
}
