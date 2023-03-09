namespace Assemblers.SshNet;

/// <summary>
/// `SshNet` 配置选项
/// </summary>
[DebuggerStepThrough]
public sealed class SshNetOptions : IOptions<SshNetOptions>
{
    /// <summary>
    /// 连接方式
    /// `sftp://username:password@host:22`
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string? PassWord { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; } = 22;

    /// <summary>
    /// 代理
    /// </summary>
    public string Proxy { get; set; }

    /// <summary>
    /// 代理方式
    /// </summary>
    public ProxyTypes ProxyType { get; set; } = ProxyTypes.Http;

    /// <summary>
    /// 私钥
    /// </summary>
    public Stream? PrivateKey { get; set; }

    /// <summary>
    /// 私钥口令
    /// </summary>
    public string? PrivateKeyPassPhrase { get; set; }

    public SshNetOptions Value => this;
}