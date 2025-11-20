using ApiScann.DTOs;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.JSInterop.Infrastructure;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ApiScann
{
    public class DefenderScanner
    {
        private string rutaPath =
            @"C:\ProgramData\Microsoft\Windows Defender\Platform\4.18.25090.3009-0\MpCmdRun.exe";

        public async Task<ParseDTO> ScanFile(string filePath)
        {
            //Objeto inicial para que .NET sepa como ejecutar el programa
            var start = new ProcessStartInfo
            {
                FileName = rutaPath,
                Arguments = $"-Scan -ScanType 3 -File \"{filePath}\"", //-Scan (Inicia el escaneo) -ScanType 3 Comando especifico para escaneo de archivo o ruta personalizada
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            



            //Inicio del escaneo
            using var process = Process.Start(start);

            //Leer todas las salidas del programa de forma asincronica
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            //Detiene la ejecucion hasta que el escaneo de Defender termine su trabajo.
            process.WaitForExit();

            //Combinamos las salidas de output y eror y se envian al siguiente paso como atributo de la clase
            string rawResult = output + "\n" + error;


            return ParseDefenderOutput(rawResult);
        }

        private ParseDTO ParseDefenderOutput(string raw)
        {
            //Establezco valores iniciales para el DTO
            var dto = new ParseDTO
            {
                malicious = false,
                threat = null,
                engine = "Windows Defender",
                DateTime = DateTime.Now
            };

            if (string.IsNullOrWhiteSpace(raw))
                return dto;

            var lineas = raw.Split('\n').Select(l => l.Trim()).Where( l => !string.IsNullOrWhiteSpace(l));

            foreach(var line in lineas)
            {
                var threarmatch = Regex.Match(line, @"detected:\s*(.+)", RegexOptions.IgnoreCase);

                if (threarmatch.Success)
                {
                    dto.malicious = true;
                    dto.threat = threarmatch.Groups[1].Value.Trim();
                    return dto;
                }

                var threatMatch = Regex.Match(line, @"(Trojan|Worm|Spyware|Adware|Ransomware|PUA|Backdoor|Virus):.+", RegexOptions.IgnoreCase);

                if (threatMatch.Success)
                {
                    dto.malicious = true;
                    dto.threat = threatMatch.Value;
                    return dto;
                }

                if (line.Contains("Found 0 threats", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("No threats found", StringComparison.OrdinalIgnoreCase))
                {
                    dto.malicious = false;
                    dto.threat = null;
                    return dto;
                }
            }

            dto.malicious = false ;
            dto.threat = null;
            return dto ;
        }

    }
}


