namespace Tickblaze.Scripts.Drawings;

public abstract class Line : Drawing
{
	[Parameter("Color", Description = "Color and opacity of the line")]
	public Color Color { get; set; } = "#389d44";

	[Parameter("Thickness", Description = "Thickness of the line"), NumericRange(1, 5)]
	public int Thickness { get; set; } = 1;

	[Parameter("Line Style", Description = "Line style of the line")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => 2;

	public IChartPoint PointA => Points[0];
	public IChartPoint PointB => Points[1];
}
