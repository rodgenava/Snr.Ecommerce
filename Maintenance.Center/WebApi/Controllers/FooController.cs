using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FooController : ControllerBase
    {
        private readonly Foo _foo;

        public FooController(Foo foo)
        {
            _foo = foo;
        }

        [HttpGet]
        public string Get()
        {
            return _foo.InstanceNumber.ToString();
        }
    }
}
