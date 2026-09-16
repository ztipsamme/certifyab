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
            dto.Url = SetUrl(dto.Id);

            return dto;
        }

        public async Task<CertificateDTO> GetByIdAsync(string id)
        {
            var certificate = await _repo.GetByIdAsync(id);

            if (_isDev)
            {
                var mockCertificate = MockCertificates.Certificates.FirstOrDefault(c => c.Id == id);

                var mockDto = _mapper.Map<CertificateDTO>(mockCertificate);
                mockDto.Url = SetUrl(mockDto.Id);

                return mockDto;
            }

            var dto = _mapper.Map<CertificateDTO>(certificate);
            dto.Url = SetUrl(dto.Id);

            return dto;
        }

        public async Task<List<CertificateDTO>> GetAllAsync()
        {
            var certificates = await _repo.GetAllAsync();

            var dtos = _mapper.Map<List<CertificateDTO>>(certificates);

            if (_isDev)
            {
                foreach (var mockDto in dtos)
                    mockDto.Url = SetUrl(mockDto.Id);
            }

            return dtos;
        }

        private string SetUrl(string id) => $"{_url}/verify/{id}";

    }
}