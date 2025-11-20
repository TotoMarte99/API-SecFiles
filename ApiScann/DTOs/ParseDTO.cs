namespace ApiScann.DTOs
{
    public class ParseDTO
    {
        public bool malicious { get; set; }
        public string threat { get; set; }
        public string engine { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string SHA256 { get; set; }

    }
}
