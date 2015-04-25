// Copyright © 2010-2015 Firebrand Technologies

using System;
using Fcs.Framework;

namespace Fcs {
    public class FcsToken {
        public string Value { get; set; }
        public DateTime? Expires { get; set; }
        public string User { get; set; }
        public string Session { get; set; }

        public bool IsValid() {
            return this.Value.IsFull() &&
                   this.Expires > DateTime.UtcNow;
        }
    }
}