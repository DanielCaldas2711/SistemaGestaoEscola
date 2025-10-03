using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Data.Repositories.Interfaces;
using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Data.Repositories
{
    public class ClassStudentsRepository : GenericRepository<ClassStudents>, IClassStudentsRepository
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;

        public ClassStudentsRepository(DataContext dataContext,
            IUserHelper userHelper) : base(dataContext)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
        }

        public async Task<ClassStudents> GetStudentByStudentId(string id)
        {
            return await _dataContext.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentId == id);
        }

        public async Task<bool> IsStudentInClass(int ClassId, string StudentId)
        {
           return await _dataContext.ClassStudents.Where(s => s.ClassId == ClassId).AnyAsync(s => s.StudentId == StudentId);
        }

        public async Task<List<StudentRequest>> GetAllStudentsFromClass(int classId)
        {
            var StudentIds = _dataContext.ClassStudents.Where(s => s.ClassId == classId).Select(s => s.StudentId);

            List<StudentRequest> Students = new List<StudentRequest>();

            foreach (string id in StudentIds)
            {
                var student = await _userHelper.GetUserByIdAsync(id);

                Students.Add(new StudentRequest
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Email = student.Email,
                    PhoneNumber = student.PhoneNumber
                });
            }

            return Students;
        }
    }
}
