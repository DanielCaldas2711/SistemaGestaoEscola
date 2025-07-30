namespace SistemaGestaoEscola.Web.Helpers.Interfaces
{
    public interface ITimeZoneHelper
    {
        DateTime ConvertLisbonToUtc(DateTime localDateTime);
        DateTime ConvertUtcToLisbon(DateTime utcDateTime);
        TimeZoneInfo GetLisbonTimeZone();
    }
}
