namespace Tickblaze.Scripts.Drawings;

public class TrendLine : Line
{
	[Parameter("Extend right", Description = "Extend trendline beyond 2nd point")]
	public bool ExtendRight { get; set; }

	[Parameter("Extend left", Description = "Extend trendline beyond 1st point")]
	public bool ExtendLeft { get; set; }

	public TrendLine()
	{
		Name = "Trend Line";
	}

	public override void OnRender(IDrawingContext context)
	{
		if (ExtendLeft && ExtendRight)
		{
			context.DrawExtendedLine(PointA, PointB, Color, Thickness, LineStyle);
		}
		else if (ExtendRight)
		{
			context.DrawRay(PointA, PointB, Color, Thickness, LineStyle);
		}
		else if (ExtendLeft)
		{
			context.DrawRay(PointB, PointA, Color, Thickness, LineStyle);
		}
		else
		{
			context.DrawLine(PointA, PointB, Color, Thickness, LineStyle);
		}
	}
}
