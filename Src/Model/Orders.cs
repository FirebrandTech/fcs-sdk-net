// Copyright © 2010-2014 Firebrand Technologies

using System;
using System.Collections.Generic;
using Cloud.Api.V2.Model;
using ServiceStack;

namespace Fcs.Model {
    [Route("/orders", "GET", Summary = "Get Orders")]
    public class OrdersRequest {}

    /**
     * @api {post} /orders Post a new order.
     * @apiName PostOrders
     * @apiGroup Orders
     * @apiParam {string}      user                 Unique information about the user.  The user.email value is required.
     * @apiParam {Object[]}    items                The items on the order.
     * @apiParam {string}      [referenceId]        Optional order identifier used by the client.
     * @apiParam {bool}        [throwErrors]        Defaults to false.  If true, user accounts will not be automatically created for unrecognized email addresses.
     * 
     * @apiSuccess {Guid}       orderId             Order Id.
     * @apiSuccess {Object}     user                User email address and Id.
     * @apiSuccess {Object[]}   items               Each item that was successfully added to the user's library.
     * 
     * @apiParamExample Create-Order-Example:
     * {
     *     "user": {
     *         "email": "username@example.com"
     *     },
     *     "items": [
     *         {
     *             "ean": "9781451686595",
     *             "title": "Test Title",
     *             "author": "Test Author",
     *             "price": "10.5",
     *             "quantity": "1"
     *         }
     *     ]
     * }
     * 
     * @apiSuccessExample Success-Response:
     * HTTP/1.1 200 OK
     * {
     *     "orderId": "161cc017-68b0-49b5-aa0d-a403015708f5",
     *     "user": {
     *         "id": "87d5e698-2d1a-4b29-b871-a3fc015d44b3",
     *         "email": "username@example.com"
     *     },
     *     "items": [
     *         {
     *             "id": "a36418fb-0d19-4c58-9337-a40301570904",
     *             "ean": "9781451686595",
     *             "title": "Test Title",
     *             "author": "Test Author",
     *             "price": 10.5,
     *             "subTotal": 0,
     *             "quantity": 1
     *         }
     *     ]
     * }
     * 
     * @apiErrorExample Error-Response:
     * {
     *     "responseStatus": {
     *         "errorCode": "ArgumentException",
     *         "message": "UserNotFound: Requested User could not be found",
     *         "stackTrace": "...",
     *         "errors": []
     *     }
     * }
     */

    [Route("/orders", "POST", Summary = "Create an Order")]
    public class Order : IReturn<Order> {
        public Guid? Id { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Shipping { get; set; }
        public IEnumerable<OrderItem> Items { get; set; }
        public ShippingOption Delivery { get; set; }
        public string Status { get; set; }
        public string ReferenceId { get; set; }
        public string CustomerReferenceId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Source { get; set; }
        public User User { get; set; }
        public string UserName { get; set; }
        public string DiscountCode { get; set; }
        public string TrackingNumber { get; set; }
    }

    [Route("/orders/{id}", "GET", Summary = "Get Order")]
    public class OrderRequest {
        public string Id { get; set; }
    }

    public class OrderItem {
        public Guid? Id { get; set; }
        public string Ean { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public string ThumbUrl { get; set; }
        public int Quantity { get; set; }
    }

    public class FreightItem {
        public string Item { get; set; }
        public decimal FreightCost { get; set; }
    }

    public class OrderAction {
        public string Description { get; set; }
        public string Code { get; set; }
        public DateTime DateTime { get; set; }
    }
}