// Copyright © 2010-2014 Firebrand Technologies

using System;
using System.Collections.Generic;
using ServiceStack;

namespace Fcs.Model {

    /**
    * @api {post} /catalogs/:catalogId/categories Post Catalog Category
    * @apiName PostCatalogCategory
    * @apiGroup Catalog
    * @apiParam {Guid}     catalogId      CatalogId.
    * 
    * @apiSuccess {Guid}      id               CatalogCategory Id.
    * @apiSuccess {string}    typeTag          Type tag.
    * @apiSuccess {string}    name             CatalogCategory name.
    * @apiSuccess {string}    label            CatalogCategory label.
    * @apiSuccess {string}    description      CatalogCategory description.
    * @apiSuccess {int}       summaryMode      Summary mode.   
    * @apiSuccess {Guid}      catalogId        Catalog Id.
    * @apiSuccess {Guid}      parentCategoryId Parent category Id.
    * @apiSuccess {string}    displayTypeTag   Display type tag.
    * @apiSuccess {string}    orderTypeTag     Order type tag.
    * @apiSuccess {int}       orderNumber      Order number.
    * @apiSuccess {string}    updatedBy        UpdatedBy name.
    * @apiSuccess {DateTime}  updatedAt        UpdatedAt date.
    */
    [Route("/catalogs/{catalogId}/categories", "POST")]
    public class CatalogCategory {
        public Guid? Id { get; set; }
        public string TypeTag { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int SummaryMode { get; set; }
        public Guid? CatalogId { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string DisplayTypeTag { get; set; }
        public string OrderTypeTag { get; set; }
        public int OrderNumber { get; set; }
        public string ReferenceId { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /**
    * @api {get} /catalogs/categories/:id Get Catalog Category
    * @apiName GetCatalogCategory
    * @apiGroup Catalog
    * @apiParam {Guid}     id      CatalogCategory id.
    * 
    * @apiSuccess {Guid}      id               CatalogCategory Id.
    * @apiSuccess {string}    tag              CatalogCategory Tag.
    * @apiSuccess {string}    typeTag          Type tag.
    * @apiSuccess {Guid}      domainId         Domain Id.
    * @apiSuccess {string}    name             CatalogCategory name.
    * @apiSuccess {string}    label            CatalogCategory label.
    * @apiSuccess {string}    description      CatalogCategory description.
    * @apiSuccess {int}       summaryMode      Summary mode.   
    * @apiSuccess {Guid}      catalogId        Catalog Id.
    * @apiSuccess {Guid}      parentCategoryId Parent category Id.
    * @apiSuccess {string}    displayTypeTag   Display type tag.
    * @apiSuccess {string}    orderTypeTag     Order type tag.
    * @apiSuccess {int}       orderNumber      Order number.
    * @apiSuccess {string}    updatedBy        UpdatedBy name.
    * @apiSuccess {DateTime}  updatedAt        UpdatedAt date.
    */
    [Route("/catalogs/categories/{id}", "GET")]
    public class CatalogCategoryRequest {
        public Guid? Id { get; set; }
    }

    /**
    * @api {post} /catalogs/:catalogId/comments Post Catalog Comment
    * @apiName PostCatalogComment
    * @apiGroup Catalog
    * @apiParam {Guid}     catalogId      Catalog Id.
    * 
    * @apiSuccess {Guid}      id               CatalogComment Id.
    * @apiSuccess {string}    typeTag          Type tag.
    * @apiSuccess {string}    html             Comment html.
    * @apiSuccess {Guid}      catalogId        Catalog Id.
    * @apiSuccess {string}    updatedBy        UpdatedBy name.
    * @apiSuccess {DateTime}  updatedAt        UpdatedAt date.
    */
    [Route("/catalogs/{catalogId}/comments", "POST")]
    public class CatalogComment {
        public Guid? Id { get; set; }
        public string TypeTag { get; set; }
        public string Html { get; set; }
        public Guid? CatalogId { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /**
    * @api {get} /catalogs/comments/:id Get Catalog Comment (or /catalogs/:catalogId/comments/:typeTag Get Catalog Comment by catalog and type)
    * @apiName GetCatalogComment
    * @apiGroup Catalog
    * @apiParam {Guid}     id      CatalogComment id.
    * 
    * @apiSuccess {Guid}      id               CatalogComment Id.
    * @apiSuccess {string}    typeTag          Type tag.
    * @apiSuccess {string}    html             Comment html.
    * @apiSuccess {Guid}      catalogId        Catalog Id.
    * @apiSuccess {string}    updatedBy        UpdatedBy name.
    * @apiSuccess {DateTime}  updatedAt        UpdatedAt date.
    */
    [Route("/catalogs/comments/{id}", "GET")]
    [Route("/catalogs/{catalogId}/comments/{typeTag}", "GET")]
    public class CatalogCommentRequest {
        public Guid? Id { get; set; }
        public Guid? CatalogId { get; set; }
        public string TypeTag { get; set; }
    }

    /**
    * @api {post} /catalogs Post Catalog.
    * @apiName PostCatalog
    * @apiGroup Catalog
    * 
    * @apiSuccess {Guid}      id               Catalog Id.
    * @apiSuccess {Guid}      siteId           Site Id.
    * @apiSuccess {string     applicationId    Application Id.
    * @apiSuccess {string}    name             Site name.
    * @apiSuccess {string}    projectTag       Project tag.
    * @apiSuccess {string}    description      Site description.
    * @apiSuccess {DateTime}  createDate       Date created.
    * @apiSuccess {DateTime}  publishDate      Date published.
    * @apiSuccess {string}    updatedBy        UpdatedBy name.
    * @apiSuccess {DateTime}  updatedAt        UpdatedAt date.
    * @apiSuccess {bool}      active           Active catalog.
    */
    [Route("/catalogs", "POST")]
    public class Catalog : IReturn<Catalog> {
        public Guid? Id { get; set; }
        public Guid? SiteId { get; set; }
        public string ApplicationId { get; set; }
        public string Name { get; set; }
        public string ProjectTag { get; set; }
        public string Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ArchiveDate { get; set; }
        public int PublishStatus { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Active { get; set; }
    }

    /**
    * @api {post} /catalogs/products/:catalogProductId/category/:categoryId Post Catalog Product Category.
    * @apiName PostCatalogProductCategory
    * @apiGroup Catalog
    * @apiParam {Guid}     id      CatalogProductCategory id.
    * 
    * @apiSuccess {Guid}      id               CatalogProduct Id.
    * @apiSuccess {string}    tag              CatalogProduct Tag.
    * @apiSuccess {Guid}      domainId         Domain Id.
    * @apiSuccess {Guid}      catalogproductId Catalog product Id.
    * @apiSuccess {Guid}      categoryId       Category Id.
    * @apiSuccess {string}    updatedBy        UpdatedBy name.
    * @apiSuccess {DateTime}  updatedAt        UpdatedAt date.
    */
    [Route("/catalogs/products/{catalogProductId}/category/{categoryId}", "POST")]
    public class CatalogProductCategory {
        public Guid? Id { get; set; }
        public Guid? CatalogProductId { get; set; }
        public Guid CategoryId { get; set; }
        public int Order { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /**
    * @api {get} /catalogs/products/categories/:id Get Catalog Product Category.
    * @apiName GetCatalogProductCategory
    * @apiGroup Catalog
    * @apiParam {Guid}     id      CatalogProductCategory id.
    * 
    * @apiSuccess {Guid}      id               CatalogProduct Id.
    * @apiSuccess {string}    tag              CatalogProduct Tag.
    * @apiSuccess {Guid}      domainId         Domain Id.
    * @apiSuccess {Guid}      catalogproductId Catalog product Id.
    * @apiSuccess {Guid}      categoryId       Category Id.
    * @apiSuccess {string}    updatedBy        UpdatedBy name.
    * @apiSuccess {DateTime}  updatedAt        UpdatedAt date.
    */
    [Route("/catalogs/products/categories/{id}", "GET")]
    public class CatalogProductCategoryRequest {
        public Guid? Id { get; set; }
    }

    /**
    * @api {post} /catalogs/:catalogId/products/:productId Post Catalog Product.
    * @apiName PostCatalogProduct
    * @apiGroup Catalog
    * @apiParam {Guid}     catalogId      Catalog Id.
    * @apiParam {Guid}     productId      Product Id.
    * 
    * @apiSuccess {Guid}      id               CatalogProduct Id.
    * @apiSuccess {Guid}      catalogId        Catalog Id.
    * @apiSuccess {Guid}      productId        Product Id.
    * @apiSuccess {string}    domain           Domain Name.
    * @apiSuccess {string}    ean              Ean.
    * @apiSuccess {string}    ean13            Ean13.
    * @apiSuccess {string}    title            Title.
    * @apiSuccess {string}    subTitle         SubTitle.
    * @apiSuccess {string}    author           Author Name.
    * @apiSuccess {string}    publisher        Publisher Name.
    * @apiSuccess {string}    imprint          Imprint Name.
    * @apiSuccess {string}    mediaFormat      MediaFormat Name.
    * @apiSuccess {int}       pageCount        Page Count.
    * @apiSuccess {string}    description      Description.
    * @apiSuccess {string}    tableOfContents  Table Of Contents.
    * @apiSuccess {string}    authorInfo       Author Info.
    * @apiSuccess {decimal}   price            Price.
    * @apiSuccess {string}    displayPrice     Price formatted for display.
    * @apiSuccess {DateTime}  releaseDate      Release Date
    * @apiSuccess {string}    coverUrl         Large Cover URL
    * @apiSuccess {string}    thumbUrl         Small Cover URL
    * 
    * @apiSuccessExample Success-Response:
    * HTTP/1.1 200 OK
    * {
    *     "id":"c61aac5a-50d1-42b2-911c-ecd64d8f5ad2",
    *     "productId":"f1a74928-7289-4a4a-8804-a354012da153",
    *     "catalogId":"5bcc220c-160d-41d3-9ba1-a33901394536",
    *     "ean13":"9781493003136",
    *     "ean":"978-1-4930-0313-6",
    *     "author":"Dunbar Hardy",
    *     "imprint":"FalconGuides",
    *     "publisher":"FalconGuides",
    *     "domain":"Harvard","title":"Paddling Colorado",
    *     "subTitle":"A Guide to the State's Best Paddling Routes",
    *     "thumbUrl":"https://fcs-img-debug.s3.amazonaws.com/83fe7068-9cba-4ba8-9114-9fa4014d2a28/e7d78671-53fe-4e8d-bce8-a354012db023/M.jpg",
    *     "coverUrl":"https://fcs-img-debug.s3.amazonaws.com/83fe7068-9cba-4ba8-9114-9fa4014d2a28/e7d78671-53fe-4e8d-bce8-a354012db023/L.jpg",
    *     "description":"Colorado may be a skier’s paradise, but once the snow melts, it makes an abrupt transition into an exciting home for paddlers...",
    *     "authorInfo":"Dunbar Hardy, whose articles and photographs have appeared in National Geographic Adventure, Outside, and Canoe and Kayak, is....",
    *     "releaseDate":"2009-04-21T00:00:00.0000000Z",
    *     "stockQty":0,
    *     "price":13.5000,
    *     "displayPrice":"$13.50",
    *     "mediaFormat":"ePub/NOAMZEP",
    *     "pageCount":176
    * }
    */
    [Route("/catalogs/{catalogId}/products/{productId}", "POST")]
    public class CatalogProductDto {
        public Guid? Id { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? CatalogId { get; set; }
        public string Ean13 { get; set; }
        public string Ean { get; set; }
        public string Author { get; set; }
        public string Imprint { get; set; }
        public string Publisher { get; set; }
        public string Domain { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string ThumbUrl { get; set; }
        public string CoverUrl { get; set; }
        public string Description { get; set; }
        public string TableOfContents { get; set; }
        public string AuthorInfo { get; set; }
        public string Reviews { get; set; }
        public string BisacCategories { get; set; }
        public List<string> RelatedSubjects { get; set; }
        public List<object> RelatedLinks { get; set; }
        public int? PublishMonth { get; set; }
        public int? PublishYear { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? EtaDate { get; set; }
        public decimal StockQty { get; set; }
        public string StockStatusDesc { get; set; }
        public decimal Price { get; set; }
        public string DisplayPrice { get; set; }
        public string MediaFormat { get; set; }
        public int? PageCount { get; set; }
        public string TrimSize { get; set; }
        public string InsertIllustration { get; set; }
        public string DiscountDescription { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /**
    * @api {get} /catalogs/products/:id Get Catalog Product with additional associated data.
    * @apiName GetCatalogProduct
    * @apiGroup Catalog
    * @apiParam {Guid}     id      Product id.
    * @apiParam {string}   ean     Optional (instead of id) Product Ean or Ean13
    * 
    * @apiSuccess {Guid}      id               CatalogProduct Id.
    * @apiSuccess {Guid}      catalogId        Catalog Id.
    * @apiSuccess {Guid}      productId        Product Id.
    * @apiSuccess {Guid}      domainId         Domain Id.
    * @apiSuccess {string}    domain           Domain Name.
    * @apiSuccess {string}    ean              Ean.
    * @apiSuccess {string}    ean13            Ean13.
    * @apiSuccess {string}    title            Title.
    * @apiSuccess {string}    subTitle         SubTitle.
    * @apiSuccess {string}    author           Author Name.
    * @apiSuccess {string}    publisher        Publisher Name.
    * @apiSuccess {string}    imprint          Imprint Name.
    * @apiSuccess {string}    mediaFormat      MediaFormat Name.
    * @apiSuccess {int}       pageCount        Page Count.
    * @apiSuccess {string}    description      Description.
    * @apiSuccess {string}    tableOfContents  Table Of Contents.
    * @apiSuccess {string}    authorInfo       Author Info.
    * @apiSuccess {decimal}   price            Price.
    * @apiSuccess {string}    displayPrice     Price formatted for display.
    * @apiSuccess {DateTime}  releaseDate      Release Date
    * @apiSuccess {string}    coverUrl         Large Cover URL
    * @apiSuccess {string}    thumbUrl         Small Cover URL
    * 
    * @apiSuccessExample Success-Response:
    * HTTP/1.1 200 OK
    * {
    *     "id":"c61aac5a-50d1-42b2-911c-ecd64d8f5ad2",
    *     "productId":"f1a74928-7289-4a4a-8804-a354012da153",
    *     "catalogId":"5bcc220c-160d-41d3-9ba1-a33901394536",
    *     "ean13":"9781493003136",
    *     "ean":"978-1-4930-0313-6",
    *     "author":"Dunbar Hardy",
    *     "imprint":"FalconGuides",
    *     "publisher":"FalconGuides",
    *     "domain":"Harvard","title":"Paddling Colorado",
    *     "subTitle":"A Guide to the State's Best Paddling Routes",
    *     "thumbUrl":"https://fcs-img-debug.s3.amazonaws.com/83fe7068-9cba-4ba8-9114-9fa4014d2a28/e7d78671-53fe-4e8d-bce8-a354012db023/M.jpg",
    *     "coverUrl":"https://fcs-img-debug.s3.amazonaws.com/83fe7068-9cba-4ba8-9114-9fa4014d2a28/e7d78671-53fe-4e8d-bce8-a354012db023/L.jpg",
    *     "description":"Colorado may be a skier’s paradise, but once the snow melts, it makes an abrupt transition into an exciting home for paddlers...",
    *     "authorInfo":"Dunbar Hardy, whose articles and photographs have appeared in National Geographic Adventure, Outside, and Canoe and Kayak, is....",
    *     "releaseDate":"2009-04-21T00:00:00.0000000Z",
    *     "stockQty":0,
    *     "price":13.5000,
    *     "displayPrice":"$13.50",
    *     "mediaFormat":"ePub/NOAMZEP",
    *     "pageCount":176
    * }
    */
    [Route("/catalogs/products/{id}", "GET")]
    [Route("/catalogs/products", "GET")]
    public class CatalogProductRequest {
        public Guid? Id { get; set; }
        public string Ean { get; set; }
    }

    //TEMPORARY PRODUCT GET WITHOUT RELYING ON CATALOG
    //!!REMOVE ONCE CATALOGS WORKS FOR HARVARD!!
    [Route("/catalogs/products/temp", "GET")]
    public class CatalogProductTempRequest {
        public string Ean { get; set; }
    }

    [Route("/catalogs/paged/products", "GET")]
    public class CatalogProductsRequest : Filter {}
}