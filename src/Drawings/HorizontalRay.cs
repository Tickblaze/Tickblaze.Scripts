namespace Tickblaze.Scripts.Drawings;

public sealed class HorizontalRay : Line
{
	public override int PointsCount => 1;

	public HorizontalRay()
	{
		Name = "Horizontal Ray";
	}

	public override void OnRender(IDrawingContext context)
	{
		var pointA = Points[0];
		var pointB = new Point(Chart.Width, pointA.Y);

		context.DrawLine(pointA, pointB, Color, Thickness);
	}
}
