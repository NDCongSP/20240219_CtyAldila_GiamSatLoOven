#nullable enable

namespace GiamSat.APIClient
{
    /// <summary>
    /// Client đồng bộ Part từ external DB ALD_MFG vào FT14.
    /// File viết tay (không qua NSwag) — tự đăng ký DI qua AutoRegisterInterfaces&lt;IApiService&gt;.
    /// </summary>
    public partial interface IFT14SyncClient : IApiService
    {
        System.Threading.Tasks.Task<PartSyncSourceListResult> GetSyncSourcesAsync();
        System.Threading.Tasks.Task<PartSyncSourceListResult> GetSyncSourcesAsync(System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task<FT14SyncResultResult> SyncPartsAsync(System.Collections.Generic.List<int> partIds);
        System.Threading.Tasks.Task<FT14SyncResultResult> SyncPartsAsync(System.Collections.Generic.List<int> partIds, System.Threading.CancellationToken cancellationToken);
    }

    public partial class FT14SyncClient : IFT14SyncClient
    {
        private System.Net.Http.HttpClient _httpClient;
        private static System.Lazy<System.Text.Json.JsonSerializerOptions> _settings = new System.Lazy<System.Text.Json.JsonSerializerOptions>(CreateSerializerSettings, true);

#pragma warning disable CS8618
        public FT14SyncClient(System.Net.Http.HttpClient httpClient)
#pragma warning restore CS8618
        {
            _httpClient = httpClient;
        }

        private static System.Text.Json.JsonSerializerOptions CreateSerializerSettings()
        {
            var settings = new System.Text.Json.JsonSerializerOptions();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        protected System.Text.Json.JsonSerializerOptions JsonSerializerSettings { get { return _settings.Value; } }

        static partial void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings);

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        public virtual System.Threading.Tasks.Task<PartSyncSourceListResult> GetSyncSourcesAsync()
        {
            return GetSyncSourcesAsync(System.Threading.CancellationToken.None);
        }

        public virtual async System.Threading.Tasks.Task<PartSyncSourceListResult> GetSyncSourcesAsync(System.Threading.CancellationToken cancellationToken)
        {
            using (var request_ = new System.Net.Http.HttpRequestMessage())
            {
                request_.Method = new System.Net.Http.HttpMethod("GET");
                request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                var url_ = "api/FT14/sync/sources";
                request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);
                PrepareRequest(_httpClient, request_, url_);

                return await SendAndReadAsync<PartSyncSourceListResult>(request_, cancellationToken).ConfigureAwait(false);
            }
        }

        public virtual System.Threading.Tasks.Task<FT14SyncResultResult> SyncPartsAsync(System.Collections.Generic.List<int> partIds)
        {
            return SyncPartsAsync(partIds, System.Threading.CancellationToken.None);
        }

        public virtual async System.Threading.Tasks.Task<FT14SyncResultResult> SyncPartsAsync(
            System.Collections.Generic.List<int> partIds, System.Threading.CancellationToken cancellationToken)
        {
            using (var request_ = new System.Net.Http.HttpRequestMessage())
            {
                var json_ = System.Text.Json.JsonSerializer.Serialize(partIds ?? new System.Collections.Generic.List<int>(), JsonSerializerSettings);
                var content_ = new System.Net.Http.StringContent(json_, System.Text.Encoding.UTF8, "application/json");
                request_.Content = content_;
                request_.Method = new System.Net.Http.HttpMethod("POST");
                request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                var url_ = "api/FT14/sync";
                request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);
                PrepareRequest(_httpClient, request_, url_);

                return await SendAndReadAsync<FT14SyncResultResult>(request_, cancellationToken).ConfigureAwait(false);
            }
        }

        private async System.Threading.Tasks.Task<T> SendAndReadAsync<T>(
            System.Net.Http.HttpRequestMessage request_, System.Threading.CancellationToken cancellationToken)
        {
            var response_ = await _httpClient.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            try
            {
                var headers_ = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<string>>();
                foreach (var item_ in response_.Headers)
                    headers_[item_.Key] = item_.Value;
                if (response_.Content != null && response_.Content.Headers != null)
                {
                    foreach (var item_ in response_.Content.Headers)
                        headers_[item_.Key] = item_.Value;
                }

                ProcessResponse(_httpClient, response_);

                var status_ = (int)response_.StatusCode;
                if (status_ == 200)
                {
                    var objectResponse_ = await ReadObjectResponseAsync<T>(response_, headers_, cancellationToken).ConfigureAwait(false);
                    if (objectResponse_.Object == null)
                        throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                    return objectResponse_.Object;
                }
                else
                {
                    var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                }
            }
            finally
            {
                response_.Dispose();
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }
            public T Object { get; }
            public string Text { get; }
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(
            System.Net.Http.HttpResponseMessage response,
            System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers,
            System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
                return new ObjectResponseResult<T>(default(T)!, string.Empty);

            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var typedBody = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(responseStream, JsonSerializerSettings, cancellationToken).ConfigureAwait(false);
                    return new ObjectResponseResult<T>(typedBody!, string.Empty);
                }
            }
            catch (System.Text.Json.JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
            }
        }
    }

    // ── DTO + Result wrappers ────────────────────────────────────────────────
    public partial class PartSyncSource
    {
        [System.Text.Json.Serialization.JsonPropertyName("partId")]
        public int PartId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("partName")]
        public string? PartName { get; set; }
    }

    public partial class PartSyncSourceListResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public System.Collections.Generic.List<string>? Messages { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public System.Collections.Generic.List<PartSyncSource>? Data { get; set; }
    }

    public partial class FT14SyncResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("inserted")]
        public int Inserted { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("updated")]
        public int Updated { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("skipped")]
        public int Skipped { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("failed")]
        public int Failed { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public System.Collections.Generic.List<string>? Messages { get; set; }
    }

    public partial class FT14SyncResultResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public System.Collections.Generic.List<string>? Messages { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public FT14SyncResult? Data { get; set; }
    }
}
