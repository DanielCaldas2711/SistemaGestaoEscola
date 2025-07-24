using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Repositories
{
    public class ClassStudentsRepository : GenericRepository<ClassStudents>, IClassStudentsRepository
    {
        private readonly DataContext _dataContext;

        public ClassStudentsRepository(DataContext dataContext) : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<bool> IsStudentInClass(int ClassId, string StudentId)
        {
           return await _dataContext.ClassStudents.Where(s => s.ClassId == ClassId).AnyAsync(s => s.StudentId == StudentId);
        }
    }
}
