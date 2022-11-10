using System;
using System.Collections.Generic;
using System.Text;

namespace JsonToSQL
{
    public class TableRelation
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public int Order { get; set; }
    }
}
