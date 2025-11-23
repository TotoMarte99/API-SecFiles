namespace ApiScann.DTOs
{
    public class ScanLog
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Sha256 { get; set; }
        public string MimeType { get; set; }
        public string DeclaredExtension { get; set; }
        public string Result { get; set; }
        public long FileSize { get; set; }
        public string? ClientIp { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    }
}
