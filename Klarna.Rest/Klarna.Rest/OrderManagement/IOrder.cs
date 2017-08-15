﻿#region Copyright Header
//-----------------------------------------------------------------------
// <copyright file="IOrder.cs" company="Klarna AB">
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
namespace Klarna.Rest.OrderManagement
{
    using Klarna.Rest.Models;
    using Klarna.Rest.Models.Requests;
    using System.Threading.Tasks;

    /// <summary>
    /// Checkout order resource interface.
    /// </summary>
    public interface IOrder : IResource
    {
        /// <summary>
        /// Fetches the order.
        /// </summary>
        /// <returns>the order data object</returns>
        Task<OrderData> Fetch();

        /// <summary>
        /// Acknowledges the order.
        /// </summary>
        Task Acknowledge();

        /// <summary>
        /// Cancels the order.
        /// </summary>
        Task Cancel();

        /// <summary>
        /// Updates the authorization data.
        /// </summary>
        /// <param name="updateAuthorization">the updateAuthorization</param>
        Task UpdateAuthorization(UpdateAuthorization updateAuthorization);

        /// <summary>
        /// Extends the authorization time.
        /// </summary>
        Task ExtendAuthorizationTime();

        /// <summary>
        /// Update the merchant references.
        /// </summary>
        /// <param name="updateMerchantReferences">the update merchant references</param>
        Task UpdateMerchantReferences(UpdateMerchantReferences updateMerchantReferences);

        /// <summary>
        /// Updates the customer details.
        /// </summary>
        /// <param name="updateCustomerDetails">the order</param>
        Task UpdateCustomerDetails(UpdateCustomerDetails updateCustomerDetails);

        /// <summary>
        /// Refunds an amount of a captured order.
        /// </summary>
        /// <param name="order">the order</param>
        Task Refund(Refund order);

        /// <summary>
        /// Release the remaining authorization for an order.
        /// </summary>
        Task ReleaseRemainingAuthorization();
    }
}
