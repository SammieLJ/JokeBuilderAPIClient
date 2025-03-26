using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using JokeService;

namespace JokeService.Tests
{
    public class JokeServiceTests : IDisposable
    {
        private readonly StringWriter _consoleOutput;
        private StringReader _consoleInput;
        private readonly TextReader _originalInput;
        private readonly TextWriter _originalOutput;

        public JokeServiceTests()
        {
            _originalOutput = Console.Out;
            _originalInput = Console.In;
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            Console.SetIn(_originalInput);
            _consoleOutput.Dispose();
            _consoleInput?.Dispose();
        }

        private void SetConsoleInput(string input)
        {
            _consoleInput = new StringReader(input);
            Console.SetIn(_consoleInput);
        }

        [Fact]
        public void TryAddJokeFromResponse_ValidResponse_AddsJoke()
        {
            // Arrange
            var scores = new ConcurrentDictionary<int, string>();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new Joke
                {
                    Id = "1",
                    Type = "general",
                    Setup = "Test setup",
                    Punchline = "Test punchline"
                }))
            };

            // Act
            var result = JokeService.TryAddJokeFromResponse(response, ref scores);

            // Assert
            Assert.True(result);
            Assert.Single(scores);
            Assert.Contains("1", scores.Keys.Select(k => k.ToString()));
        }

        [Fact]
        public void TryAddJokeFromResponse_InvalidResponse_ReturnsFalse()
        {
            // Arrange
            var scores = new ConcurrentDictionary<int, string>();
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            // Act
            var result = JokeService.TryAddJokeFromResponse(response, ref scores);

            // Assert
            Assert.False(result);
            Assert.Empty(scores);
        }

        [Fact]
        public void GetJokeDisplayStrings_ReturnsCorrectFormat()
        {
            // Arrange
            var scores = new ConcurrentDictionary<int, string>();
            scores.TryAdd(1, JsonConvert.SerializeObject(new Joke
            {
                Id = "1",
                Type = "general",
                Setup = "Test setup",
                Punchline = "Test punchline"
            }));

            // Act
            var display = JokeService.GetJokeDisplayStrings(scores);

            // Assert
            Assert.Contains("Joke ID: 1", display);
            Assert.Contains("Type: general", display);
            Assert.Contains("Setup: Test setup", display);
            Assert.Contains("Punchline: Test punchline", display);
        }

        [Fact]
        public void GetJokeDisplayStrings_EmptyCache_ReturnsEmptyList()
        {
            // Arrange
            var scores = new ConcurrentDictionary<int, string>();

            // Act
            var display = JokeService.GetJokeDisplayStrings(scores);

            // Assert
            Assert.Empty(display);
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _mockResponse;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _mockResponse = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_mockResponse);
        }
    }
}