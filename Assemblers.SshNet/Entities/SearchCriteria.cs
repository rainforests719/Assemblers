namespace Assemblers.SshNet;

/// <summary>
/// 搜索条件
/// </summary>
internal class SearchCriteria
{
    public string Prefix { get; set; }
    public Regex Pattern { get; set; }
}
