using AfipServiceReference;
using Microsoft.AspNetCore.Http;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Model.Auditoria;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AFIP.Facturacion.Services
{
    public class FileStorageService : IFileStorageService
    {
        private static readonly string pathProyect = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "AFIP.Facturacion");

        public FileStorageService()
        {

        }
        public async Task<string> ReplaceCertificateAsync(IFormFile file, int idTienda)
        {
            var filePath = Path.Combine(pathProyect, "Certificados", idTienda.ToString() + "_Tienda");

            await DeleteFileIfExistsAsync(filePath);

            var newFilePath = await SaveFileAsync(file, filePath);

            return newFilePath;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return filePath;
        }

        private async Task DeleteFileIfExistsAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public VMX509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword)
        {
            ExistCertificate(certificatePath);

            X509Certificate2 certificate = new X509Certificate2(certificatePath, certificatePassword);

            // Display certificate information
            //Console.WriteLine("Subject: " + certificate.Subject); // SERIALNUMBER=CUIT 23365081999, CN=Test
            //Console.WriteLine("Valid From: " + certificate.NotBefore); // fecha inicio
            //Console.WriteLine("Valid To: " + certificate.NotAfter); // fecha caducidad

            if (string.IsNullOrEmpty(certificate.Subject))
            {
                throw new Exception("El certificado no tiene Subject");
            }

            var cert = new VMX509Certificate2()
            {
                Subject = certificate.Subject,
                FechaInicio = certificate.NotBefore,
                FechaCaducidad = certificate.NotAfter
            };

            if (!string.IsNullOrEmpty(cert.Cuil) && cert.Cuil.Length != 11)
            {
                throw new Exception("El certificado no tiene CUIL / CUIT valido, debe ser de esta forma: 'SERIALNUMBER=CUIT xxxxxxxxxxx'");
            }

            return cert;
        }

        public static void ExistCertificate(string certificatePath)
        {
            if (!File.Exists(certificatePath))
            {
                throw new FileNotFoundException("El archivo de certificado no existe en la ruta especificada.", certificatePath);
            }
        }

        public static string ObtenerRutaCertificado(AjustesFacturacion ajustesFacturacion)
        {
            if (string.IsNullOrEmpty(ajustesFacturacion.CertificadoNombre))
            {
                throw new Exception($"La tienda no tiene un certificado guardado.");
            }

            // Construye la ruta completa del certificado
            var certificadoPath = Path.Combine(pathProyect, "Certificados", ajustesFacturacion.IdTienda.ToString() + "_Tienda", ajustesFacturacion.CertificadoNombre);

            return certificadoPath;
        }
    }
}
