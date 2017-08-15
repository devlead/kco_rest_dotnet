#region Copyright Header
//-----------------------------------------------------------------------
// <copyright file="ConnectorTests.cs" company="Klarna AB">
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
namespace Klarna.Rest.Tests.Transport
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Klarna.Rest.Transport;
    using NUnit.Framework;
    using Rhino.Mocks;

    /// <summary>
    /// Tests the Connector class.
    /// </summary>
    [TestFixture]
    public class ConnectorTests
    {
        #region Private Fields

        /// <summary>
        /// The connector.
        /// </summary>
        private IConnector connector;

        /// <summary>
        /// The request factory mock.
        /// </summary>
        private IRequestFactory requestMock;

        /// <summary>
        /// The merchant id.
        /// </summary>
        private string merchantId;

        /// <summary>
        /// the shared secret.
        /// </summary>
        private string secret;

        /// <summary>
        /// The base url.
        /// </summary>
        private Uri baseUrl;

        #endregion

        #region Set Up

        /// <summary>
        /// The set up before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.requestMock = MockRepository.GenerateStub<IRequestFactory>();
            this.merchantId = "merchantId";
            this.secret = "secret";
            this.baseUrl = new Uri("https://dummytesturi.test");
            this.connector = ConnectorFactory.Create(this.requestMock, this.merchantId, this.secret, this.baseUrl);
        }

        #endregion

        #region Tests

        /// <summary>
        /// Basic test of CreateRequest.
        /// </summary>
        [Test]
        public void Transport_Connector_CreateRequest_Basic()
        {
            // Arrange
            HttpMethod method = HttpMethod.Post;
            var request = (HttpWebRequest)WebRequest.Create("https://somerandomuri.test");
            this.requestMock.Stub(x => x.CreateRequest(this.baseUrl.ToString())).Return(request);

            // Act
            this.connector.CreateRequest(this.baseUrl.ToString(), HttpMethod.Post);

            // Assert
            TestsHelper.AssertRequest(this.merchantId, this.secret, request, method);
        }

        /// <summary>
        /// Test of CreateRequest with new url.
        /// </summary>
        [Test]
        public void Transport_Connector_CreateRequest_NewUrl()
        {
            // Arrange
            string newUrl = "newUrl/sdf";
            HttpMethod method = HttpMethod.Get;
            var request = (HttpWebRequest)WebRequest.Create(this.baseUrl);
            this.requestMock.Stub(x => x.CreateRequest(this.baseUrl.ToString() + newUrl)).Return(request);

            // Act
            this.connector.CreateRequest(newUrl, method);

            // Assert
            TestsHelper.AssertRequest(this.merchantId, this.secret, request, method);
        }

        /// <summary>
        /// Test of CreateRequest when url starts with base url.
        /// </summary>
        [Test]
        public void Transport_Connector_CreateRequest_UrlStartsWithBaseUrl()
        {
            // Arrange
            string newUrl = this.baseUrl + "newUrl";
            HttpMethod method = HttpMethod.Get;
            var request = (HttpWebRequest)WebRequest.Create("https://somerandomuri.test");
            this.requestMock.Stub(x => x.CreateRequest(newUrl)).Return(request);

            // Act
            this.connector.CreateRequest(newUrl, method);

            // Assert
            TestsHelper.AssertRequest(this.merchantId, this.secret, request, method);
        }

        /// <summary>
        /// Basic test of Send.
        /// </summary>
        [Test]
        public void Transport_Connector_Send_Basic()
        {
            // Arrange
            string payload = "payload";
            var request = (HttpWebRequest)WebRequest.Create("https://somerandomuri.test");
            Task<IResponse> responseValidatorMock = Task.FromResult(MockRepository.GenerateStub<IResponse>());
            this.requestMock.Stub(x => x.Send(request, payload)).Return(responseValidatorMock);

            // Act
            var responseValidator = this.connector.Send(request, payload);

            // Assert
            Assert.AreEqual(responseValidatorMock, responseValidator);
        }

        #endregion
    }
}
