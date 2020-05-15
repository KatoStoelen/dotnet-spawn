using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetSpawn.Points.Bitbucket.Client
{
    internal class BitbucketClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        private string _cookie;

        public BitbucketClient(string serverUrl, string personalAccessToken)
        {
            _httpClient = new HttpClient(new HttpClientHandler { UseCookies = false })
            {
                BaseAddress = new Uri(new Uri(serverUrl), "/rest/api/1.0/")
            };

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {personalAccessToken}");
            _httpClient.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0");

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync("application-properties", cancellationToken);

            response.EnsureSuccessStatusCode();

            if (!response.Headers.Contains("Set-Cookie"))
                return;

            _cookie = response.Headers.GetValues("Set-Cookie")
                .Single()
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .First();
        }

        public Task<Models.Read.Project> CreateProjectAsync(
            Models.Write.Project project, CancellationToken cancellationToken)
        {
            return PostAsync<Models.Write.Project, Models.Read.Project>(
                "projects", project, cancellationToken);
        }

        public Task<Models.Read.Repository> CreateRepositoryAsync(
            string projectKey,
            Models.Write.Repository repository,
            CancellationToken cancellationToken)
        {
            return PostAsync<Models.Write.Repository, Models.Read.Repository>(
                $"projects/{projectKey}/repos", repository, cancellationToken);
        }

        private async Task<TReturn> PostAsync<TPayload, TReturn>(
            string uri, TPayload payload, CancellationToken cancellationToken)
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, uri);

            if (!string.IsNullOrEmpty(_cookie))
                postRequest.Headers.Add("Cookie", _cookie);

            postRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload, _serializerOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(postRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonSerializer.Deserialize<TReturn>(responseContent, _serializerOptions);
        }
    }
}