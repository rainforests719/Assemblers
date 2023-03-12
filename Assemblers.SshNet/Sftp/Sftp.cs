namespace Assemblers.SshNet;

public class Sftp : ISftp, IDisposable
{
    private readonly ConnectionInfo _connectionInfo;
    private readonly SftpClient _client;
    private readonly SshNetOptions _sshNetOptions;
    private const string SftpExample = "sftp://username:password@host:port";

    #region TODO private ━━━━━━━━━━━━━━━━━━━━━━

    private void EnsureClientConnected()
    {
        if (!_client.IsConnected) _client.Connect();
    }

    private ConnectionInfo CreateConnectionInfo(SshNetOptions options)
    {
        if (String.IsNullOrEmpty(options.ConnectionString))
        {
            options.ConnectionString = SftpExample.Replace("username", options.UserName)
                .Replace("password", options.PassWord)
                .Replace("host", options.Host)
                .Replace("port", options.Port.ToString());
        }

        if (!Uri.TryCreate(options.ConnectionString, UriKind.Absolute, out var uri) || String.IsNullOrEmpty(uri?.UserInfo))
            throw new ArgumentException("Unable to parse connection string uri", nameof(options.ConnectionString));

        string[] userParts = uri.UserInfo.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
        string username = Uri.UnescapeDataString(userParts.First());
        string password = Uri.UnescapeDataString(userParts.Length > 1 ? userParts[1] : String.Empty);
        // 默认端口: 22
        int port = uri.Port > 0 ? uri.Port : 22;

        var authenticationMethods = new List<AuthenticationMethod>();
        if (!String.IsNullOrEmpty(password))
            authenticationMethods.Add(new PasswordAuthenticationMethod(username, password));

        if (options.PrivateKey != null)
            authenticationMethods.Add(new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(options.PrivateKey, options.PrivateKeyPassPhrase)));

        if (authenticationMethods.Count == 0)
            authenticationMethods.Add(new NoneAuthenticationMethod(username));

        if (!String.IsNullOrEmpty(options.Proxy))
        {
            if (!Uri.TryCreate(options.Proxy, UriKind.Absolute, out var proxyUri) || String.IsNullOrEmpty(proxyUri?.UserInfo))
                throw new ArgumentException("Unable to parse proxy uri", nameof(options.Proxy));

            string[] proxyParts = proxyUri.UserInfo.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            string proxyUsername = proxyParts.First();
            string proxyPassword = proxyParts.Length > 1 ? proxyParts[1] : null;

            var proxyType = options.ProxyType;
            if (proxyType == ProxyTypes.None && proxyUri.Scheme != null && proxyUri.Scheme.StartsWith("http"))
                proxyType = ProxyTypes.Http;

            return new ConnectionInfo(uri.Host, port, username, proxyType, proxyUri.Host, proxyUri.Port, proxyUsername, proxyPassword, authenticationMethods.ToArray());
        }

