using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using PointOfSale.Model;

namespace AFIP.Facturacion
{
    /// <summary>
    /// Libreria de utilidades para manejo de certificados
    /// </summary>
    /// <remarks></remarks>
    internal class X509CertificateManager
    {
        public static bool VerboseMode = false;

        /// <summary>
        /// Firma mensaje
        /// </summary>
        /// <param name="messageBytes">Bytes del mensaje</param>
        /// <param name="signerCertificate">Certificado usado para firmar</param>
        /// <returns>Bytes del mensaje firmado</returns>
        /// <remarks></remarks>
        public static byte[] SignMessageBytes(byte[] messageBytes, X509Certificate2 signerCertificate)
        {
            var pasos = "";

            const string ID_FNC = "[FirmaBytesMensaje]";
            try
            {
                if (!signerCertificate.HasPrivateKey)
                {
                    throw new Exception("El certificado no tiene clave privada.");
                }

                if (VerboseMode) Console.WriteLine(ID_FNC + "***Firmando bytes del mensaje...");

                using (RSA privateKey = signerCertificate.GetRSAPrivateKey())
                {
                    if (privateKey == null)
                        throw new Exception("No se pudo obtener la clave privada.");


                    var contentInfo = new ContentInfo(messageBytes);
                    var signedCms = new SignedCms(contentInfo);
                    var cmsSigner = new CmsSigner(signerCertificate)
                    {
                        IncludeOption = X509IncludeOption.EndCertOnly
                    };

                    signedCms.ComputeSignature(cmsSigner);

                    return signedCms.Encode();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ID_FNC + pasos + " ***Error al firmar: " + ex.ToString());
            }
        }

        /// <summary>
        /// Lee certificado de disco
        /// </summary>
        /// <param name="file">Ruta del certificado a leer.</param>
        /// <returns>Un objeto certificado X509</returns>
        /// <remarks></remarks>
        public static X509Certificate2 GetCertificateFromFile(string file, SecureString password)
        {
            string ID_FNC = "[ObtieneCertificadoDesdeArchivo]";

            try
            {
                if (!File.Exists(file))
                {
                    throw new Exception($"{ID_FNC} ***Error:6 El archivo no existe en la ruta: {file}");
                }

                byte[] rawData = File.ReadAllBytes(file);
                if (rawData.Length == 0)
                {
                    throw new Exception($"{ID_FNC} ***Error:7 El archivo está vacío.");
                }

                ID_FNC += $"--- *1.5**Leyendo certificado. --";

                var objCert = new X509Certificate2(rawData, password,
                                    X509KeyStorageFlags.UserKeySet |
                                    X509KeyStorageFlags.PersistKeySet |
                                    X509KeyStorageFlags.Exportable);

                if (!objCert.HasPrivateKey)
                {
                    throw new Exception($"{ID_FNC} ***Error:8 El certificado no tiene clave privada.");
                }

                return objCert;
            }
            catch (CryptographicException ex)
            {
                throw new Exception($"{ID_FNC} ***Error9 criptográfico al leer el certificado: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ID_FNC} ***Error10 general al leer certificado: {ex.Message}");
            }
        }

    }
}
