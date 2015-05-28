using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using ServiceStack;

namespace Fcs.Model {

    /**
    * @api {post} /promos Post Promo
    * @apiName PostPromo
    * @apiGroup Promo
    * 
    * @apiSuccess {Guid}      id                Promo Id.
    * @apiSuccess {ReferenceId} referenceId     Promo Id from reference system.
    * @apiSuccess {string}    name              Promo name.
    * @apiSuccess {DateTime}  effectiveDate     Effective date.
    * @apiSuccess {DateTime}  expirationDate    Expiration date.
    * @apiSuccess {decimal}   maxDiscountAmount Max discount amount.
    * @apiSuccess {int}       maxTimesAllowed   Max times allowed.
    * @apiSuccess {bool}      inactive          Inactive promo.
    */

    [Route("/promos", "POST")]
    public class Promo : IReturn<Promo> {
        public Guid? Id { get; set; }
        public string ReferenceId { get; set; }
        public string App { get; set; }
        public string Name { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Decimal? MaxDiscountAmount { get; set; }
        public int? MaxTimesAllowed { get; set; }
        public bool Inactive { get; set; }
    }

    /**
    * @api {post} /promos/code Post Promo Code
    * @apiName PostPromoCode
    * @apiGroup Promo
    * 
    * @apiSuccess {Guid}      id                Promo Code Id.
    * @apiSuccess {ReferenceId} referenceId     Promo Code Id from reference system.
    * @apiSuccess {string}    code              The Code a user enters to get the promo deal.
    * @apiSuccess {Guid}      promoId           The Promo Id this code belongs to.
    * @apiSuccess {string}    description       Promo Code description.
    * @apiSuccess {string}    promoCodeType     The Promo Code type.
    * @apiSuccess {DateTime}  effectiveDate     Effective date.
    * @apiSuccess {DateTime}  expirationDate    Expiration date.
    * @apiSuccess {decimal}   maxDiscountAmount Max discount amount.
    * @apiSuccess {int}       maxTimesAllowed   Max times allowed.
    * @apiSuccess {bool}      inactive          Whether the code is inactive.
    */

    [Route("/promos/code", "POST")]
    public class PromoCode : IReturn<PromoCode> {
        public Guid? Id { get; set; }
        public string ReferenceId { get; set; }
        public string Code { get; set; }
        public Guid? PromoId { get; set; }
        public string Description { get; set; }
        public string PromoCodeType { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Decimal? MaxDiscountAmount { get; set; }
        public int? MaxTimesAllowed { get; set; }
        public Decimal? DiscountPercent { get; set; }
        public Decimal? DiscountAmount { get; set; }
        public List<PromoCodeProduct> Products { get; set; } 
        public bool Inactive { get; set; }
    }

    public class PromoCodeProduct : IReturn<PromoCodeProduct> {
        public Guid? Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? PromoCodeId { get; set; }
        public Decimal? DiscountPercent { get; set; }
        public Decimal? DiscountAmount { get; set; }
        public bool Inactive { get; set; }
    }

    /**
    * @api {post} /promos/code/validate         Validate a Promo Code
    * @apiName ValidatePromoCode
    * @apiGroup Promo
    * 
    * @apiSuccess {string}    code              The Code a user enters to get the promo deal.
    */
    [Route("/promos/code/validate", "POST")]
    public class PromoCodeValidation : IReturn<ValidatePromoCodeResponse> {
        public string Code { get; set; }
    }

    public class ValidatePromoCodeResponse {
        public bool Valid { get; set; }
        public string Message { get; set; }
        public Guid? PromoCodeId { get; set; }
    }
}