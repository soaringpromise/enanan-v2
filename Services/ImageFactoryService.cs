using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace EnananV2.Services;

public class ImageFactoryService
{
    private readonly SemaphoreSlim _renderSemaphore = new(2);

    private const string ColorLight = "#FBFBFB";
    private const string ColorAsh = "#323339";
    private const string ColorDark = "#1A1A1E";
    private const string ColorOnyx = "#070709";

    private const string TextLight = "#DCDCDF";
    private const string TextDark = "#323339";
    private const string TextTimestamp = "#9D9EA5";

    private const string TagBackgroundLight = "#E4E4E6";
    private const string TagBackgroundDark = "#4A4B50";

    private static readonly string[] Lines =
    [
        "Was this the right choice?",
        "Where did I take a wrong turn?",
        "This warmth in my chest…",
        "Why is it not cooling down?"
    ];

    private static readonly ImageGenerationSettings DefaultSettings = new()
    {
        RasterDpi = 144,
        ImageFormat = ImageFormat.Png,
        ImageCompressionQuality = ImageCompressionQuality.High,
        UseTransparentBackground = true
    };

    public async Task<byte[]> GenerateNamePreviewAsync(
        string username, string color, byte[] avatarBytes, byte[]? decorationBytes, string? guildTag, byte[]? guildBadgeBytes)
    {
        await _renderSemaphore.WaitAsync();

        try
        {
            return await Task.Run(() =>
                GenerateNamePreview(username, color, avatarBytes, decorationBytes, guildTag, guildBadgeBytes));
        }
        finally
        {
            _renderSemaphore.Release();
        }
    }
    
    public async Task<byte[]> GenerateColorPreviewAsync(string name, int color)
    {
        await _renderSemaphore.WaitAsync();

        try
        {
            return await Task.Run(() => GenerateColorPreview(name, color));
        }
        finally
        {
            _renderSemaphore.Release();
        }
    }

