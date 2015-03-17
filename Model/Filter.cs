// Copyright © 2010-2013 Firebrand Technologies

using System;

namespace Fcs.Model {
    public class Filter {
        //public Guid? Id { get; set; }
        public string Search { get; set; }
        public bool IncludeInactive { get; set; }
        public DateTime? ModifiedSince { get; set; }
        public bool UsePaging { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string OrderBy { get; set; }

        // Aliases
        public string S {
            get { return this.Search; }
            set { this.Search = value; }
        }

        public int? P {
            get { return this.Page; }
            set { this.Page = value; }
        }

        public int? Ps {
            get { return this.PageSize; }
            set { this.PageSize = value; }
        }

        public string O {
            get { return this.OrderBy; }
            set { this.OrderBy = value; }
        }

        public string Sort {
            get { return this.OrderBy; }
            set { this.OrderBy = value; }
        }

        // For backwards compatibility
        public string SearchFilter { 
            get { return this.Search; } 
            set { this.Search = value; } 
        }
    }
}