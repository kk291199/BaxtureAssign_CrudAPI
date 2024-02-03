using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment.Common_Classes
{
    public class UserSearchCriteria
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }
    }
}