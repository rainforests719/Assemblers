namespace Assemblers.SshNet;

/// <summary>
/// `Stp` 操作
/// </summary>
public interface ISftp
{
    /// <summary>
    /// 创建文件夹
    /// </summary>
    /// <param name="path"> 文件夹目录 </param>
    /// <returns></returns>
    Task CreateDirectory(string path);

    /// <summary>
    /// 读取文件流 `Stream`
    /// </summary>
    /// <param name="path"> 文件路径 </param>
    /// <returns> Stream </returns>
    Task<Stream?> GetFileStreamAsync(string path);

    /// <summary>
    /// 读取文件信息
    /// </summary>
    /// <param name="path"> 文件路径 </param>
    /// <returns> FileInfo </returns>
    Task<FileItem?> GetFileAsync(string path);

    /// <summary>
    /// 读取当前目录所有文件
    /// </summary>
    /// <param name="path"> 文件路径 </param>
    /// <returns> FileItems </returns>
    Task<List<FileItem>> GetFilesAsync(string path);

    /// <summary>
    /// 文件是否存在
    /// </summary>
    /// <param name="path"> 文件路径 </param>
    /// <returns> bool </returns>
    Task<bool> ExistsAsync(string path);

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="path"> 文件存放路径(绝对路径) </param>
    /// <param name="stream"> 流 </param>
    /// <returns> bool </returns>
    Task<bool> SaveFileAsync(string path, Stream stream);

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="path"> 文件存放路径(绝对路径) </param>
    /// <param name="uploadFilePath"> 上传文件路径 </param>
    /// <returns> bool </returns>
    Task<bool> SaveFileAsync(string path, string uploadFilePath);

    /// <summary>
    /// 文件重命名
    /// </summary>
    /// <param name="path"> 文件路径 </param>
    /// <param name="newPath"> 新的文件路径 </param>
    /// <param name="cancellationToken"></param>
    /// <returns> bool </returns>
    Task<bool> RenameFileAsync(string path, string newPath);

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="path"> 当前文件路径 </param>
    /// <param name="targetPath"> 目标文件路径 </param>
    /// <param name="cancellationToken"></param>
    /// <returns> bool </returns>
    Task<bool> CopyFileAsync(string path, string targetPath);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path"> 文件路径 </param>
    /// <param name="cancellationToken"></param>
    /// <returns> bool </returns>
    Task<bool> DeleteFileAsync(string path);

    /// <summary>
    /// 批量删除文件
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<int> BulkDeleteFileAsync(string? path);

    /// <summary>
    /// 文件下载
    /// </summary>
    /// <param name="path"> 文件路径 </param>
    /// <param name="targetPath"> 文件存放路径 </param>
    /// <returns></returns>
    Task Down(string path, string targetPath);
}