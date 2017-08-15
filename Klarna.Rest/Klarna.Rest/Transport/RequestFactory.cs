#region Copyright Header
//-----------------------------------------------------------------------
// <copyright file="RequestFactory.cs" company="Klarna AB">
//     Copyright 2014 Klarna AB
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
#endregion
namespace Klarna.Rest.Transport
{
    using System.IO;
    using System.Net;
    using System.Text;
    using Klarna.Rest.Models;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// The HTTP request factory interface.
    /// </summary>
    internal class RequestFactory : IRequestFactory
    {
/// <summary>
        /// Creates a HttpWebRequest object.
        /// </summary>
        /// <param name="url">request url</param>
        /// <returns>a HTTP request object</returns>
        public HttpWebRequest CreateRequest(string url)
        {

            // Create the request with correct method to use
            var request = (HttpWebRequest)WebRequest.Create(url);

            return request;
        }

        /// <summary>
        /// Performs a HTTP request.
        /// </summary>
        /// <param name="request">the HTTP request to send</param>
        /// <param name="payload">the payload to send if this is a POST or a PATCH</param>
        /// <returns>the response</returns>
        public async Task<IResponse> Send(HttpWebRequest request, string payload)
        {

            var content = string.IsNullOrEmpty(payload)
                ? null
                : Encoding.UTF8.GetBytes(payload);

            if (content?.Length>0)
            {
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    requestStream.Write(content, 0, content.Length);
                }
            }

            HttpWebResponse webResponse = null;

            try
            {
                using (webResponse = (HttpWebResponse)await request.GetResponseAsync())
                {
                    return new Response(webResponse.StatusCode, webResponse.Headers, this.Json(webResponse));
                }
            }
            catch (WebException ex)
            {
                webResponse = (HttpWebResponse)ex.Response;

                if (webResponse == null || webResponse.ContentLength == 0 || !webResponse.ContentType.Contains("json"))
                {
                    throw;
                }

                ErrorMessage errorMessage = null;
                Response response = new Response(webResponse.StatusCode, webResponse.Headers, this.Json(webResponse));

                try
                {
                    errorMessage = response.Data<ErrorMessage>();
                }
                catch (JsonException)
                {
                    throw ex;
                }

                throw new ApiException(ex.Message, webResponse.StatusCode, errorMessage, ex);
            }
        }

        /// <summary>
        /// Gets the JSON response.
        /// </summary>
        /// <param name="response">the HTTP response</param>
        /// <returns>the JSON string</returns>
        private string Json(HttpWebResponse response)
        {
            Stream webStream = response.GetResponseStream();
            StreamReader responseReader = new StreamReader(webStream, Encoding.UTF8);
            return responseReader.ReadToEnd();
        }
    }
}
