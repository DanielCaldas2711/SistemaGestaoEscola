using SistemaGestaoEscola.Web.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SistemaGestaoEscola.Web.Data.Entities
{
    public class Subject : IEntity, IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public int Code { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(25, 50, ErrorMessage = "Hours must be between 25 and 50.")]
        public int Hours { get; set; }

        [Required]
        public int Absence { get; set; }

        public ICollection<CourseDisciplines> CourseDisciplines { get; set; } = new List<CourseDisciplines>();

        public ICollection<ClassProfessors> Professors { get; set; } = new List<ClassProfessors>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Hours == 25 && Absence != 10)
            {
                yield return new ValidationResult("Absence must be 3 when Hours is 25.", new[] { nameof(Absence) });
            }

            if (Hours == 50 && Absence != 20)
            {
                yield return new ValidationResult("Absence must be 6 when Hours is 50.", new[] { nameof(Absence) });
            }
        }
    }
}
