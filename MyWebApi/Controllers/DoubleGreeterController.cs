using Microsoft.AspNetCore.Mvc;
using MyGrpcService;

namespace MyWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DoubleGreeterController : ControllerBase
    {

        private readonly ILogger<DoubleGreeterController> _logger;
        private readonly Greeter.GreeterClient _grpcClient;

        public DoubleGreeterController(ILogger<DoubleGreeterController> logger, Greeter.GreeterClient grpClient)
        {
            _logger = logger;
            _grpcClient = grpClient;
        }

        [HttpGet]
        public async Task<DoubleGreeting> Get(string a, string b)
        {
            var greetA = _grpcClient.SayHelloAsync(new HelloRequest() { Name = a });
            var greetB = _grpcClient.SayHelloAsync(new HelloRequest() { Name = b });
            await Task.WhenAll(greetA.ResponseAsync, greetB.ResponseAsync);
            return new DoubleGreeting()
            {
                GreetingA = (await greetA).Message,
                GreetingB = (await greetB).Message,
            };
        }
    }
}
