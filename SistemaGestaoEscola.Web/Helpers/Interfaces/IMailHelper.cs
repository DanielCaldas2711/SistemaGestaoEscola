using SistemaGestaoEscola.Web.Models;

namespace SistemaGestaoEscola.Web.Helpers.Interfaces
{
    public interface IMailHelper
    {
        Response SendEmail(string to, string subject, string body);
    }
}
