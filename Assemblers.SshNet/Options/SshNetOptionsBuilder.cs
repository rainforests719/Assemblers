namespace Assemblers.SshNet;

/// <summary>
/// `SshNetOptions` 建造者
/// </summary>
public class SshNetOptionsBuilder : OptionsBuilder<SshNetOptions>
{
    public SshNetOptionsBuilder SetConnectionString(string connectionString)
    {
        Target.ConnectionString = connectionString;
        return this;
    }

    public SshNetOptionsBuilder SetUserName(string userName)
    {
        Target.UserName = userName;
        return this;
    }

    public SshNetOptionsBuilder SetPassWord(string passWord)
    {
        Target.PassWord = passWord;
        return this;
    }

    public SshNetOptionsBuilder SetHost(string host)
    {
        Target.Host = host;
        return this;
    }

    public SshNetOptionsBuilder SetPort(int port)
    {
        Target.Port = port;
        return this;
    }

    public SshNetOptionsBuilder SetProxyType(ProxyTypes proxyType)
    {
        Target.ProxyType = proxyType;
        return this;
    }

    public SshNetOptionsBuilder SetPrivateKey(string privateKey)
    {
        Target.PrivateKey = new MemoryStream(Encoding.UTF8.GetBytes(privateKey)); ;
        return this;
    }

    public SshNetOptionsBuilder SetPrivateKey(Stream privateKey)
    {
        Target.PrivateKey = privateKey;
        return this;
    }

    public SshNetOptionsBuilder SetPrivateKeyPassPhrase(string privateKeyPassPhrase)
    {
        Target.PrivateKeyPassPhrase = privateKeyPassPhrase;
        return this;
    }
}