using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack;

namespace Fcs.Model {

    [Route("/domains", "GET")]
    public class Domains : Filter, IReturn<List<DomainSummary>> {
        public int? Service { get; set; }
        public string Status { get; set; }
        public string CalcStatus { get; set; }
    }

    public class DomainSummary {
        public Guid? Id { get; set; }
        public string Tag { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Name { get; set; }
        public string ServiceLevel { get; set; }
        public string Status { get; set; }
        public string CalculatedStatus { get; set; }
    }
}
