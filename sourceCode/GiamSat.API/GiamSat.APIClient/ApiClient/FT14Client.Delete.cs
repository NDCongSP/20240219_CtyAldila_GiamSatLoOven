#nullable enable

namespace GiamSat.APIClient
{
    /// <summary>
    /// Bổ sung DELETE cho FT14 — xóa cứng Part khỏi database.
    /// Viết tay (partial mở rộng IFT14Client/FT14Client do NSwag sinh) để không phải regenerate GiamSatApi.cs.
    /// Dùng chung _httpClient + các helper (ConvertToString, ReadObjectResponseAsync, PrepareRequest...) của FT14Client.
    /// </summary>
    public partial interface IFT14Client
    {
        System.Threading.Tasks.Task<BooleanResult> DeleteAsync(System.Guid id);
        System.Threading.Tasks.Task<BooleanResult> DeleteAsync(System.Guid id, System.Threading.CancellationToken cancellationToken);
    }

    public partial class FT14Client
    {
        public virtual System.Threading.Tasks.Task<BooleanResult> DeleteAsync(System.Guid id)
        {
            return DeleteAsync(id, System.Threading.CancellationToken.None);
        }

        public virtual async System.Threading.Tasks.Task<BooleanResult> DeleteAsync(System.Guid id, System.Threading.CancellationToken cancellationToken)
        {
            var client_ = _httpClient;
            using (var request_ = new System.Net.Http.HttpRequestMessage())
            {
                request_.Method = new System.Net.Http.HttpMethod("DELETE");
                request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                // Operation Path: "api/FT14/{id}"
                var urlBuilder_ = new System.Text.StringBuilder();
                urlBuilder_.Append("api/FT14/");
                urlBuilder_.Append(System.Uri.EscapeDataString(ConvertToString(id, System.Globalization.CultureInfo.InvariantCulture)));

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);
                PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
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
                        var objectResponse_ = await ReadObjectResponseAsync<BooleanResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
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
                    response_.Dispose();
                }
            }
        }
    }

    /// <summary>Result wrapper cho Result&lt;bool&gt; trả về từ API.</summary>
    public partial class BooleanResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public System.Collections.Generic.List<string>? Messages { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public bool Data { get; set; }
    }
}
