﻿using HealthChecks.Uris;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UrisHealthCheckBuilderExtensions
    {
        const string NAME = "uri-group";

        /// <summary>
        /// Add a health check for single uri.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uri">The uri to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions()
                        .AddUri(uri);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for single uri.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uri">The uri to check.</param>
        /// <param name="httpMethod">The http method to use on check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri, HttpMethod httpMethod, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions()
                        .AddUri(uri)
                        .UseHttpMethod(httpMethod);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for single uri, validating the response content matches <paramref name="expectedContent"/> when read as a plain string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uri">The uri to check.</param>
        /// <param name="httpMethod">The http method to use on check.</param>
        /// <param name="expectedContent">The content expected in the response body. Compared with response content as plain string.</param>
        /// <param name="uriOptionsSetup">A way to configure the options that will be used for the Uri. Optional.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri, HttpMethod httpMethod, 
            string expectedContent, 
            Action<IUriOptions> uriOptionsSetup = null,
            string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions()
                        .AddUri(uri, uriOptionsSetup)
                        .UseHttpMethod(httpMethod)
                        .ExpectContent(expectedContent);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for single uri, validate the response content is expected using <paramref name="contentCheckFunc"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uri">The uri to check.</param>
        /// <param name="httpMethod">The http method to use on check.</param>
        /// <param name="contentCheckFunc">
        /// A function that checks the HttpContent of the response. 
        /// Should return a <see cref="ExpectedContentResult" /> if valid,
        /// or a <see cref="UnexpectedContentResult" /> if invalid.
        /// </param>
        /// <param name="uriOptionsSetup">A way to configure the options that will be used for the Uri. Optional.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri, HttpMethod httpMethod,
            Func<HttpContent, Task<ContentCheckResult>> contentCheckFunc,
            Action<IUriOptions> uriOptionsSetup = null,
            string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions()
                        .AddUri(uri, uriOptionsSetup)
                        .UseHttpMethod(httpMethod)
                        .ExpectContent(contentCheckFunc);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags));
        }
        
        /// <summary>
        /// Add a health check for multiple uri's.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uris">The collection of uri's to be checked.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, IEnumerable<Uri> uris, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp => CreateHealthCheck(sp, registrationName, UriHealthCheckOptions.CreateFromUris(uris)),
                failureStatus,
                tags,
                timeout));
        }
        
        /// <summary>
        /// Add a health check for multiple uri's.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uris">The collection of uri's to be checked.</param>
        /// <param name="httpMethod">The http method to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, IEnumerable<Uri> uris, HttpMethod httpMethod, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = UriHealthCheckOptions
                        .CreateFromUris(uris)
                        .UseHttpMethod(httpMethod);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }
        
        /// <summary>
        /// Add a health check for multiple uri's.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uriOptions">The action used to configured uri values and specified http methods to be checked.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Action<UriHealthCheckOptions> uriOptions, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddHttpClient();

            var registrationName = name ?? NAME;
            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions();
                    uriOptions?.Invoke(options);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }
        
        private static UriHealthCheck CreateHealthCheck(IServiceProvider sp, string name, UriHealthCheckOptions options)
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new UriHealthCheck(options, () => httpClientFactory.CreateClient(name));
        }
    }
}
