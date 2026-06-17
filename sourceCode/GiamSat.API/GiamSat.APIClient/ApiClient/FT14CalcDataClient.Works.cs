#nullable enable

namespace GiamSat.APIClient
{
    // Mở rộng IFT14CalcDataClient / FT14CalcDataClient: lấy danh sách Work theo Part.
    // Tách file partial để không đụng phần NSwag-style trong FT14CalcDataClient.cs.
    public partial interface IFT14CalcDataClient
    {
        System.Threading.Tasks.Task<PartWorksResult> GetWorksAsync(string? part);
        System.Threading.Tasks.Task<PartWorksResult> GetWorksAsync(string? part, System.Threading.CancellationToken cancellationToken);
    }

    public partial class FT14CalcDataClient
    {
        public virtual System.Threading.Tasks.Task<PartWorksResult> GetWorksAsync(string? part)
        {
            return GetWorksAsync(part, System.Threading.CancellationToken.None);
        }

        public virtual async System.Threading.Tasks.Task<PartWorksResult> GetWorksAsync(
            string? part, System.Threading.CancellationToken cancellationToken)
        {
            using (var request_ = new System.Net.Http.HttpRequestMessage())
            {
                request_.Method = new System.Net.Http.HttpMethod("GET");
                request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                var urlBuilder_ = new System.Text.StringBuilder();
                urlBuilder_.Append("api/FT14/works");
                urlBuilder_.Append('?');
                if (part != null)
                {
                    urlBuilder_.Append(System.Uri.EscapeDataString("part")).Append('=')
                        .Append(System.Uri.EscapeDataString(ConvertToString(part, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                }
                if (urlBuilder_.Length > 0 && urlBuilder_[urlBuilder_.Length - 1] == '&')
                    urlBuilder_.Length--;
                if (urlBuilder_.Length > 0 && urlBuilder_[urlBuilder_.Length - 1] == '?')
                    urlBuilder_.Length--;

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);
                PrepareRequest(_httpClient, request_, url_);

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
                        var objectResponse_ = await ReadObjectResponseAsync<PartWorksResult>(response_, headers_, cancellationToken).ConfigureAwait(false);
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
        }
    }

    // DTO + Result wrapper cho danh sách work theo part
    public partial class PartWorks
    {
        [System.Text.Json.Serialization.JsonPropertyName("freWorks")]
        public System.Collections.Generic.List<string>? FreWorks { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("spineWorks")]
        public System.Collections.Generic.List<string>? SpineWorks { get; set; }
    }

    public partial class PartWorksResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("succeeded")]
        public bool Succeeded { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public System.Collections.Generic.List<string>? Messages { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public PartWorks? Data { get; set; }
    }
}
