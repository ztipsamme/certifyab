namespace certifyab.Core.Dto
{
    public class CertificateBase
    {
        public string Recipient { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class CertificateMetaData : CertificateBase
    {
        public string Id { get; set; } = string.Empty;
        public string Uuid { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class CertificateDTO : CertificateMetaData { }

    public class CertificateCreateDTO : CertificateBase { }

    public class CertificateCreatedDTO : CertificateMetaData { }

    public class CertificatePublicDTO : CertificateBase
    {
        public string Uuid { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    };

}