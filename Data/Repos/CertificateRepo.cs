using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using certifyab.Data.Interfaces;
using certifyAb.Data.Entities;

namespace certifyab.Data.Repos
{

    public class CertificateRepo : ICertificateRepo
    {
        private readonly BlobServiceClient _serviceClient = null!;
        private readonly BlobContainerClient _container = null!;
        private readonly bool _isDev;

        public CertificateRepo(IConfiguration config, IHostEnvironment environment)
        {
            _isDev = environment.IsDevelopment();

            if (_isDev)
                return;

            var accountURL = config["Storage:AccountUrl"]!;
            var containerName = config["Storage:Container"] ?? "certificates";

            _serviceClient = new BlobServiceClient(
                new Uri(accountURL),
                new DefaultAzureCredential());

            _container = _serviceClient.GetBlobContainerClient(containerName);
        }

        public async Task<Certificate> CreateAsync(Certificate certificate)
        {
            certificate.Id = Guid.NewGuid().ToString();

            if (_isDev)
            {
                MockData.MockCertificates.Certificates.Add(certificate);
                return certificate;
            }

            var blobName = $"{certificate.Id}.json";
            var blob = _container.GetBlobClient(blobName);

            var metadata = new Dictionary<string, string>
            {
                ["Id"] = certificate.Id,
            };

            var json = BinaryData.FromObjectAsJson(certificate);

            await blob.UploadAsync(json, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/json"
                },
                Metadata = metadata
            });

            return certificate;
        }

        public async Task<Certificate?> GetByIdAsync(string id)
        {
            if (_isDev)
                return MockData.MockCertificates.Certificates.FirstOrDefault(c => c.Id == id);

            await foreach (BlobItem blobItem in _container.GetBlobsAsync(new GetBlobsOptions
            {
                Traits = BlobTraits.Metadata
            }))
            {
                if (!blobItem.Metadata.TryGetValue("Id", out var blobId) || blobId != id)
                    continue;

                var certificate = await GetCertificateContentAsync(blobItem);
            }

            return null;
        }

        public async Task<List<Certificate>> GetAllAsync()
        {
            var certificates = new List<Certificate>();

            if (_isDev) return MockData.MockCertificates.Certificates;

            await foreach (BlobItem blobItem in _container.GetBlobsAsync(new GetBlobsOptions
            {
                Traits = BlobTraits.Metadata
            }))
            {
                var certificate = await GetCertificateContentAsync(blobItem);

                if (certificate != null)
                {
                    certificates.Add(certificate);
                }
            }

            return certificates;
        }

        private async Task<Certificate?> GetCertificateContentAsync(BlobItem blobItem)
        {
            var blob = _container.GetBlobClient(blobItem.Name);
            var response = await blob.DownloadContentAsync();
            var certificate = response.Value.Content.ToObjectFromJson<Certificate>();

            return certificate;
        }
    }
}