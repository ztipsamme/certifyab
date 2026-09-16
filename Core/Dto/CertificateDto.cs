using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace certifyAb.Core.Dto
{
    public class CertificateBase
    {
        public string Receiver { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
    }

    public class CertificateDTO : CertificateBase
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    };

    public class CertificateCreateDTO : CertificateBase
    {
    };

    public class CertificateCreatedDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    };
}