using PaintDotNet;
using PaintDotNet.Direct2D1.Effects;
using PaintDotNet.PropertySystem;
using PaintDotNet.Rendering;
using System.Drawing.Imaging.Effects;
using System.IO;

namespace PaintNetPnm;

[PluginSupportInfo(typeof(PluginSupportInfo))]
public sealed class PnmFileType(IFileTypeHost _) : PropertyBasedFileType(
    "Pnm", 
    new FileTypeOptions
    {
        SupportsLayers = false,
        LoadExtensions = [ ".pnm" ],
        SaveExtensions = [ ".pnm" ]
    }
)
{
    private enum Property
    {
        Format,
        Channel
    }

    protected override Document OnLoad(Stream input)
    {
        if (!PnmDecoder.TryRead(input, out var surface, out var error)) throw new(error);

        var document = new Document(surface.Size);
        document.Layers.Add(Layer.CreateBackgroundLayer(surface, true));
        return document;
    }

    public override PropertyCollection OnCreateSavePropertyCollection() => new([
        StaticListChoiceProperty.CreateForEnum(Property.Format, PnmFormat.P6, false),
        StaticListChoiceProperty.CreateForEnum(Property.Channel, Channel.R, false)
    ], [
        new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(Property.Channel, Property.Format, PnmFormat.P6)
    ]);

    protected override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progress)
    {
        var format = (PnmFormat)token.GetProperty<StaticListChoiceProperty>(Property.Format)!.Value;
        var channel = (Channel)token.GetProperty<StaticListChoiceProperty>(Property.Channel)!.Value;

        scratchSurface.Clear();
        input.CreateRenderer().Render(scratchSurface);

        void prog(int done, int total) => progress(this, new(done * 100.0 / total));
        switch (format)
        {
            case PnmFormat.P5:
                PnmEncoder.WriteP5(output, scratchSurface, channel, prog);
                break;
            case PnmFormat.P6:
                PnmEncoder.WriteP6(output, scratchSurface, prog);
                break;
            default:
                break;
        }
    }
}
