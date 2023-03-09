namespace Assemblers.SshNet;

public class FileItem
{
    public string Name { get; set; }
    public string ExtensionsName { get; set; }
    public string Path { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public long Size { get; set; }
}