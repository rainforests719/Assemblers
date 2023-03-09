namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// `SshNet` 扩展类
/// </summary>
public static class SshNetServiceCollectionExtensions
{
    /// <summary>
    /// 注册 `SshNetOptions` 配置选项
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <param name="inludeDefaultHttpClient">是否包含默认客户端</param>
    /// <returns></returns>
    private static IServiceCollection AddSshNetOptions(this IServiceCollection services, Action optionAction)
    {
        optionAction();
        return services;
    }

    /// <summary>
    /// 注册 `SshNet`
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionSettingKey"> 配置文件`Key` </param>
    /// <returns> IServiceCollection </returns>
    public static IServiceCollection AddSshNet(this IServiceCollection services, string optionSettingKey)
    {
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var Configuration = serviceProvider.GetRequiredService<IConfiguration>();
        services.AddSshNetOptions(() => services.Configure<SshNetOptions>(Configuration.GetSection(optionSettingKey)));
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISftp, Sftp>());
        return services;
    }
}