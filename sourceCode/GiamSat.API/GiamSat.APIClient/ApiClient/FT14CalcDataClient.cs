#nullable enable

namespace GiamSat.APIClient
{
    // DTO for a single row returned by the calc-data endpoint
    public partial class AutoSandingTestRow
    {
        [System.Text.Json.Serialization.JsonPropertyName("rowIndex")]
        public int RowIndex { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("fre1")]
        public double Fre1 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("beltRotationRpm")]
        public double BeltRotationRpm { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("fre2")]
        public double Fre2 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("stiffnessY")]
        public double StiffnessY { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("freqDiff")]
        public double FreqDiff { get; set; }
    }

    // Result wrapper for List<AutoSandingTestRow>
    public partial class AutoSandingTestRowListResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public System.Collections.Generic.ICollection<string>? Messages { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public System.Collections.Generic.ICollection<AutoSandingTestRow>? Data { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.7.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial interface IFT14CalcDataClient : IApiService
    {
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<AutoSandingTestRowListResult> GetCalcDataAsync(
            string? part,
            string? work,
            double? offsetFre1,
            double? offsetFre2,
            double? motorFrom,
            double? motorTo,
            double? motorStep);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<AutoSandingTestRowListResult> GetCalcDataAsync(
            string? part,
            string? work,
            double? offsetFre1,
            double? offsetFre2,
            double? motorFrom,
            double? motorTo,
            double? motorStep,
            System.Threading.CancellationToken cancellationToken);
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.7.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class FT14CalcDataClient : IFT14CalcDataClient
    {
        private System.Net.Http.HttpClient _httpClient;
        private static System.Lazy<System.Text.Json.JsonSerializerOptions> _settings = new System.Lazy<System.Text.Json.JsonSerializerOptions>(CreateSerializerSettings, true);

    #pragma warning disable CS8618
        public FT14CalcDataClient(System.Net.Http.HttpClient httpClient)
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

        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual System.Threading.Tasks.Task<AutoSandingTestRowListResult> GetCalcDataAsync(
            string? part, string? work,
            double? offsetFre1, double? offsetFre2,
            double? motorFrom, double? motorTo, double? motorStep)
        {
            return GetCalcDataAsync(part, work, offsetFre1, offsetFre2, motorFrom, motorTo, motorStep, System.Threading.CancellationToken.None);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<AutoSandingTestRowListResult> GetCalcDataAsync(
            string? part, string? work,
            double? offsetFre1, double? offsetFre2,
            double? motorFrom, double? motorTo, double? motorStep,
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

                    // Operation Path: "api/FT14/calcdata"
                    urlBuilder_.Append("api/FT14/calcdata");
                    urlBuilder_.Append('?');

                    if (part != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("part")).Append('=').Append(System.Uri.EscapeDataString(ConvertToString(part, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (work != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("work")).Append('=').Append(System.Uri.EscapeDataString(ConvertToString(work, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (offsetFre1 != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("offsetFre1")).Append('=').Append(System.Uri.EscapeDataString(ConvertToString(offsetFre1, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (offsetFre2 != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("offsetFre2")).Append('=').Append(System.Uri.EscapeDataString(ConvertToString(offsetFre2, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (motorFrom != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("motorFrom")).Append('=').Append(System.Uri.EscapeDataString(ConvertToString(motorFrom, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (motorTo != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("motorTo")).Append('=').Append(System.Uri.EscapeDataString(ConvertToString(motorTo, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (motorStep != null)
                    {
                        urlBuilder_.Append(System.Uri.EscapeDataString("motorStep")).Append('=').Append(System.Uri.EscapeDataString(ConvertToString(motorStep, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }

                    // trim trailing '&' or '?'
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
                            var objectResponse_ = await ReadObjectResponseAsync<AutoSandingTestRowListResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
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
            {
                return new ObjectResponseResult<T>(default(T)!, string.Empty);
            }

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
            if (value == null)
            {
                return "";
            }

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
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[])value);
            }
            else if (value is string[])
            {
                return string.Join(",", (string[])value);
            }
            else if (value.GetType().IsArray)
            {
                var valueArray = (System.Array)value;
                var valueTextArray = new string[valueArray.Length];
                for (var i = 0; i < valueArray.Length; i++)
                {
                    valueTextArray[i] = ConvertToString(valueArray.GetValue(i), cultureInfo);
                }
                return string.Join(",", valueTextArray);
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }
}
