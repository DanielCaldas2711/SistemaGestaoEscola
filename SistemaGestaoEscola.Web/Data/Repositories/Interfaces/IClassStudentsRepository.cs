using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Data.Repositories.Interfaces
{
    public interface IClassStudentsRepository : IGenericRepository<ClassStudents>
    {
        Task<bool> IsStudentInClass(int ClassId, string StudentId);

        Task<List<StudentRequest>> GetAllStudentsFromClass(int classId);

        Task<ClassStudents> GetStudentByStudentId(string id);
    }    
}
