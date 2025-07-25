using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Repositories
{
    public class StudentGradesRepository : GenericRepository<StudentGrades>, IStudentGradesRepository
    {
        private readonly DataContext _dataContext;

        public StudentGradesRepository(DataContext dataContext) :base(dataContext)
        {
            _dataContext = dataContext;
        }
    }
}
