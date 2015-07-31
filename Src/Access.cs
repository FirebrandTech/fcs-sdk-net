// Copyright © 2010-2015 Firebrand Technologies

using System;

namespace Fcs {
    public class Access {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public string User { get; set; }
        public string Session { get; set; }
        public string Continue { get; set; }
    }
}