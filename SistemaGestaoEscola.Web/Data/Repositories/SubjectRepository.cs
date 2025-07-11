using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;

namespace SistemaGestaoEscola.Web.Data.Repositories
{
    public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
    {
        private readonly DataContext _dataContext;

        public SubjectRepository(DataContext dataContext) : base(dataContext) 
        {        
            _dataContext = dataContext;
        }

        public async Task<Subject> GetByCodeAsync(int code)
        {
            return await _dataContext.Subjects.Where(o => o.Code == code).FirstOrDefaultAsync();
        }
    }
}
