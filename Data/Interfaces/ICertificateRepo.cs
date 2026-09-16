using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using certifyAb.Data.Entities;

namespace certifyab.Data.Interfaces
{
    public interface ICertificateRepo
    {
        public Task<Certificate> Create(Certificate certificate);
    }
}