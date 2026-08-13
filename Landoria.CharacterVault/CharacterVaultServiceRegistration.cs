using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Landoria.CharacterVault
{
    internal static class CharacterVaultServiceRegistration
    {
        internal static ServiceProvider Build(SynchronizationContext unityContext)
        {
            if (unityContext == null)
            {
                throw new ArgumentNullException(nameof(unityContext));
            }

            ServiceCollection services = new ServiceCollection();
            services.AddSingleton(unityContext);
            services.AddSingleton<IValheimAdapterFactory, ValheimAdapterFactory>();
            services.AddSingleton<IValheimEnvironment, ValheimEnvironment>();
            services.AddSingleton<IBackupFileSystem, SystemBackupFileSystem>();
            services.AddSingleton(provider => new BackupRetention(
                provider.GetRequiredService<IBackupFileSystem>()));
            services.AddSingleton(provider => new VaultStorage(
                provider.GetRequiredService<BackupRetention>()));
            services.AddSingleton(provider => new ProfileTransferService(
                provider.GetRequiredService<SynchronizationContext>(),
                provider.GetRequiredService<IValheimAdapterFactory>(),
                provider.GetRequiredService<IValheimEnvironment>(),
                provider.GetRequiredService<VaultStorage>()));
            services.AddSingleton<IWorldCheckpointRequest>(provider =>
                new WorldCheckpointRequest(provider.GetRequiredService<ProfileTransferService>()));
            services.AddSingleton(provider => new GracefulShutdownCoordinator(
                provider.GetRequiredService<SynchronizationContext>()));
            services.AddSingleton<VoluntaryDisconnectCoordinator>();
            services.AddSingleton<ServerDisconnectSaveCoordinator>();
            services.AddSingleton<CharacterSaveStatusDisplay>();
            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }
    }
}
