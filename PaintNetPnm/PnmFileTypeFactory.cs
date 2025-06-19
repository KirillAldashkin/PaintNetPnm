using PaintDotNet;

namespace PaintNetPnm;

public sealed class PnmFileTypeFactory : IFileTypeFactory2
{
    public FileType[] GetFileTypeInstances(IFileTypeHost host) => [new PnmFileType(host)];
}