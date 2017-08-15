#region Copyright Header
//-----------------------------------------------------------------------
// <copyright file="CheckoutOrderComponentTests.cs" company="Klarna AB">
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
namespace Klarna.Rest.Tests.Checkout
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using Klarna.Rest.Models;
    using Klarna.Rest.Transport;
    using NUnit.Framework;
    using Rhino.Mocks;

    /// <summary>
    /// Component tests of the order class.
    /// </summary>
    [TestFixture]
    public class CheckoutOrderComponentTests
    {
        #region Private Fields

        /// <summary>
        /// The merchant id.
        /// </summary>
        private string merchantId = "1234";

        /// <summary>
        /// The shared secret.
        /// </summary>
        private string secret = "MySecret";

        /// <summary>
        /// The path.
        /// </summary>
        private string path = "/checkout/v3/orders";

        /// <summary>
        /// The base url.
        /// </summary>
        private Uri baseUrl = new Uri("https://dummytesturi.test");

        /// <summary>
        /// The HTTP request.
        /// </summary>
        private HttpWebRequest httpWebRequest;

        /// <summary>
        /// The request factory.
        /// </summary>
        private IRequestFactory requestMock;

        /// <summary>
        /// The connector.
        /// </summary>
        private IConnector connector;

        /// <summary>
        /// The order.
        /// </summary>
        private Klarna.Rest.Checkout.CheckoutOrder checkoutOrder;

        /// <summary>
        /// A location.
        /// </summary>
        private string location = "https://somelocation.test/";

        #endregion

        #region Set Up

        /// <summary>
        /// The set up before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.httpWebRequest = (HttpWebRequest)WebRequest.Create(this.baseUrl);
            this.requestMock = MockRepository.GenerateStub<IRequestFactory>();

            this.connector = ConnectorFactory.Create(this.requestMock, this.merchantId, this.secret, this.baseUrl);
            this.checkoutOrder = new Klarna.Rest.Checkout.CheckoutOrder(this.connector, null);
        }

        #endregion

        #region Tests

        /// <summary>
        /// Make sure that the request sent and retrieved data is correct when fetching the order.
        /// </summary>
        [Test]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueMustBeDocumented", Justification = "Reviewed.")]
        public async Task TestCreate()
        {
            // Arrange
            CheckoutOrderData orderData = TestsHelper.GetCheckoutOrderData1();
            var orderDataJson = orderData.ConvertToJson();
            this.requestMock.Expect(x => x.CreateRequest(this.baseUrl + this.path.TrimStart('/'))).Return(this.httpWebRequest);
            WebHeaderCollection headers = new WebHeaderCollection();
            headers["Location"] = this.location;
            HttpStatusCode status = HttpStatusCode.Created;
            Task<IResponse> response = Task.FromResult<IResponse>(new Response(status, headers, string.Empty));
            this.requestMock.Expect(x => x.Send(this.httpWebRequest, orderDataJson)).Return(response);

            // Act
            await this.checkoutOrder.Create(orderData);

            // Assert
            this.requestMock.VerifyAllExpectations();
            Assert.AreEqual(this.location, this.checkoutOrder.Location.OriginalString);
            TestsHelper.AssertRequest(this.merchantId, this.secret, this.httpWebRequest, HttpMethod.Post);
        }

        /// <summary>
        /// Make sure that the request sent is correct and that the updated data is accessible.
        /// </summary>
        [Test]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueMustBeDocumented", Justification = "Reviewed.")]
        public async Task TestUpdate()
        {
            // Arrange
            CheckoutOrderData orderData1 = TestsHelper.GetCheckoutOrderData1();
            CheckoutOrderData orderData2 = TestsHelper.GetCheckoutOrderData2();
            var order1Json = orderData1.ConvertToJson();
            var order2Json = orderData2.ConvertToJson();
            
            this.requestMock.Expect(x => x.CreateRequest(this.baseUrl + this.path.TrimStart('/'))).Return(this.httpWebRequest);
            WebHeaderCollection headers = new WebHeaderCollection();
            headers[HttpResponseHeader.Location] = this.location;

            Task<IResponse> response = Task.FromResult<IResponse>(new Response(HttpStatusCode.Created, headers, string.Empty));
            this.requestMock.Expect(x => x.Send(this.httpWebRequest, order1Json)).Return(response);

            this.httpWebRequest = (HttpWebRequest)WebRequest.Create(this.baseUrl);
            this.requestMock.Expect(x => x.CreateRequest(this.baseUrl + this.location)).Return(this.httpWebRequest);

            WebHeaderCollection headers2 = new WebHeaderCollection();
            headers2[HttpResponseHeader.ContentType] = "application/json";

            Task<IResponse> response2 = Task.FromResult<IResponse>(new Response(HttpStatusCode.OK, headers2, orderData2.ConvertToJson()));
            this.requestMock.Expect(x => x.Send(this.httpWebRequest, order2Json)).Return(response2);

            // Act
            await this.checkoutOrder.Create(orderData1);
            CheckoutOrderData updatedCheckoutOrderData = await this.checkoutOrder.Update(orderData2);

            // Assert
            this.requestMock.VerifyAllExpectations();
            Assert.AreEqual(this.location, this.checkoutOrder.Location.OriginalString);
            Assert.AreEqual(orderData2.PurchaseCountry, updatedCheckoutOrderData.PurchaseCountry);
            Assert.AreEqual(orderData2.PurchaseCurrency, updatedCheckoutOrderData.PurchaseCurrency);
            TestsHelper.AssertRequest(this.merchantId, this.secret, this.httpWebRequest, HttpMethod.Post);
        }

        /// <summary>
        /// Make sure that the request sent and retrieved data is correct.
        /// </summary>
        [Test]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueMustBeDocumented", Justification = "Reviewed.")]
        public async Task TestFetch()
        {
            // Arrange
            string orderId = "0003";
            string expectedUrl = "https://dummytesturi.test/checkout/v3/orders/0003";
            int orderAmount = 1234;
            this.requestMock.Expect(x => x.CreateRequest(expectedUrl)).Return(this.httpWebRequest);

            string json = "{\r\n  \"order_id\": \"" + orderId + "\",\r\n  \"order_amount\": " + orderAmount + ",\r\n }";
            WebHeaderCollection headers = new WebHeaderCollection();
            headers[HttpResponseHeader.ContentType] = "application/json";

            Task<IResponse> response = Task.FromResult((IResponse)new Response(HttpStatusCode.OK, headers, json));
            this.requestMock.Expect(x => x.Send(this.httpWebRequest, string.Empty)).Return(response);

            // Act
            this.checkoutOrder = new Klarna.Rest.Checkout.CheckoutOrder(this.connector, orderId);
            CheckoutOrderData checkoutOrderData = await this.checkoutOrder.Fetch();

            // Assert
            this.requestMock.VerifyAllExpectations();
            Assert.AreEqual(orderId, checkoutOrderData.OrderId);
            Assert.AreEqual(orderAmount, checkoutOrderData.OrderAmount);
            TestsHelper.AssertRequest(this.merchantId, this.secret, this.httpWebRequest, HttpMethod.Get);
        }

        #endregion
    }
}
