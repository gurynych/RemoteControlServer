using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.KeyStore
{
    public class ServerKeysStore : AsymmetricKeyStoreBase
    {
        private readonly IServiceProvider serviceProvider;

        public ServerKeysStore(IServiceProvider serviceProvider, IAsymmetricCryptographer cryptographer) :
            base(cryptographer)
        {            
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Устанавливает закрытый ключ для сервера
        /// </summary>
        protected override byte[] SetPrivateKey()
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            ApplicationContext context = scope.ServiceProvider.GetService<ApplicationContext>();
            if (context == null) return default;

            if (context.ServerPrivateKeys.Any())
            {
                ServerPrivateKey privateKeyModel =
                    context.ServerPrivateKeys.OrderByDescending(x => x.KeyRegistrationDate).FirstOrDefault();

                if (DateTime.Now.ToUniversalTime() < privateKeyModel.KeyExpirationDate)
                {
                    return privateKeyModel.PrivateKey;
                }
            }

            byte[] privateK = cryptographer.GeneratePrivateKey();
            context.ServerPrivateKeys.Add(new ServerPrivateKey(privateK, DateTime.Now.ToUniversalTime()));
            context.SaveChanges();
            return privateK;
        }
    }
}