    private static byte[] GenerateNamePreview(
        string username, string roleColor, byte[] avatarBytes, byte[]? decorationBytes, string? guildTag, byte[]? guildBadgeBytes)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                var textStyle = TextStyle.Default.FontFamily(
                    "gg sans",
                    "Segoe UI",
                    "Segoe UI Variable Small Light",
                    "Segoe UI Light",
                    "Segoe UI Semibold",
                    "Segoe UI Semilight",
                    "Segoe UI Historic",
                    "Segoe UI Symbol",
                    "Segoe UI Fluent Icons",
                    "Segoe UI Emoji"
                );
                page.DefaultTextStyle(textStyle);
                page.ContinuousSize(600);
                page.Margin(12);

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorLight, TextDark, 
                        username, roleColor, avatarBytes, decorationBytes, guildTag, guildBadgeBytes,
                        true, 0));
                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorAsh, TextLight,
                        username, roleColor, avatarBytes, decorationBytes, guildTag, guildBadgeBytes,
                        false, 1));
                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorDark, TextLight,
                        username, roleColor, avatarBytes, decorationBytes, guildTag, guildBadgeBytes,
                        false, 2));
                    col.Item().Element(e => RenderFakeMessage(
                        e, ColorOnyx, TextLight,
                        username, roleColor, avatarBytes, decorationBytes, guildTag, guildBadgeBytes,
                        false, 3));
                });
            });
        });
        return document.GenerateImages(DefaultSettings).First();
    }

    private static void RenderFakeMessage(
        IContainer container,
        string bgHex,
        string textHex,
        string username,
        string roleColor,
        byte[] avatar,
        byte[]? decoration,
        string? guildTag,
        byte[]? guildBadge,
        bool lightTheme,
        int index)
    {
        const float avatarSize = 45;
        const float decorationSize = 54;
        const float decorationOffset = (decorationSize - avatarSize) / 2f;

        container
            .CornerRadius(8)
            .Background(bgHex)
            .Padding(16)
            .Row(row =>
            {
                row.Spacing(16);

                row.ConstantItem(decorationSize)
                    .Height(decorationSize)
                    .Layers(layers =>
                    {
                        layers.PrimaryLayer()
                            .Padding(decorationOffset)
                            .CornerRadius(avatarSize / 2f)
                            .Image(avatar)
                            .FitArea();

                        if (decoration is not null)
                        {
                            layers.Layer()
                                .Image(decoration)
                                .FitArea();
                        }
                    });

                row.RelativeItem().Column(col =>
                {
                    col.Item()
                        .PaddingTop(4)
                        .PaddingLeft(1)
                        .Row(header =>
                        {
                            header.Spacing(6);

                            header.AutoItem()
                                .MinHeight(20)
                                .AlignBottom()
                                .Text(username)
                                .LineHeight(1.2f)
                                .FontColor(roleColor)
                                .FontSize(17.5f)
                                .SemiBold()
                                .FontFamily("gg sans");

                            if (!string.IsNullOrWhiteSpace(guildTag) &&
                                guildBadge is not null)
                            {
                                header.AutoItem()
                                    .AlignBottom()
                                    .Element(tag => RenderGuildTag(
                                        tag,
                                        guildTag,
                                        guildBadge,
                                        lightTheme));
                            }

                            header.AutoItem()
                                .PaddingLeft(2)
                                .MinHeight(20)
                                .AlignBottom()
                                .Text("1:00 AM")
                                .FontColor(TextTimestamp)
                                .FontSize(14)
                                .FontFamily("gg sans");
                        });

                    col.Item()
                        .PaddingTop(2)
                        .Text(Lines[index])
                        .FontColor(textHex)
                        .FontSize(17.5f)
                        .FontFamily("gg sans")
                        .NormalWeight();
                });
            });
    }

    private static void RenderGuildTag(
        IContainer container,
        string guildTag,
        byte[] guildBadge,
        bool lightTheme)
    {
        container
            .Background(
                lightTheme
                    ? TagBackgroundLight
                    : TagBackgroundDark)
            .CornerRadius(6)
            .PaddingHorizontal(4.5f)
            .PaddingBottom(1.75f)
            .Row(row =>
            {
                row.Spacing(1);

                row.ConstantItem(15)
                    .Height(14.5f)
                    .PaddingTop(1)
                    .PaddingLeft(-1)
                    .PaddingBottom(-2)
                    .AlignBottom()
                    .Image(guildBadge)
                    .FitArea();

                row.AutoItem()
                    .AlignBottom()
                    .PaddingBottom(-1)
                    .Text(guildTag)
                    .FontSize(13)
                    .Bold()
                    .FontColor(
                        lightTheme
                            ? TextDark
                            : TextLight)
                    .FontFamily("gg sans");
            });
    }
    
    private static byte[] GenerateColorPreview(string name, int color)
    {
        var colorHex = $"#{color:X6}";
        var textColor = GetContrastingTextColor(color);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(300, 300);
                page.Margin(12);

                page.DefaultTextStyle(
                    TextStyle.Default.FontFamily(
                        "gg sans",
                        "Segoe UI",
                        "Segoe UI Variable",
                        "Segoe UI Emoji"
                    )
                );

                page.Content()
                    .CornerRadius(24)
                    .Background(colorHex)
                    .AlignCenter()
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Spacing(4);

                        column.Item()
                            .AlignCenter()
                            .Text(name)
                            .FontSize(24)
                            .SemiBold()
                            .FontColor(textColor);

                        column.Item()
                            .AlignCenter()
                            .Text(colorHex.ToUpperInvariant())
                            .FontSize(16)
                            .FontColor(textColor);
                    });
            });
        });

        return document.GenerateImages(DefaultSettings).First();
    }

    private static string GetContrastingTextColor(int color)
    {
        var red = (color >> 16) & 0xFF;
        var green = (color >> 8) & 0xFF;
        var blue = color & 0xFF;

        var luminance = (red * 299 + green * 587 + blue * 114) / 1000;

        return luminance >= 150
            ? ColorOnyx
            : ColorLight;
    }
}