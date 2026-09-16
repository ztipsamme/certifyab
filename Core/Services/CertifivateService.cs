using System;
using AutoMapper;
using certifyab.Core.Interfaces;
using certifyab.Data.Interfaces;
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
            dto.Url = $"{_url}/verify/{savedCertificate.Id}";

            return dto;
        }

        public async Task<List<CertificateDTO>> GetAllAsync()
        {
            var certificates = await _repo.GetAllAsync();

            var dtos = _mapper.Map<List<CertificateDTO>>(certificates);

            if (_isDev)
            {
                foreach (var mockDto in dtos)
                    mockDto.Url = mockDto.Url = $"{_url}/verify/{mockDto.Id}";
            }

            return dtos;
        }
    }
}