using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Repositories
{
    public class ClassProfessorsRepository : GenericRepository<ClassProfessors>, IClassProfessorsRepository
    {
        private readonly DataContext _dataContext;

        public ClassProfessorsRepository(DataContext dataContext) : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public IQueryable<ClassProfessors> GetAllTracked()
        {
            return _dataContext.ClassProfessors;
        }


        public IEnumerable<ClassProfessors> GetAllClassProfessors(int ClassId)
        {
            return _dataContext.ClassProfessors.Where(p => p.ClassId == ClassId);
        }
    }
}
