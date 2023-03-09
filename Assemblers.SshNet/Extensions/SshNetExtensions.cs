namespace Assemblers.SshNet;

internal static class SshNetExtensions
{
    public static Task<IEnumerable<SftpFile>> ListDirectoryAsync(this SftpClient client, string path, Action<int> listCallback = null)
        => Task.Factory.FromAsync(client.BeginListDirectory(path, null, null, listCallback), client.EndListDirectory);
}