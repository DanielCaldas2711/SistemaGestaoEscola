using SistemaGestaoEscola.Web.Helpers.Interfaces;

namespace SistemaGestaoEscola.Web.Helpers
{
    public class TimeZoneHelper : ITimeZoneHelper
    {
        private static readonly TimeZoneInfo LisbonTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");

        public DateTime ConvertLisbonToUtc(DateTime localDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, LisbonTimeZone);
        }

        public DateTime ConvertUtcToLisbon(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, LisbonTimeZone);
        }

        public TimeZoneInfo GetLisbonTimeZone() => LisbonTimeZone;
    }
}
