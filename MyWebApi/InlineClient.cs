using Grpc.Core;
using MyGrpcService;
using MyGrpcService.Services;

namespace MyWebApi
{
    public class InlineClient : Greeter.GreeterClient
    {

        public InlineClient(ILogger<GreeterService> serviceLogger)
            : base(new MyCallInvoker(serviceLogger))
        {
        }

        protected override Greeter.GreeterClient NewInstance(ClientBaseConfiguration configuration)
        {
            return this;
        }

        //public override HelloReply SayHello(HelloRequest request, CallOptions options)
        //{
        //    return new HelloReply()
        //    {
        //        Message = "hi there " + request.Name,
        //    };
        //}

        //public override AsyncUnaryCall<HelloReply> SayHelloAsync(HelloRequest request, CallOptions options)
        //{
        //    var ret = SayHello(request, options);
        //    return new AsyncUnaryCall<HelloReply>(Task.FromResult(ret), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        //}

        private class MyServiceBinder : ServiceBinderBase
        {
            public readonly Dictionary<string, Delegate> UnaryServerMethods = new Dictionary<string, Delegate>();

            public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
            {
                UnaryServerMethods.Add(method.FullName, handler);
            }
        }

        private class MyCallInvoker : CallInvoker
        {
            readonly GreeterService _inner;
            readonly Dictionary<string, Delegate> UnaryServerMethods;

            public MyCallInvoker(ILogger<GreeterService> serviceLogger)
            {
                _inner = new GreeterService(serviceLogger);
                var binder = new MyServiceBinder();
                Greeter.BindService(binder, _inner);
                this.UnaryServerMethods = binder.UnaryServerMethods;
            }

            public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
            {
                throw new NotImplementedException();
            }

            public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
            {
                throw new NotImplementedException();
            }

            public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
            {
                throw new NotImplementedException();
            }

            public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
            {
                var handler = (UnaryServerMethod<TRequest, TResponse>)UnaryServerMethods[method.FullName];

                var callContext = new MyCallContext<TRequest, TResponse>(method, options);

                //TODO: fill in the trailers and status properly
                return new AsyncUnaryCall<TResponse>(callContext.WrapCall(handler, request), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), new Action(() => { }));
            }

            public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
            {
                throw new NotImplementedException();
            }
        }

        private class MyCallContext<TRequest, TResponse> : ServerCallContext
            where TRequest : class
            where TResponse : class
        {
            readonly IMethod _method;
            readonly CallOptions _options;

            public Metadata ResponseHeaders { get; private set; }

            public MyCallContext(IMethod method, CallOptions options)
            {
                this._method = method;
                this._options = options;
                this.WriteOptionsCore = options.WriteOptions!;
            }

            public async Task<TResponse> WrapCall(UnaryServerMethod<TRequest, TResponse> method, TRequest req)
            {
                // TODO: provide the same sort of exception handlering as the client does
                return await method(req, this);
            }

            protected override string MethodCore => _method.FullName;

            protected override string HostCore => "localhost:7119";

            protected override string PeerCore => "ipv6:[::1]:58864";

            protected override DateTime DeadlineCore => _options.Deadline ?? DateTime.MaxValue;

            protected override Metadata RequestHeadersCore => _options.Headers ?? new Metadata();

            protected override CancellationToken CancellationTokenCore => _options.CancellationToken;

            protected override Metadata ResponseTrailersCore { get; } = new Metadata();

            protected override Status StatusCore { get; set; }
            protected override WriteOptions WriteOptionsCore { get; set; }

            protected override AuthContext AuthContextCore => throw new NotImplementedException();

            protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
            {
                throw new NotImplementedException();
            }

            protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
            {
                this.ResponseHeaders = responseHeaders;
                return Task.CompletedTask;
            }
        }
    }
}
