using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Slipways.DataProvider.Infrastructure
{
    public interface ISecretProvider
    {
        string GetSecret(string key);
    }
    public class SecretProvider : ISecretProvider
    {

        public string GetSecret(
              string key)
        {
            const string DOCKER_SECRET_PATH = "/run/secrets/";
            if (Directory.Exists(DOCKER_SECRET_PATH))
            {
                using var provider = new PhysicalFileProvider(DOCKER_SECRET_PATH);
                var fileInfo = provider.GetFileInfo(key);
                if (fileInfo.Exists)
                {
                    using var stream = fileInfo.CreateReadStream();
                    using var streamReader = new StreamReader(stream);
                    return streamReader.ReadToEnd();
                }
            }
            return string.Empty;
        }
    }
}