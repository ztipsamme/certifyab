using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace certifyAb.Data.Entities
{
    public class Certificate
    {
        public string Id { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    };
}