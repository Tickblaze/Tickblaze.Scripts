namespace Tickblaze.Scripts.Drawings;

public sealed class FibonacciExtension : Drawing
{
	[Parameter("Trend line color", Description = "Color of the anchor line that connects the drawing tool handle points")]
	public Color TrendLineColor { get; set; } = Color.Gray;

	[Parameter("Trend line thickness", Description = "Thickness of the anchor line that connects the handle points"), NumericRange(1)]
	public int TrendLineThickness { get; set; } = 1;

	[Parameter("Trend line style", Description = "Line style of the anshore line that connects the handle points")]
	public LineStyle TrendLineStyle { get; set; } = LineStyle.Dash;

	[Parameter("Level thickness", Description = "Thickness of each calculated fib level"), NumericRange(1)]
	public int LevelThickness { get; set; } = 1;

	[Parameter("Level style", Description = "Line style of each calculated fib level")]
	public LineStyle LevelStyle { get; set; } = LineStyle.Solid;

	[Parameter("Extend levels right", Description = "Extend the calculated fib levels to the right edge")]
	public bool ExtendLevelsRight { get; set; }

	[Parameter("Extend levels left", Description = "Extend the calculated fib levels to the left edge")]
	public bool ExtendLevelsLeft { get; set; }

	[Parameter("Font", Description = "Font used for the price text on each calculated fib level")]
	public Font TextFont { get; set; } = new("Arial", 12);

	[Parameter("Level #0", Description = "Fib ratio of Level #0, Example 0.38 is a 38% extension")]
	public double LevelRatio0 { get; set; } = 0;

	[Parameter("Color #0", Description = "Color of Level #0")]
	public Color LevelColor0 { get; set; } = "#787b86";

	[Parameter("Level #1", Description = "Fib ratio of Level #1, Example 0.38 is a 38% extension")]
	public double LevelRatio1 { get; set; } = 0.236;

	[Parameter("Color #1", Description = "Color of Level #1")]
	public Color LevelColor1 { get; set; } = "#f23645";

	[Parameter("Level #2", Description = "Fib ratio of Level #2, Example 0.38 is a 38% extension")]
	public double LevelRatio2 { get; set; } = 0.5;

	[Parameter("Color #2", Description = "Color of Level #2")]
	public Color LevelColor2 { get; set; } = "#4caf50";

	[Parameter("Level #3", Description = "Fib ratio of Level #3, Example 0.38 is a 38% extension")]
	public double LevelRatio3 { get; set; } = 0.618;

	[Parameter("Color #3", Description = "Color of Level #3")]
	public Color LevelColor3 { get; set; } = "#089981";

	[Parameter("Level #4", Description = "Fib ratio of Level #4, Example 0.38 is a 38% extension")]
	public double LevelRatio4 { get; set; } = 1;

	[Parameter("Color #4", Description = "Color of Level #4")]
	public Color LevelColor4 { get; set; } = "#787b86";

	[Parameter("Level #5", Description = "Fib ratio of Level #5, Example 0.38 is a 38% extension")]
	public double LevelRatio5 { get; set; } = 1.618;

	[Parameter("Color #5", Description = "Color of Level #5")]
	public Color LevelColor5 { get; set; } = "#2962ff";

	public override int PointsCount => 3;

	private record Level(double Ratio, Color Color);

	public FibonacciExtension()
	{
		Name = "Fibonacci Extension";
	}

	public override void OnRender(IDrawingContext context)
	{
		var pointA = Points[0];
		var pointB = Points[1];
		var pointC = Points.Count > 2 ? Points[2] : null;

		if (pointC is null)
		{
			context.DrawLine(pointA, pointB, TrendLineColor, TrendLineThickness, TrendLineStyle);

			return;
		}

		context.DrawPolygon([pointA, pointB, pointC], null, TrendLineColor, TrendLineThickness, TrendLineStyle);

		var range = pointB.Y - pointA.Y;
		var levels = new Level[]
		{
			new(LevelRatio0, LevelColor0),
			new(LevelRatio1, LevelColor1),
			new(LevelRatio2, LevelColor2),
			new(LevelRatio3, LevelColor3),
			new(LevelRatio4, LevelColor4),
			new(LevelRatio5, LevelColor5)
		};

		foreach (var level in levels)
		{
			var y = pointC.Y + (level.Ratio * range);
			var levelPointA = new Point(Math.Min(pointB.X, pointC.X), y);
			var levelPointB = new Point(Math.Max(pointB.X, pointC.X), y);

			var value = ChartScale.GetValueByYCoordinate(y);
			var text = $"{level.Ratio:P2} ({ChartScale.FormatPrice(value)})";
			var textSize = context.MeasureText(text, TextFont);
			var textOrigin = new Point(levelPointB.X, y - textSize.Height);

			if (ExtendLevelsRight)
			{
				levelPointB.X = Chart.Width;
				textOrigin.X = levelPointB.X - textSize.Width;
			}

			if (ExtendLevelsLeft)
			{
				levelPointA.X = 0;
			}

			context.DrawLine(levelPointA, levelPointB, level.Color, LevelThickness, LevelStyle);
			context.DrawText(textOrigin, text, level.Color, TextFont);
		}
	}
}
