using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Repositories
{
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        private readonly DataContext _dataContext;

        public ClassRepository(DataContext dataContext) : base(dataContext) 
        {
            _dataContext = dataContext;
        }
    }
}
