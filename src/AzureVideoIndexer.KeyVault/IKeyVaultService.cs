using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AzureVideoIndexer.KeyVault
{
    public interface IKeyVaultService
    {
        Task<X509Certificate2> GetCertificateAsync(string certificateName);
    }
}
