#nullable enable

namespace GiamSat.APIClient
{
    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.7.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial interface IFT16ReportClient : IApiService
    {
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FT16SandingLogDataListResult> GetReportAsync(
            System.DateTime? from,
            System.DateTime? to,
            int? mode);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FT16SandingLogDataListResult> GetReportAsync(
            System.DateTime? from,
            System.DateTime? to,
            int? mode,
            System.Threading.CancellationToken cancellationToken);
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.7.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class FT16ReportClient : IFT16ReportClient
    {
        private System.Net.Http.HttpClient _httpClient;
        private static System.Lazy<System.Text.Json.JsonSerializerOptions> _settings = new System.Lazy<System.Text.Json.JsonSerializerOptions>(CreateSerializerSettings, true);

#pragma warning disable CS8618
        public FT16ReportClient(System.Net.Http.HttpClient httpClient)
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
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        public virtual System.Threading.Tasks.Task<FT16SandingLogDataListResult> GetReportAsync(
            System.DateTime? from, System.DateTime? to, int? mode)
        {
            return GetReportAsync(from, to, mode, System.Threading.CancellationToken.None);
        }

        public virtual async System.Threading.Tasks.Task<FT16SandingLogDataListResult> GetReportAsync(
            System.DateTime? from, System.DateTime? to, int? mode,
            System.Threading.CancellationToken cancellationToken)
        {
            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    var urlBuilder_ = new System.Text.StringBuilder();
                    urlBuilder_.Append("api/FT16/report");
                    urlBuilder_.Append('?');

                    if (from != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("from")).Append('=')
                            .Append(System.Uri.EscapeDataString(from.Value.ToString("o", System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (to != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("to")).Append('=')
                            .Append(System.Uri.EscapeDataString(to.Value.ToString("o", System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (mode != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("mode")).Append('=')
                            .Append(System.Uri.EscapeDataString(mode.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }

                    if (urlBuilder_.Length > 0 && urlBuilder_[urlBuilder_.Length - 1] == '&')
                        urlBuilder_.Length--;
                    if (urlBuilder_.Length > 0 && urlBuilder_[urlBuilder_.Length - 1] == '?')
                        urlBuilder_.Length--;

                    PrepareRequest(client_, request_, urlBuilder_);
                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);
                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
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

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<FT16SandingLogDataListResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
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
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
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

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = System.Text.Json.JsonSerializer.Deserialize<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody!, responseText);
                }
                catch (System.Text.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
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

        private string ConvertToString(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null) return "";
            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                            return attribute.Value != null ? attribute.Value : name;
                    }
                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            else if (value is byte[])
                return System.Convert.ToBase64String((byte[])value);
            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }

    // FT16SandingLogData DTO và Result wrapper
    public partial class FT16SandingLogData
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public System.Guid Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
        public System.DateTime? CreatedAt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("createdMachine")]
        public string? CreatedMachine { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("part")]
        public string? Part { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("work")]
        public string? Work { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("formula")]
        public int? Formula { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("logType")]
        public int? LogType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("shaftNum")]
        public int? ShaftNum { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("motorSandingSpeed")]
        public double? MotorSandingSpeed { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("spineA")]
        public double? SpineA { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("spineB")]
        public double? SpineB { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("spineTarget")]
        public double? SpineTarget { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("spine_Low")]
        public double? Spine_Low { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("spine_Hight")]
        public double? Spine_Hight { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("oK_NG_SpineB")]
        public int? OK_NG_SpineB { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tipOdLength_1")]
        public string? TipOdLength_1 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tipOdLength_2")]
        public string? TipOdLength_2 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tipOdLength_3")]
        public string? TipOdLength_3 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_Reading_1")]
        public double? Diam_Reading_1 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_Reading_2")]
        public double? Diam_Reading_2 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_Reading_3")]
        public double? Diam_Reading_3 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("oK_NG_OD_1")]
        public int? OK_NG_OD_1 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("oK_NG_OD_2")]
        public int? OK_NG_OD_2 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("oK_NG_OD_3")]
        public int? OK_NG_OD_3 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_LL_1")]
        public int? Diam_LL_1 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_LL_2")]
        public int? Diam_LL_2 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_LL_3")]
        public int? Diam_LL_3 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_UL_1")]
        public int? Diam_UL_1 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_UL_2")]
        public int? Diam_UL_2 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("diam_UL_3")]
        public int? Diam_UL_3 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("sandingMode")]
        public int? SandingMode { get; set; }
    }

    public partial class FT16SandingLogDataListResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public System.Collections.Generic.List<string>? Messages { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public System.Collections.Generic.List<FT16SandingLogData>? Data { get; set; }
    }
}
