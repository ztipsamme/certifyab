using certifyab.Core.Dto;

namespace certifyab.Core.Interfaces
{
    public interface ICertificateService
    {
        Task<CertificateCreatedDTO> CreateAsync(CertificateCreateDTO certificate);
        Task<CertificateDTO> GetByIdAsync(string id);
        Task<List<CertificateDTO>> GetAllAsync();
        Task<CertificatePublicDTO> GetByUuidAsync(string uuid);
    }
}