using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Repositories
{
    public class CourseDisciplinesRepository : GenericRepository<CourseDisciplines>, ICourseDisciplinesRepository
    {
        private readonly DataContext _dataContext;

        public CourseDisciplinesRepository(DataContext dataContext) : base(dataContext)
        {
            {
                _dataContext = dataContext;
            }
        }
    }
}
