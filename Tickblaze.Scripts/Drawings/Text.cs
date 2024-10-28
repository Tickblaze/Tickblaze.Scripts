namespace Tickblaze.Scripts.Drawings;

public class Text : Drawing
{
	[Parameter("Text", Description = "Characters to print to this text object")]
	public string Value { get; set; } = "Text";

	[Parameter("Vertical alignment", Description = "Vertical alignment of text within the text box")]
	public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Bottom;

	[Parameter("Horizontal alignment", Description = "Horizontal alignment of text within text box")]
	public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Right;

	[Parameter("Font", Description = "Font name and size")]
	public Font Font { get; set; } = new("Segoe UI", 13);

	[Parameter("Foreground", Description = "Color and opacity of text")]
	public Color Foreground { get; set; } = Color.TealGreen;

	[Parameter("Background", Description = "Color and opacity of background behind text")]
	public Color Background { get; set; } = Color.Transparent;

	public override int PointsCount => 1;

	public override void OnRender(IDrawingContext context)
	{
		var origin = new Point(Points[0]);
		var text = Value.ToString();
		var textSize = context.MeasureText(text, Font);
		var margin = new Point(3, 1);

		origin.X -= HorizontalAlignment switch
		{
			HorizontalAlignment.Left => textSize.Width + margin.X,
			HorizontalAlignment.Center => (textSize.Width + margin.X) / 2,
			HorizontalAlignment.Right => -margin.X,
			_ => throw new NotImplementedException()
		};

		origin.Y -= VerticalAlignment switch
		{
			VerticalAlignment.Top => textSize.Height + margin.Y,
			VerticalAlignment.Center => (textSize.Height + margin.Y) / 2,
			VerticalAlignment.Bottom => -margin.Y,
			_ => throw new NotImplementedException()
		};

		context.DrawRectangle(origin - margin, textSize.Width + margin.X * 2, textSize.Height + margin.Y * 2, Background);
		context.DrawText(origin, text, Foreground, Font);
	}
}
