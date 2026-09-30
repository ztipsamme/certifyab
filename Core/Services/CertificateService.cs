using AutoMapper;
using certifyab.Core.Interfaces;
using certifyab.Data.Interfaces;
using certifyab.Core.Dto;
using certifyab.Data.Entities;

namespace certifyab.Core.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepo _repo;
        private readonly IMapper _mapper;
        private readonly string _url;

        public CertificateService(IConfiguration config, ICertificateRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _url = config["ApiUrl"] ?? "";
        }

        public async Task<CertificateCreatedDTO> CreateAsync(CertificateCreateDTO certificate)
        {
            var savedCertificate = await _repo.CreateAsync(
                _mapper.Map<Certificate>(certificate));

            var dto = _mapper.Map<CertificateCreatedDTO>(savedCertificate);
            dto.Url = UrlFor(dto.Uuid);

            return dto;
        }

        public async Task<CertificateDTO> GetByIdAsync(string id)
        {
            var certificate = await _repo.GetByIdAsync(id);

            var dto = _mapper.Map<CertificateDTO>(certificate); ;
            dto.Url = UrlFor(dto.Uuid);

            return dto;
        }

        public async Task<List<CertificateDTO>> GetAllAsync()
        {
            var certificates = await _repo.GetAllAsync();

            var dtos = _mapper.Map<List<CertificateDTO>>(certificates);
            dtos.ForEach(d => d.Url = UrlFor(d.Uuid));

            return dtos;
        }

        public async Task<CertificatePublicDTO> GetByUuidAsync(string uuid)
        {
            var certificate = await _repo.GetByUuidAsync(uuid);

            var dto = _mapper.Map<CertificatePublicDTO>(certificate);
            dto.Url = UrlFor(dto.Uuid);

            return dto;
        }

        private string UrlFor(string uuid) => $"{_url}/verify/{uuid}";
    }
}