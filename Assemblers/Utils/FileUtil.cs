namespace Assemblers;

/// <summary>
/// `FileUtil` 文件操作工具扩展类
/// </summary>
public static class FileUtil
{
    /// <summary>
    /// 文件大小单位转换 `M/G`
    /// </summary>
    /// <param name="length"> Length </param>
    /// <returns> string </returns>
    public static string ConvertToFileSizeDisplay(this long i, int decimals = 2)
    {
        if (i < 1024 * 1024 * 1024)
        {
            string value = Math.Round((decimal)i / 1024m / 1024m, decimals).ToString("N" + decimals);
            if (decimals > 0 && value.EndsWith(new string('0', decimals)))
                value = value.Substring(0, value.Length - decimals - 1);

            return String.Concat(value, " MB");
        }
        else
        {
            string value = Math.Round((decimal)i / 1024m / 1024m / 1024m, decimals).ToString("N" + decimals);
            if (decimals > 0 && value.EndsWith(new string('0', decimals)))
                value = value.Substring(0, value.Length - decimals - 1);

            return String.Concat(value, " GB");
        }
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
    public static string NormalizePath(this string path) => Path.Combine(path).Replace(@"\", "/").Replace("\\", "/").Replace("//", "/");

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