namespace SistemaGestaoEscola.Web.DTOs
{
    public class SubjectGradeSummaryDto
    {
        public int SubjectId { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hours { get; set; }
        public int AbsenceLimit { get; set; }

        public bool HasGrade { get; set; }
        public int? Value { get; set; }
        public int? UnexcusedAbsence { get; set; }

        public bool FailedByAbsence { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
