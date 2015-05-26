// Copyright © 2010-2014 Firebrand Technologies

using System.Collections.Generic;
using ServiceStack;

namespace Fcs.Model {
    public class CountryCode {
        public string Source { get; set; }
        public string Code { get; set; }
        public string Desc { get; set; }
    }

    public class Countries {
        public string Source { get; set; }
        public List<CountryCode> CountryCodes { get; set; }
    }


    // TODO: REVIEW AD: This is not a good usage of REST URLs.  /cart/true & /cart/false
    //                  Also, the service should not specify whether to refresh or not
    //                  I don't think this get's used.  Please remove this property and
    //                  All references to it...
    [Route("/cart", "GET", Summary = "Get Cart")]
    [Route("/cart/{refresh}", "GET", Summary = "Provide For a boolean to force non-cached cart get")]
    public class CartRequest {
        public bool? RefreshCart { get; set; }
    }

    [Route("/cart/checkout", "GET", Summary = "Checkout")]
    public class CartCheckout {}

    [Route("/cart/finalizeCart", "PUT", Summary = "Finalize Cart Inputs")]
    public class CartFinalizeCartRequest {
        public ShippingOption ShippingCode { get; set; }
        public string PromotionalCode { get; set; }
    }

    [Route("/cart", "PUT", Summary = "UpdateItem")]
    [Route("/cart/{ean}", "DELETE", Summary = "Delete from Cart")]
    [Route("/cart/{ean}", "Put", Summary = "Add to Cart")]
    public class CartItemRequest {
        public string Id { get; set; }
        public string Ean { get; set; }
        public int? Quantity { get; set; }
    }

    // TODO: REVIEW AD: The Cart Property should be removed as it is not used and
    //                  it is of no use in a GET call.  Please change the Route to
    //                  /cart/payment/location since it is a redirect request.
    [Route("/cart/payment/location", "GET", Summary = "Make Payment")]
    public class CartMakePayment {}

    // TODO: REVIEW AD: Can this be a POST?  I know we are getting this from the
    //                  Payment page.  If it can let's make it POST.  
    [Route("/cart/finalizeorder", "GET", Summary = "Receive PLP - Finalize Order")]
    public class CartFinalizeOrder {
        public string Mfcust { get; set; }
        public string Mforder { get; set; }
        public string Mftype { get; set; }
        public string ApprovalText { get; set; }
        public string ApprovalCode { get; set; }
        public string Cust { get; set; }
        public string Amount { get; set; }
        public string Mfseqn { get; set; }
        public string Mfrref { get; set; }
        public string Mfrtxt { get; set; }
        public string Mfrcvv { get; set; }
        public string Mfatal { get; set; }
        public string Mfukey { get; set; }
        public string MFRTRN_response_code { get; set; }
    }

    [Route("/cart/options", "PUT", Summary = "Finalize Cart Inputs")]
    public class Cart {
        public int ItemCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalShipping { get; set; }
        public IEnumerable<CartItem> Items { get; set; }
        public List<ShippingOption> ShippingOptions { get; set; }
        public string ShippingCode { get; set; }
        public string Continue { get; set; }
        public string ExternalProcessNumber { get; set; }
        public string ExternalContactId { get; set; }
        public string PromotionalCode { get; set; }
        public string ShippingMethod { get; set; }
        public string CustomerReference { get; set; }
    }

    public class CartItem {
        public string Id { get; set; }
        public string Ean { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string ThumbUrl { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public string Format { get; set; }
        public string Group { get; set; }
    }

    public class ShippingOptions {
        public List<ShippingOption> Options { get; set; }
        public string DefaultCode { get; set; }
        //public string SelectedDesc { get; set; }
        //public string SelectedCode { get; set; }
        //public decimal SelectedCost { get; set; }
    }

    public class ShippingOption {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Carrier { get; set; }
    }
}