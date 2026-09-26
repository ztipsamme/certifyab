using certifyab.Data.Entities;

namespace certifyab.Data.MockData
{
    public static class MockCertificates
    {
        internal static List<Certificate> Certificates { get; } =
           [
            new Certificate {
                Id = "550e8400-e29b-41d4-a716-446655440000",
                Uuid = "6ba7b810-9dad-41d1-80b4-00c04fd430c8",
                Recipient = "Natcho Mästerkatt",
                Course = "Avancerad Kurragömma",
                Date = DateTime.UtcNow
            },
            new Certificate {
                Id = "6ba7b810-9dad-41d1-80b4-00c04fd430c8",
                Uuid = "550e8400-e29b-41d4-a716-446655440000",
                Recipient = "Ghibli Tjuvare",
                Course = "Godis Smuggleri",
                Date = DateTime.UtcNow
            }];
    }
}