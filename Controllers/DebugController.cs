using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            Console.WriteLine("=== DEBUG CONTROLLER TEST METHOD CALLED ===");
            throw new Exception("Debug breakpoint!"); // This will force debugger to stop
            var message = "Breakpoint hit!"; 
            Console.WriteLine($"Message: {message}");
            return Ok(new { message, timestamp = DateTime.Now });
        }

        [HttpGet("simple")]
        public string Simple()
        {
            var result = "Simple endpoint working"; // SET BREAKPOINT HERE
            return result;
        }
    }
}