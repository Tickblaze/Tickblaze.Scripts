namespace Tickblaze.Scripts.Drawings;

public sealed class Rectangle : Drawing
{
	[Parameter("Background", Description = "Color and opacity of the rectangle fill")]
	public Color BackgroundColor { get; set; } = "#339c27b0";

	[Parameter("Border color", Description = "Color and opacity of the border lines")]
	public Color BorderColor { get; set; } = "#9c27b0";

	[Parameter("Border thickness", Description = "Thickness of the border lines"), NumericRange(1)]
	public int BorderThickness { get; set; } = 1;

	[Parameter("Border line style", Description = "Line style for the border lines")]
	public LineStyle BorderLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Extend right", Description = "Extend the border lines to the right of the chart")]
	public bool ExtendRight { get; set; }

	[Parameter("Extend left", Description = "Extend the border lines to the left of the chart")]
	public bool ExtendLeft { get; set; }

	public override int PointsCount => !IsCreated ? 2 : 4;

	public IChartPoint PointA => Points[0];
	public IChartPoint PointB => Points[2];
	public IChartPoint PointC => Points[1];
	public IChartPoint PointD => Points[3];

	public override void OnCreated()
	{
		Points.Add(PointC.Time, PointA.Value);
		Points.Add(PointA.Time, PointC.Value);
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count < 4)
		{
			return;
		}

		if (index == PointA.Index)
		{
			PointB.Value = yDataValue;
			PointD.Time = xDataValue;
		}
		else if (index == PointB.Index)
		{
			PointA.Value = yDataValue;
			PointC.Time = xDataValue;
		}
		else if (index == PointC.Index)
		{
			PointB.Time = xDataValue;
			PointD.Value = yDataValue;
		}
		else if (index == PointD.Index)
		{
			PointA.Time = xDataValue;
			PointC.Value = yDataValue;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		var pointA = new Point(Math.Min(PointA.X, PointC.X), Math.Min(PointA.Y, PointC.Y));
		var pointB = new Point(Math.Max(PointA.X, PointC.X), Math.Max(PointA.Y, PointC.Y));

		if (ExtendLeft)
		{
			pointA.X = -BorderThickness;
		}

		if (ExtendRight)
		{
			pointB.X = Chart.Width + BorderThickness;
		}

		context.DrawRectangle(pointA, pointB, BackgroundColor, BorderColor, BorderThickness, BorderLineStyle);
	}
}
