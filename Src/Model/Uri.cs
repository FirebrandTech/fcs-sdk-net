// Copyright © 2010-2016 Firebrand Technologies

using System;
using System.Collections.Generic;
using ServiceStack;

namespace Fcs.Model {

    // When you request URIs for a list of EANs, you get back a dictionary mapping EAN -> List<AssetDetails>.
    // This means that the API consumer can sort their EANs however they want and use the dictionary 
    // to look up details for each EAN.

    [Route("/uri/assets/externalDownloadUris", "GET")]
    public class AssetDownloadUrisRequest : IReturn<EanToAssetDetails> {
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<EanSaleDetails> EanDetails { get; set; }
        public string UserIdentifier { get; set; }
        public int? MinutesTilExpiration { get; set; }
    }

    public class EanSalesDetails : List<EanSaleDetails> {
        public void Add(string ean, decimal price) {
            this.Add(new EanSaleDetails {Ean = ean, SalePrice = price});
        }
    }

    public class EanSaleDetails {
        public string Ean { get; set; }
        public decimal SalePrice { get; set; }
    }


    public class EanToAssetDetails : Dictionary<string, List<AssetDetails>> { } // map EANs to lists of URIs

    public class AssetDetails {
        public string Ean { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public string Type { get; set; }
    }
}