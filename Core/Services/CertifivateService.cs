using System;
using AutoMapper;
using certifyab.Core.Interfaces;
using certifyab.Data.Interfaces;
using certifyab.Data.MockData;
using certifyAb.Core.Dto;
using certifyAb.Data.Entities;

namespace certifyAb.Core.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepo _repo;
        private readonly IMapper _mapper;
        private readonly bool _isDev;
        private readonly string _url = "";

        public CertificateService(IConfiguration config, ICertificateRepo repo, IMapper mapper, IHostEnvironment environment)
        {
            _repo = repo;
            _mapper = mapper;
            _isDev = environment.IsDevelopment();
            _url = config["Azure:ApiUrl"]!;
        }

        public async Task<CertificateCreatedDTO> CreateAsync(CertificateCreateDTO certificate)
        {
            var savedCertificate = await _repo.CreateAsync(
                _mapper.Map<Certificate>(certificate));

            var dto = _mapper.Map<CertificateCreatedDTO>(savedCertificate);
            dto.Url = SetUrl(dto.Uuid);

            return dto;
        }

        public async Task<CertificateDTO> GetByIdAsync(string id)
        {
            var certificate = await _repo.GetByIdAsync(id);

            if (_isDev)
            {
                var mockCertificate = FindMockCertificate(id);

                var mockDto = _mapper.Map<CertificateDTO>(mockCertificate);
                mockDto.Url = SetUrl(mockDto.Uuid);

                return mockDto;
            }

            var dto = MapToCertificateDTO(certificate);

            return dto;
        }

        public async Task<List<CertificateDTO>> GetAllAsync()
        {
            var certificates = await _repo.GetAllAsync();

            var dtos = _mapper.Map<List<CertificateDTO>>(certificates);

            if (_isDev)
            {
                foreach (var mockDto in dtos)
                    mockDto.Url = SetUrl(mockDto.Uuid);
            }

            return dtos;
        }

        public async Task<CertificatePublicDTO> GetByUuidAsync(string uuid)
        {
            var certificate = await _repo.GetByUuidAsync(uuid);

            if (_isDev)
            {
                var mockCertificate = FindMockCertificate(uuid);
                var mockDto = MapToCertificatePublicDTO(mockCertificate);

                return mockDto;
            }

            var dto = MapToCertificatePublicDTO(certificate);

            return dto;
        }

        private string SetUrl(string Uuid) => $"{_url}/verify/{Uuid}";

        private Certificate? FindMockCertificate(string idOrUuid) =>
            MockCertificates.Certificates.FirstOrDefault(c => c.Id == idOrUuid || c.Uuid == idOrUuid);

        private CertificateDTO MapToCertificateDTO(Certificate? certificate)
        {
            var dto = _mapper.Map<CertificateDTO>(certificate);
            dto.Url = SetUrl(dto.Uuid);

            return dto;
        }

        private CertificatePublicDTO MapToCertificatePublicDTO(Certificate? certificate)
        {
            var dto = _mapper.Map<CertificatePublicDTO>(certificate);
            dto.Url = SetUrl(dto.Uuid);

            return dto;
        }

    }
}