using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GiamSat.APIClient
{
    public partial class TemperatureDataClient
    {
        public virtual Task<ICollection<FT12_TemperatureDatalog>> GetDataLogsAsync(DateTime? fromDate, DateTime? toDate)
        {
            return GetDataLogsAsync(fromDate, toDate, CancellationToken.None);
        }

        public virtual async Task<ICollection<FT12_TemperatureDatalog>> GetDataLogsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
        {
            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new HttpRequestMessage())
                {
                    request_.Method = new HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                    var urlBuilder_ = new StringBuilder();

                    // Operation Path: "api/TemperatureData/GetDataLogs"
                    urlBuilder_.Append("api/TemperatureData/GetDataLogs");
                    urlBuilder_.Append('?');
                    if (fromDate != null)
                    {
                        urlBuilder_.Append(Uri.EscapeDataString("fromDate")).Append('=').Append(Uri.EscapeDataString(fromDate.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    if (toDate != null)
                    {
                        urlBuilder_.Append(Uri.EscapeDataString("toDate")).Append('=').Append(Uri.EscapeDataString(toDate.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                    }
                    urlBuilder_.Length--;

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = new Dictionary<string, IEnumerable<string>>();
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
                            var objectResponse_ = await ReadObjectResponseAsync<ICollection<FT12_TemperatureDatalog>>(response_, headers_, cancellationToken).ConfigureAwait(false);
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
    }
}
