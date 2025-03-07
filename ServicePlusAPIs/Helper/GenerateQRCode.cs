using System.Security.Cryptography;
using System.Text;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace ServicePlusAPIs.Helper
{
    public class GenerateQRCode
    {
       

        // Method to generate the QR code with security enhancements
        public string GetGenerateQRCode(string secureCertificateId)
        {  
            string qrText = $"https://pbsports.punjab.gov.in/ServicePlusSports/VerifySportsCertificate.php?certificateHashKey={secureCertificateId}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (var qrCodeImage = qrCode.GetGraphic(20))
                    {
                        string qrFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedCertificates", "QR");
                        if (!Directory.Exists(qrFolderPath))
                        {
                            Directory.CreateDirectory(qrFolderPath);
                        }

                        string qrFilePath = Path.Combine(qrFolderPath, $"{secureCertificateId}.png");
                        qrCodeImage.Save(qrFilePath, new PngEncoder());
                        return qrFilePath;
                    }
                }
            }
        }
    }
}
