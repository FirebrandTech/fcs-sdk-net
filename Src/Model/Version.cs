// Copyright © 2010-2015 Firebrand Technologies

using System;
using ServiceStack;

namespace Fcs.Model {
    /**
    * @api {post} /promos Post Promo
    * @apiName PostPromo
    * @apiGroup Promo
    * 
    * @apiSuccess {Guid}      id                Version Id.
    * @apiSuccess {ReferenceId} domainId        Domain Id
    * @apiSuccess {string}    name              Source Name.
    * @apiSuccess {string}    number            Source Number.
    */

    [Route("/versions", "POST")]
    public class VersionDto : IReturn<VersionDto> {
        public Guid? Id { get; set; }
        public Guid? DomainId { get; set; }
        public string DomainAccessKey { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
    }
}