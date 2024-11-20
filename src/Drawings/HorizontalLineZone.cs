namespace Tickblaze.Scripts.Drawings;

public sealed class HorizontalLineZone : HorizontalZone
{
	[Parameter("Line Color", Description = "Color of the horizontal center line of the zone")]
	public Color LineColor { get; set; } = Color.White;

	[Parameter("Line Thickness", Description = "Thickness of the horizontal center line of the zone"), NumericRange(1, 10)]
	public int LineThickness { get; set; } = 1;

	[Parameter("Line Style", Description = "Line style of the horizontal center line of the zone")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Size Type", Description = "Type of 'Size Value' to determine the vertical size of the zone")]
	public SizeType Size { get; set; } = SizeType.Ticks;

	[Parameter("Size Value", Description = "Vertical size dimension of the zone above the center line, and below the center line.  E.g. a 10-tick is zone of overall height 20-ticks")]
	public double SizeValue { get; set; } = 5;

	public override int PointsCount => 1;

	public enum SizeType
	{
		Points,
		Ticks
	}

	public HorizontalLineZone()
	{
		Name = "Horizontal LZ";
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		Points[0].Value = Symbol.RoundToTick((double)yDataValue);
	}

	public override void OnRender(IDrawingContext context)
	{
		var midPoint = Points[0];
		var midPrice = (double)midPoint.Value;

		var zoneOffset = SizeValue * (Size is SizeType.Ticks ? Symbol.TickSize : 1);
		var upperPrice = Symbol.RoundToTick(midPrice + zoneOffset);
		var lowerPrice = Symbol.RoundToTick(midPrice - zoneOffset);

		var upperPoint = new ChartPoint()
		{
			Time = midPoint.Time,
			Value = upperPrice,
			X = midPoint.X,
			Y = ChartScale.GetYCoordinateByValue(upperPrice)
		};

		var lowerPoint = new ChartPoint()
		{
			Time = midPoint.Time,
			Value = lowerPrice,
			X = midPoint.X,
			Y = ChartScale.GetYCoordinateByValue(lowerPrice)
		};

		context.DrawExtendedLine(midPoint, new Point(midPoint.X + 10, midPoint.Y), LineColor, LineThickness);
		DrawZone(context, upperPoint, lowerPoint);
	}
}
