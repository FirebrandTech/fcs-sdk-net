using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack;

namespace Fcs.Model {

    [Route("/domains", "GET")]
    public class Domains : Filter, IReturn<List<Domains>> {
        public int? Service { get; set; }
        public string Status { get; set; }
        public string CalcStatus { get; set; }
    }
}
