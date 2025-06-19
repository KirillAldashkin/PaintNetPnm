using PaintDotNet;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using PaintDotNet.Rendering;
using PaintNetPnm.Locale;
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
        Channel,
        GitHubLink
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
        StaticListChoiceProperty.CreateForEnum(Property.Channel, Channel.R, false),
        new UriProperty(Property.GitHubLink, new("https://github.com/KirillAldashkin/PaintNetPnm"))
    ], [
        new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(Property.Channel, Property.Format, PnmFormat.P6)
    ]);

    public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
    {
        var info = CreateDefaultSaveConfigUI(props);

        var format = info.FindControlForPropertyName(Property.Channel)!;
        format.ControlProperties[ControlInfoPropertyNames.DisplayName]!.Value = Strings.SaveDialog_Channel;
        format.SetValueDisplayName(Channel.R, Strings.SaveDialog_Channel_R);
        format.SetValueDisplayName(Channel.G, Strings.SaveDialog_Channel_G);
        format.SetValueDisplayName(Channel.B, Strings.SaveDialog_Channel_B);
        format.SetValueDisplayName(Channel.A, Strings.SaveDialog_Channel_A);

        info.SetPropertyControlValue(Property.Format, ControlInfoPropertyNames.DisplayName, Strings.SaveDialog_Format);
        info.SetPropertyControlValue(Property.GitHubLink, ControlInfoPropertyNames.DisplayName, Strings.SaveDialog_GitHubLink);
        return info;
    }

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
