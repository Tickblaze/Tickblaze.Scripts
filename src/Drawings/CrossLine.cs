namespace Tickblaze.Scripts.Drawings;

public sealed class CrossLine : Line
{
	public override int PointsCount => 1;

	public CrossLine()
	{
		Name = "Cross Line";
	}

	public override void OnRender(IDrawingContext context)
	{
		var x = PointA.X;
		var y = PointA.Y;

		var points = new Point[]
		{
			new(0,y),
			new(Chart.Width,y),
			new(x, 0),
			new(x, Chart.Height)
		};

		context.DrawLine(points[0], points[1], Color, Thickness, LineStyle);
		context.DrawLine(points[2], points[3], Color, Thickness, LineStyle);
	}
}
