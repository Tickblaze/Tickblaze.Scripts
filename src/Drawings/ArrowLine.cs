
namespace Tickblaze.Scripts.Drawings;

public class ArrowLine : TrendLine
{
	public ArrowLine()
	{
		Name = "Arrow Line";
	}

	public override void OnRender(IDrawingContext context)
	{
		base.OnRender(context);

		var angle = Math.Atan2(PointB.Y - PointA.Y, PointB.X - PointA.X);
		var arrowSize = 14;

		var arrowPoint1 = new Point(PointB.X - arrowSize * Math.Cos(angle - Math.PI / 6), PointB.Y - arrowSize * Math.Sin(angle - Math.PI / 6));
		var arrowPoint2 = new Point(PointB.X - arrowSize * Math.Cos(angle + Math.PI / 6), PointB.Y - arrowSize * Math.Sin(angle + Math.PI / 6));

		context.DrawPolygon([arrowPoint1, PointB, arrowPoint2], Color, Color, Thickness, LineStyle);
	}
}
