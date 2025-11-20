using Microsoft.AspNetCore.Mvc;
using ApiScann;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace ApiScann.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController : ControllerBase
    {
        private readonly DefenderScanner _scanner = new DefenderScanner();

        [HttpPost("scan")]
        public async Task<IActionResult> ScanFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".zip", ".exe", };

            if (file == null || file.Length == 0)
                return BadRequest("No se subió ningún archivo.");


            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Extension not allowed");
            }
            //Verficacion el MIME type
            var allowedMimeTypes = new[] {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/zip",
            "application/x-msdownload"
            };

            if(!allowedMimeTypes.Contains(file.ContentType))
            {
                return BadRequest("Invalid MIME type");
            }

            //Verificar archivos disfrazados con doble extension
            var realFileName = Path.GetFileName(file.FileName);

            var lastExtension = Path.GetExtension(realFileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(lastExtension))
            {
                return BadRequest("File extension mismatch");
            }

            
            // Crear archivo temporal
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + extension);

            using (var stream = System.IO.File.Create(tempPath))
            {
                await file.CopyToAsync(stream);
            }

            string sha256Hash;
            using (var sha = SHA256.Create())
            using (var fileStream = System.IO.File.OpenRead(tempPath))
            {
                sha256Hash = BitConverter.ToString(sha.ComputeHash(fileStream)).Replace("-", "").ToLower();
            }

            // Escanear el archivo con el motor de Windows Defender
            var result = await _scanner.ScanFile(tempPath);

            result.SHA256 = sha256Hash;

            // Borrar archivo temporal
            System.IO.File.Delete(tempPath);

            return Ok(result);
        }
    }
}
