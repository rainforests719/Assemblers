namespace Assemblers;

/// <summary>
/// `DateTime` 扩展工具类
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <returns> string </returns>
    public static string To10UnixTime()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }

    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <returns> string </returns>
    public static string To13UnixTime()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds).ToString();
    }

    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <returns> string </returns>
    public static string To10UnixTime(this DateTime dateTime)
    {
        TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }

    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <returns> string </returns>
    public static string To13UnixTime(this DateTime dateTime)
    {
        TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds).ToString();
    }
}