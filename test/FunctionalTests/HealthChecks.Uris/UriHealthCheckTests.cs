﻿using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.Uris
{
    [Collection("execution")]
    public class uri_healthcheck_should
    {
        private readonly ExecutionFixture _fixture;

        public uri_healthcheck_should(ExecutionFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task be_healthy_if_uri_is_available()
        {
            var uri = new Uri("https://httpbin.org/get");

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddUrlGroup(uri, tags: new string[] { "uris" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("uris")
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_method_is_available()
        {
            var uri = new Uri("https://httpbin.org/post");

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddUrlGroup(uri, HttpMethod.Post, tags: new string[] { "uris" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("uris")
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_content_matches()
        {
            // TODO: Know of a service we can use that has a fixed response body we can control?
            var uri = new Uri("https://httpbin.org/get");

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddUrlGroup(
                       uri,
                       httpMethod: HttpMethod.Get,
                       expectedContent: "expected value",
                       tags: new string[] { "uris" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("uris"),
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_healthy_if_content_func_matches()
        {
            var uri = new Uri("https://httpbin.org/get");

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddUrlGroup(
                       uri, 
                       httpMethod: HttpMethod.Get, 
                       expectedContentFunc: async httpContent =>
                       {
                           var json = await httpContent.ReadAsStringAsync();
                           var jToken = JToken.Parse(json);
                           if (jToken["url"].Value<string>() == "https://httpbin.org/get")
                           {
                               return (true, null);
                           }
                           return (false, "Url property didn't match.");
                       }, 
                       tags: new string[] { "uris" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("uris"),
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_uri_is_not_available()
        {
            var uri = new Uri("http://200.0.0.100");

            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddUrlGroup(uri, tags: new string[] { "uris" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("uris")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [InlineData(199)]
        [InlineData(300)]
        [Theory]
        public async Task be_unhealthy_if_status_code_is_error(int statusCode)
        {
            var uri = new Uri($"https://httpbin.org/status/{statusCode}");

            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddUrlGroup(uri, tags: new string[] { "uris" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("uris")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_if_request_is_timeout()
        {
            var uri = new Uri($"https://httpbin.org/delay/2");

            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddUrlGroup(opt=>
                   {
                       opt.AddUri(uri, setup => setup.UseTimeout(TimeSpan.FromSeconds(1)));
                   }, tags: new string[] { "uris" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("uris")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
        [Fact]
        public async Task be_unhealthy_if_request_is_timeout_using_default_timeout()
        {
            var uri = new Uri($"https://httpbin.org/delay/3");

            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddUrlGroup(setup =>
                   {
                       setup.UseTimeout(TimeSpan.FromSeconds(1));
                       setup.AddUri(uri);
                   }, tags: new string[] { "uris" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("uris")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.ServiceUnavailable);
        }


        [Fact]
        public async Task be_unhealthy_if_content_doesnt_match()
        {
            var uri = new Uri("https://httpbin.org/get");

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddUrlGroup(
                       uri,
                       httpMethod: HttpMethod.Get,
                       expectedContent: "something random",
                       tags: new string[] { "uris" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("uris"),
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_if_content_func_doesnt_match()
        {
            var uri = new Uri("https://httpbin.org/get");

            var webHostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                    .AddUrlGroup(
                       uri,
                       httpMethod: HttpMethod.Get,
                       expectedContentFunc: async httpContent => await Task.FromResult((false, "Just cuz")),
                       tags: new string[] { "uris" });
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => r.Tags.Contains("uris"),
                   });
               });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_request_sucess_and_default_timeout_is_configured()
        {
            var uri = new Uri($"https://httpbin.org/delay/2");

            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddUrlGroup(setup =>
                   {
                       setup.UseTimeout(TimeSpan.FromSeconds(3));
                       setup.AddUri(uri);
                   }, tags: new string[] { "uris" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("uris")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.OK);
        }
        [Fact]
        public async Task be_healthy_if_request_sucess_and_timeout_is_configured()
        {
            var uri = new Uri($"https://httpbin.org/delay/2");

            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks()
                   .AddUrlGroup(opt =>
                   {
                       opt.AddUri(uri, setup => setup.UseTimeout(TimeSpan.FromSeconds(3)));
                   }, tags: new string[] { "uris" });
              })
              .Configure(app =>
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions()
                  {
                      Predicate = r => r.Tags.Contains("uris")
                  });
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.OK);
        }
    }
}
