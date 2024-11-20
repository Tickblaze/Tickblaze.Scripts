namespace Tickblaze.Scripts.Drawings;

public sealed class HorizontalLine : Line
{
	public override int PointsCount => 1;

	public HorizontalLine()
	{
		Name = "Horizontal Line";
	}

	public override void OnRender(IDrawingContext context)
	{
		var y = Points[0].Y;
		var p0 = new Point(0, y);
		var p1 = new Point(Chart.Width, y);

		context.DrawLine(p0, p1, Color, Thickness, LineStyle);
	}
}
