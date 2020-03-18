using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microservice.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microservice.Application.Common.Behaviours
{
    public class RequestPerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly ServiceSettings _serviceSettings;

        public RequestPerformanceBehaviour(IOptions<ServiceSettings> serviceSettingsAccesor, ILogger<TRequest> logger)
        {
            _logger = logger;
            _serviceSettings = serviceSettingsAccesor.Value;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            _timer.Restart();
            var response = await next();
            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
            if (elapsedMilliseconds > _serviceSettings.Context.ResponseTimeThreshold)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning("Long Running Request: {Name} ({elapsedMilliseconds} milliseconds)",
                requestName, elapsedMilliseconds);
            }

            return response;
        }
    }
}