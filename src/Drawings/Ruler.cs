namespace Tickblaze.Scripts.Drawings;

public partial class Ruler : Drawing
{
	[Parameter("Line Color", Description = "Color and opacity of the drawn lines")]
	public Color LineColor { get; set; } = Color.Gray;

	[Parameter("Line Thickness", Description = "Thickness of the drawn lines"), NumericRange(1, 10)]
	public int LineThickness { get; set; } = 1;

	[Parameter("Lines style", Description = "Line style of the drawn lines")]
	public LineStyle LineStyle { get; set; } = LineStyle.Dot;

	[Parameter("Text Font", Description = "Font name and size for the text")]
	public Font TextFont { get; set; } = new("Arial", 10);

	[Parameter("Text Foreground", Description = "Color and opacity of the text")]
	public Color TextForeground { get; set; } = Color.Silver;

	[Parameter("Text Background", Description = "Color and opacity of the background fill")]
	public Color TextBackground { get; set; } = "#33696969";

	public override int PointsCount => 3;
	public IChartPoint PointA => Points[0];
	public IChartPoint PointB => Points[1];
	public IChartPoint PointC => Points[2];

	public override void OnRender(IDrawingContext context)
	{
		context.DrawPolygon(Points, null, LineColor, LineThickness, LineStyle);

		if (Points.Count < PointsCount)
		{
			return;
		}

		var price = new[] { (double)PointA.Value, (double)PointB.Value };
		var change = price[1] - price[0];
		var ticks = (int)Math.Round(Symbol.RoundToTick(change) / Symbol.TickSize);
		var bars = Chart.GetBarIndexByXCoordinate(PointB.X) - Chart.GetBarIndexByXCoordinate(PointA.X);
		var time = ((DateTime)PointB.Time).Subtract((DateTime)PointA.Time);
		var text = $"Bars:\t{bars}\nTime:\t{time}\nChange:\t{ChartScale.FormatPrice(change)}\nTicks:\t{ticks}";
		var textSize = context.MeasureText(text, TextFont);
		var textMargin = 5;
		var textOrigin = new Point(PointC.X + textMargin, PointC.Y + textMargin);
		var rectPointB = new Point(PointC.X + textSize.Width + textMargin * 2, PointC.Y + textSize.Height + textMargin * 2);

		if (PointB.X > PointC.X)
		{
			textOrigin.X -= textSize.Width + textMargin * 2;
			rectPointB.X = textOrigin.X - textMargin;
		}

		if (PointB.Y > PointC.Y)
		{
			textOrigin.Y -= textSize.Height + textMargin * 2;
			rectPointB.Y = textOrigin.Y - textMargin;
		}

		context.DrawRectangle(PointC, rectPointB, TextBackground, LineColor, LineThickness);
		context.DrawText(textOrigin, text, TextForeground, TextFont);
	}
}
