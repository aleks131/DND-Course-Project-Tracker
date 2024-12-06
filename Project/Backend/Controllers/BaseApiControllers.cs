using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController] // Indicates this class is used as a Web API controller.
    [Route("api/[controller]")] // Specifies the default route template for the controller.
    public abstract class BaseApiController : ControllerBase
    {
    }
}
