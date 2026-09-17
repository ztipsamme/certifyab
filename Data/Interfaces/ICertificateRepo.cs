using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using certifyAb.Data.Entities;

namespace certifyab.Data.Interfaces
{
    public interface ICertificateRepo
    {
        Task<Certificate> CreateAsync(Certificate certificate);
        Task<Certificate?> GetByIdAsync(string id);
        Task<List<Certificate>> GetAllAsync();
        Task<Certificate?> GetByUuidAsync(string uuid);
    }
}