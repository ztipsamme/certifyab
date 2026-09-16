using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using certifyab.Data.Interfaces;
using certifyAb.Core.Dto;
using certifyAb.Data.Entities;

namespace certifyab.Data.Repos
{

    public class CertificateRepo : ICertificateRepo
    {
        private readonly BlobServiceClient _serviceClient;
        private readonly BlobContainerClient _container;

        public CertificateRepo(IConfiguration config)
        {
            var accountURL = config["Storage:AccountUrl"]!;
            var containerName = config["Storage:Container"] ?? "certificates";

            _serviceClient = new BlobServiceClient(
                new Uri(accountURL),
                new DefaultAzureCredential());

            _container = _serviceClient.GetBlobContainerClient(containerName);
        }

        public async Task<Certificate> Create(Certificate certificate)
        {
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
    }
}