﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Uris
{
    public class UriHealthCheck
        : IHealthCheck
    {
        private readonly UriHealthCheckOptions _options;
        private readonly Func<HttpClient> _httpClientFactory;
        public UriHealthCheck(UriHealthCheckOptions options, Func<HttpClient> httpClientFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var defaultHttpMethod = _options.HttpMethod;
            var defaultExpectedStatusCodes = _options.ExpectedHttpCodes;
            var defaultExpectedContent = _options.ExpectedContent;
            var defaultExpectedContentFunc = _options.ExpectedContentFunc;
            var defaultTimeout = _options.Timeout;
            var idx = 0;

            try
            {
                foreach (var item in _options.UrisOptions)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"{nameof(UriHealthCheck)} execution is cancelled.");
                    }

                    var method = item.HttpMethod ?? defaultHttpMethod;
                    var expectedStatusCodes = item.ExpectedHttpCodes ?? defaultExpectedStatusCodes;
                    var expectedContent = item.ExpectedContent ?? defaultExpectedContent;
                    var expectedContentFunc = item.ExpectedContentFunc ?? defaultExpectedContentFunc;
                    var timeout = item.Timeout != TimeSpan.Zero ? item.Timeout : defaultTimeout;

                    var httpClient = _httpClientFactory();

                    var requestMessage = new HttpRequestMessage(method, item.Uri);

                    foreach (var (Name, Value) in item.Headers)
                    {
                        requestMessage.Headers.Add(Name, Value);
                    }

                    using (var timeoutSource = new CancellationTokenSource(timeout))
                    using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken))
                    {
                        var response = await httpClient.SendAsync(requestMessage, linkedSource.Token);

                        if (!((int)response.StatusCode >= expectedStatusCodes.Min && (int)response.StatusCode <= expectedStatusCodes.Max))
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint #{idx} is not responding with code in {expectedStatusCodes.Min}...{expectedStatusCodes.Max} range, the current status is {response.StatusCode}.");
                        }

                        if (expectedContent != null)
                        {
                            var actualContent = await response.Content.ReadAsStringAsync();
                            if (actualContent != expectedContent)
                            {
                                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint #{idx} is not responding with content {expectedContent}, the current content is {actualContent}.");
                            }
                        }

                        if (expectedContentFunc != null)
                        {
                            var contentCheckResult = await expectedContentFunc(response.Content);
                            if (contentCheckResult is UnexpectedContentResult unexpectedContentResult)
                            {
                                var actualContent = await response.Content.ReadAsStringAsync();
                                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint #{idx} is not responding with expected content, reason: '{unexpectedContentResult.Reason}'. The current content is {actualContent}.");
                            }
                        }

                        ++idx;
                    }
                }
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
