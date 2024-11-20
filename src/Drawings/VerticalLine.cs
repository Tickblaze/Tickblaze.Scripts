namespace Tickblaze.Scripts.Drawings;

public sealed class VerticalLine : Line
{
	public override int PointsCount => 1;

	public VerticalLine()
	{
		Name = "Vertical Line";
	}

	public override void OnRender(IDrawingContext context)
	{
		var x = Points[0].X;
		var pointA = new Point(x, 0);
		var pointB = new Point(x, Chart.Height);

		context.DrawLine(pointA, pointB, Color, Thickness, LineStyle);
	}
}