        return new ConnectionInfo(uri.Host, port, username, authenticationMethods.ToArray());
    }

    private async Task GetFileListRecursivelyAsync(string path, ICollection<FileItem> list, CancellationToken cancellationToken = default)
    {
        System.Diagnostics.Debug.WriteLine($"━> `GetFileListRecursivelyAsync` Checking {path}...");
        if (cancellationToken.IsCancellationRequested) return;
        var files = new List<SftpFile>();
        try
        {
            files.AddRange(await _client.ListDirectoryAsync(path, null));
        }
        catch (SftpPathNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine($"━> `GetFileListRecursivelyAsync` ListDirectoryAsync Error: {ex.Message} 。");
            return;
        }

        foreach (var file in files.Where(f => f.IsRegularFile || f.IsDirectory).OrderBy(f => f.IsRegularFile).ThenBy(f => f.Name))
        {
            if (cancellationToken.IsCancellationRequested) return;
            if (file.IsDirectory)
            {
                if (file.Name is "." or "..") continue;
                await GetFileListRecursivelyAsync(String.Concat(path, "/", file.Name), list, cancellationToken);
                continue;
            }

            if (!file.IsRegularFile) continue;
            list.Add(new FileItem
            {
                Name = file.Name,
                ExtensionsName = file.FullName.GetFileExtensionsName(),
                Path = file.FullName,
                Created = file.LastWriteTimeUtc,
                Modified = file.LastWriteTimeUtc,
                Size = file.Length
            });
        }
    }

    private async Task<int> DeleteDirectory(string path, bool includeSelf)
    {
        int count = 0;
        foreach (var file in await _client.ListDirectoryAsync(path))
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                if (file.IsDirectory)
                {
                    count += await DeleteDirectory(file.FullName, true);
                }
                else
                {
                    _client.DeleteFile(file.FullName);
                    count++;
                }
            }
        }

        if (includeSelf) _client.DeleteDirectory(path);
        return count;
    }

    #endregion TODO private ━━━━━━━━━━━━━━━━━━━━━━

    public Sftp(IOptions<SshNetOptions> sshNetOptions)
    {
        if (sshNetOptions.Value == null)
            throw new ArgumentNullException(nameof(sshNetOptions));

        this._sshNetOptions = sshNetOptions.Value;
        this._connectionInfo = CreateConnectionInfo(this._sshNetOptions);
        this._client = new SftpClient(_connectionInfo);
    }

    public Sftp(SshNetOptions sshNetOptions)
    {
        if (sshNetOptions == null)
            throw new ArgumentNullException(nameof(sshNetOptions));

        this._sshNetOptions = sshNetOptions;
        this._connectionInfo = CreateConnectionInfo(this._sshNetOptions);
        this._client = new SftpClient(_connectionInfo);
    }

    public void Dispose()
    {
        if (_client.IsConnected) _client.Disconnect();
        _client.Dispose();
    }

    /// <inheritdoc/>
    public async Task CreateDirectory(string path)
    {
        if (String.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        string directory = (Path.GetDirectoryName(path)).NormalizePath();
        string[] folderSegments = directory.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string currentDirectory = String.Empty;

        EnsureClientConnected();
        foreach (string segment in folderSegments)
        {
            currentDirectory = String.Concat(currentDirectory, "/", segment);
            if (!_client.Exists(currentDirectory))
                _client.CreateDirectory(currentDirectory);
        }
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<Stream?> GetFileStreamAsync(string path)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (path.IsDirectory()) throw new Exception($"`{path}` is not file path.");
        EnsureClientConnected();
        try
        {
            if (!_client.Exists(path)) throw new Exception($"`{path}` file not exist.");
            var stream = new MemoryStream();
            await Task.Factory.FromAsync(_client.BeginDownloadFile(path.NormalizePath(), stream, null, null), _client.EndDownloadFile);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
        catch (SftpPathNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine($"━> `GetFileStreamAsync<Stream>` Path: {path},Error: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<FileItem?> GetFileAsync(string path)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        EnsureClientConnected();
        try
        {
            if (!_client.Exists(path)) throw new Exception($"`{path}` file not exist.");
            var file = _client.Get(path.NormalizePath());
            return await Task.FromResult(new FileItem()
            {
                Name = file.Name,
                ExtensionsName = file.FullName.GetFileExtensionsName(),
                Path = file.FullName,
                Created = file.LastWriteTimeUtc,
                Modified = file.LastWriteTimeUtc,
                Size = file.Length
            });
        }
        catch (SftpPathNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine("━> `GetFileAsync<FileItem>` Path: {Path},Error: {Message}.", path, ex.Message);
            return await Task.FromResult<FileItem?>(null);
        }
    }

    /// <inheritdoc/>
    public async Task<List<FileItem>> GetFilesAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return new List<FileItem>();
        var list = new List<FileItem>();
        if (!path.EndsWith("/")) path = path + "/";
        EnsureClientConnected();
        await GetFileListRecursivelyAsync(path, list);
        return list;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string path)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        EnsureClientConnected();
        return Task.FromResult(_client.Exists(path.NormalizePath()));
    }

    /// <inheritdoc/>
    public async Task<bool> SaveFileAsync(string path, Stream stream)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        path = path.NormalizePath();
        EnsureClientConnected();
        try
        {
            await Task.Factory.FromAsync(_client.BeginUploadFile(stream, path, null, null), _client.EndUploadFile);
        }
        catch (SftpPathNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine($"`SaveFileAsync` Path: {path},Error: {ex.Message},正在创建中...");
            await CreateDirectory(path);
            await Task.Factory.FromAsync(_client.BeginUploadFile(stream, path, null, null), _client.EndUploadFile);
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> SaveFileAsync(string path, string uploadFilePath) => await SaveFileAsync(path, uploadFilePath.ConvertToStream());

    /// <inheritdoc/>
    public async Task<bool> CopyFileAsync(string path, string targetPath)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (String.IsNullOrEmpty(targetPath)) throw new ArgumentNullException(nameof(targetPath));
        using var stream = await GetFileStreamAsync(path);
        if (stream == null) return false;
        return await SaveFileAsync(targetPath, stream);
    }

    /// <inheritdoc/>
    public async Task<bool> RenameFileAsync(string path, string newPath)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        if (String.IsNullOrEmpty(newPath)) throw new ArgumentNullException(nameof(newPath));
        newPath = newPath.NormalizePath();
        EnsureClientConnected();

        try
        {
            _client.RenameFile(path.NormalizePath(), newPath, true);
        }
        catch (SftpPathNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine("━> `RenameFileAsync` {Path},Error: {Message}.", path, ex.Message);
            await CreateDirectory(newPath);
            _client.RenameFile(path.NormalizePath(), newPath, true);
        }

        return await Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteFileAsync(string path)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        EnsureClientConnected();
        try
        {
            _client.DeleteFile(path.NormalizePath());
        }
        catch (SftpPathNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine("━> `DeleteFileAsync` {Path},Error: {Message}.", path, ex.Message);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task<int> BulkDeleteFileAsync(string? path)
    {
        EnsureClientConnected();

        if (path == null)
            return await DeleteDirectory("/", false);
        else if (path.EndsWith("/*"))
            return await DeleteDirectory(path.Substring(0, path.Length - 2), false);

        var files = await GetFilesAsync(path);
        int count = 0;
        foreach (var file in files)
        {
            await DeleteFileAsync(file.Path);
            count++;
        }

        return count;
    }

    /// <inheritdoc/>
    public async Task Down(string path, string targetPath)
    {
        if (String.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        var newTargetPath = targetPath.Replace(Path.GetFileName(targetPath), string.Empty);
        if (!Directory.Exists(newTargetPath)) Directory.CreateDirectory(newTargetPath);

        EnsureClientConnected();
        try
        {
            byte[] bytes = this._client.ReadAllBytes(path);
            File.WriteAllBytes(targetPath, bytes);
        }
        catch (SftpPathNotFoundException ex)
        {
            System.Diagnostics.Debug.WriteLine($"`Down` Path: {path},Error: {ex.Message}");
        }
        await Task.CompletedTask;
    }
}