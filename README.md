# API-SecFiles

**API-SecFiles** es una API REST desarrollada en C# (.NET 8) que permite escanear archivos subidos para detectar malware usando **Windows Defender**. Incluye validaciones de seguridad como extensión de archivo, MIME type, doble extensión y genera el **hash SHA256** del archivo para auditoría.

---

##  Características

- Escaneo de archivos con **Windows Defender**.
- Validación de **extensiones permitidas** (`.pdf`, `.docx`, `.xlsx`, `.zip`, `.exe`).
- Validación de **MIME type**.
- Verificación de **doble extensión** (para archivos disfrazados).
- Generación de **SHA256** de cada archivo.
- Límite de subida de archivos: **100 MB**.
- Logs básicos de auditoría en consola.
- Archivos temporales eliminados automáticamente.

---
## Archivo valido
<img width="1403" height="196" alt="image" src="https://github.com/user-attachments/assets/4ee073f5-6e90-4c29-8a75-89b5cee0d9df" />




