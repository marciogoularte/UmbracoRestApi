﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Tests.TestHelpers;

namespace Umbraco.RestApi.Tests
{
    public abstract class ControllerTests
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));
            var mockSettings = MockUmbracoSettings.GenerateMockSettings();
            UmbracoConfig.For.CallMethod("SetUmbracoSettings", mockSettings);
        }

        [TearDown]
        public void TearDown()
        {
            //Hack - because Reset is internal
            typeof(PropertyEditorResolver).CallStaticMethod("Reset", true);
            UmbracoRestApiOptionsInstance.Options = new UmbracoRestApiOptions();
        }

        protected async Task Get_Root_With_OPTIONS(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}"),
                    Method = HttpMethod.Options,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Headers.Add("Access-Control-Request-Headers", "accept, authorization");
                request.Headers.Add("Access-Control-Request-Method", "GET");
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Add("Referer", "http://localhost:12061/browser.html");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        protected async Task Get_Root_Result(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}"),
                    Method = HttpMethod.Get,
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                var asdf = GlobalConfiguration.Configuration;

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual($"/umbraco/rest/v1/{segment}", djson["_links"]["root"]["href"].Value<string>());
                Assert.AreEqual(2, djson["_links"]["content"].Count());
                Assert.AreEqual(2, djson["_embedded"]["content"].Count());
            }
        }

        protected async Task Search_200_Result(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}/search?query=parentID:\\-1"),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        protected async Task Get_Id_Result(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}/123"),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual($"/umbraco/rest/v1/{segment}/123", djson["_links"]["self"]["href"].Value<string>());
                Assert.AreEqual($"/umbraco/rest/v1/{segment}/456", djson["_links"]["parent"]["href"].Value<string>());
                Assert.AreEqual($"/umbraco/rest/v1/{segment}/123/children{{?page,size,query}}", djson["_links"]["children"]["href"].Value<string>());
                Assert.AreEqual($"/umbraco/rest/v1/{segment}", djson["_links"]["root"]["href"].Value<string>());

                var properties = djson["properties"].ToObject<IDictionary<string, object>>();
                Assert.AreEqual(2, properties.Count);
                Assert.IsTrue(properties.ContainsKey("TestProperty1"));
                Assert.IsTrue(properties.ContainsKey("testProperty2"));
            }
        }

        protected async Task Get_Metadata_Result(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}/123/meta"),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                //TODO: Assert values!


            }
        }
        
        protected async Task Get_Children_Is_200_Response(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}/123/children"),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }


        protected async Task Get_Descendants_Is_200_Response(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}/123/descendants?page=2&size=3&query=hello"),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        protected async Task Get_Ancestors_Is_200_Response(TestStartup startup, string segment)
        {
            using (var server = TestServer.Create(startup.Configuration))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{segment}/123/ancestors?page=2&size=3&query=hello"),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }

        }
    }
}