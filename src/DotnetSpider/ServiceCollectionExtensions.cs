using System;
using DotnetSpider.Common;
using DotnetSpider.DataFlow;
using DotnetSpider.DownloadAgent;
using DotnetSpider.DownloadAgentRegisterCenter;
using DotnetSpider.DownloadAgentRegisterCenter.Store;
using DotnetSpider.MessageQueue;
using DotnetSpider.Network;
using DotnetSpider.Network.InternetDetector;
using DotnetSpider.Statistics;
using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetSpider
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ConfigureAppConfiguration(this IServiceCollection services,
			string config = null)
		{
			Check.NotNull(services, nameof(services));

			var configurationBuilder = Framework.CreateConfigurationBuilder(config);
			var configurationRoot = configurationBuilder.Build();
			services.AddSingleton<IConfiguration>(configurationRoot);

			return services;
		}

		#region DownloadCenter

		public static IServiceCollection AddDownloadCenter(this IServiceCollection services,
			Action<DownloadAgentRegisterCenterBuilder> configure = null)
		{
			services.AddHostedService<DefaultDownloadAgentRegisterCenter>();

			var downloadCenterBuilder = new DownloadAgentRegisterCenterBuilder(services);
			configure?.Invoke(downloadCenterBuilder);

			return services;
		}

		public static IServiceCollection AddLocalDownloadCenter(this IServiceCollection services)
		{
			services.AddDownloadCenter(x => x.UseLocalDownloaderAgentStore());
			return services;
		}

		public static DownloadAgentRegisterCenterBuilder UseLocalDownloaderAgentStore(
			this DownloadAgentRegisterCenterBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IDownloaderAgentStore, LocalDownloaderAgentStore>();
			return builder;
		}

		#endregion

		#region  MessageQueue

		public static IServiceCollection AddThroughMessageQueue(this IServiceCollection services)
		{
			services.AddSingleton<IMq, ThroughMessageQueue>();
			return services;
		}

		#endregion

		#region DownloaderAgent

		public static IServiceCollection AddDownloaderAgent(this IServiceCollection services,
			Action<DownloaderAgentBuilder> configure = null)
		{
			services.AddSingleton<IHostedService, DefaultDownloaderAgent>();
			services.AddSingleton<NetworkCenter>();
			services.AddSingleton<DownloaderAgentOptions>();

			var spiderAgentBuilder = new DownloaderAgentBuilder(services);
			configure?.Invoke(spiderAgentBuilder);

			return services;
		}

		public static DownloaderAgentBuilder UseFileLocker(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<ILockerFactory, FileLockerFactory>();

			return builder;
		}

		public static DownloaderAgentBuilder UseDefaultAdslRedialer(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IAdslRedialer, DefaultAdslRedialer>();

			return builder;
		}

		public static DownloaderAgentBuilder UseDefaultInternetDetector(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IInternetDetector, DefaultInternetDetector>();

			return builder;
		}

		public static DownloaderAgentBuilder UseVpsInternetDetector(this DownloaderAgentBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));

			builder.Services.AddSingleton<IInternetDetector, VpsInternetDetector>();

			return builder;
		}

		#endregion

		#region  Statistics

		public static IServiceCollection AddStatisticsCenter(this IServiceCollection services,
			Action<StatisticsBuilder> configure)
		{
			services.AddSingleton<IHostedService, StatisticsCenter>();

			var spiderStatisticsBuilder = new StatisticsBuilder(services);
			configure?.Invoke(spiderStatisticsBuilder);

			return services;
		}

		public static StatisticsBuilder UseMemory(this StatisticsBuilder builder)
		{
			Check.NotNull(builder, nameof(builder));
			builder.Services.AddSingleton<IStatisticsStore, MemoryStatisticsStore>();
			return builder;
		}

		#endregion
	}
}
