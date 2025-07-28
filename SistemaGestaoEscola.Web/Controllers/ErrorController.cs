using Microsoft.AspNetCore.Mvc;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : Controller
{
    [HttpGet]
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        return statusCode switch
        {
            400 => View("ErrorViews/BadRequest"),
            401 => View("ErrorViews/Unauthorized"),
            403 => View("ErrorViews/Forbidden"),
            404 => View("ErrorViews/NotFound"),
            _ => View("ErrorViews/ServerError")
        };
    }

    [Route("Error/500")]
    public IActionResult InternalServerError()
    {
        return View("ErrorViews/ServerError");
    }

}
