using ApiScann;
using ApiScann.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace ApiScann.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController : ControllerBase
    {
        private readonly DefenderScanner _scanner;
        private readonly AppDbContext _context;

        public ScanController(DefenderScanner scanner, AppDbContext context)
        {
            _scanner = scanner;
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> ScanFile(IFormFile file)
        {
            var allowedExtensions = new[] {".pdf",".docx",".xlsx",".zip",".rar",".exe",".txt",".jpg",".png"};

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
            "application/x-msdownload",
            "application/x-rar-compressed",
            "application/rar",
            "text/plain"
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
            try
            {
                using (var stream = System.IO.File.Create(tempPath))
                {
                    await file.CopyToAsync(stream);
                }

                // Se obtiene el sha256 y se guarda
                string sha256Hash;
                using (var sha = SHA256.Create())
                using (var fileStream = System.IO.File.OpenRead(tempPath))
                {
                    sha256Hash = BitConverter.ToString(sha.ComputeHash(fileStream)).Replace("-", "").ToLower();
                }

                // Escanear el archivo con el motor de Windows Defender
                var result = await _scanner.ScanFile(tempPath);

                result.SHA256 = sha256Hash;

                return Ok(result);
            }
            catch (System.IO.IOException ex) when (ex.Message.Contains("virus") || ex.HResult == -2147024864)
            {
                string sha256Hash = "ANTIVIRUS_INTERCEPTED";

                var logEntry = new ScanLog
                {
                    Id = Guid.NewGuid(),
                    FileName = file.FileName,
                    Sha256 = sha256Hash,
                    MimeType = file.ContentType,
                    DeclaredExtension = extension,
                    Result = "Malicioso: Bloqueado por Antivirus del Sistema Operativo",
                    FileSize = file.Length,
                    ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    CreatedAt = DateTimeOffset.Now
                };

                _context.ScanLog.Add(logEntry);
                await _context.SaveChangesAsync();

                var blockedResult = new ParseDTO 
                {
                    malicious = true,
                    threat = "Bloqueado por Antivirus del Sistema Operativo (IOException)",
                    SHA256 = sha256Hash
                };

                return Ok(blockedResult);
            }
            catch (Exception ex)
            {
                // Para otros errores no relacionados con el antivirus (ej. DB, Scanner)
                // Puedes loguear el error aquí
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Ocurrió un error inesperado durante el procesamiento del archivo.");
            }
            finally
            {
                // Asegurar el borrado del archivo temporal
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }

            }
        }
    }
}