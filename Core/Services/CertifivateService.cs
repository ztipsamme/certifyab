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
        private readonly string _url = "";

        public CertificateService(ICertificateRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CertificateCreatedDTO> Create(CertificateCreateDTO certificate)
        {
            var savedCertificate = await _repo.Create(
                _mapper.Map<Certificate>(certificate));

            var dto = _mapper.Map<CertificateCreatedDTO>(savedCertificate);
            dto.Url = $"{_url}/verify/{savedCertificate.Id}";

            return dto;
        }

    }
}