using PaintDotNet;
using PaintNetPnm;

Surface result;
Console.Write("Input path: ");
var path = Console.ReadLine() ?? throw new("null stdio result");
{
    using var file = File.OpenRead(path);
    if (!PnmDecoder.TryRead(file, out result, out var error)) throw new(error);
}
{
    path = Path.ChangeExtension(path, ".out.pnm");
    Console.WriteLine($"Output path: {path}");
    using var file = File.Create(path);
    PnmEncoder.WriteP6(file, result, (a, b) => { });
}