namespace Assemblers;

/// <summary>
/// `FileUtil` 文件操作工具扩展类
/// </summary>
public static class FileUtil
{
    /// <summary>
    /// 文件大小单位转换 `Byte/K/M/G`
    /// </summary>
    /// <param name="length"> Length </param>
    /// <returns> string </returns>
    public static string ConvertToFileSizeUnit(this long length)
    {
        long factSize = 0;
        factSize = length;
        string sizecn = string.Empty;

        if (factSize < 1024.00)
            sizecn = factSize.ToString("F2") + " Byte";
        else if (factSize >= 1024.00 && factSize < 1048576)
            sizecn = (factSize / 1024.00).ToString("F2") + " K";
        else if (factSize >= 1048576 && factSize < 1073741824)
            sizecn = (factSize / 1024.00 / 1024.00).ToString("F2") + " M";
        else if (factSize >= 1073741824)
            sizecn = (factSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " G";
        return sizecn;
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="path"> 文件目录 </param>
    /// <returns> string </returns>
    public static string GetFileExtensionsName(this string path)
        => string.IsNullOrWhiteSpace(path) ? String.Empty
        : path.Substring(path.LastIndexOf('.')).ToLower().Trim();

    /// <summary>
    /// 获取文件名
    /// </summary>
    /// <param name="path"> 文件目录 </param>
    /// <returns> string </returns>
    public static string GetFileName(this string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        return Path.GetFileName(path);
    }

    /// <summary>
    /// 是否为目录
    /// </summary>
    /// <param name="path"> 路径(文件/目录) </param>
    /// <returns> bool </returns>
    public static bool IsDirectory(this string path) => string.IsNullOrWhiteSpace(new FileInfo(path).Extension);

    /// <summary>
    /// 目录统一(`linux 为: /`)
    /// </summary>
    /// <param name="path"> 目录 </param>
    /// <returns> string </returns>
    public static string NormalizePath(this string path) => path.Replace('\\', '/');

    /// <summary>
    /// 获取当前目录下所有文件夹
    /// </summary>
    /// <param name="path"> 目录 </param>
    /// <returns> Array </returns>
    public static string[] GetDirectories(this string path) => Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

    /// <summary>
    /// 获取当前目录下所有文件
    /// </summary>
    /// <param name="path"> 目录 </param>
    /// <returns> Array </returns>
    public static string[] GetFiles(this string path) => Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

    /// <summary>
    /// 文件转换文件流
    /// </summary>
    /// <param name="path"> 文件目录 </param>
    /// <returns></returns>
    public static Stream ConvertToStream(this string path)
    {
        using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();
        return new MemoryStream(bytes);
    }
}