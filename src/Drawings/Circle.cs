namespace Tickblaze.Scripts.Drawings;

public sealed class Circle : Ellipse
{
	public Circle()
	{
		Name = "Circle";
	}

	public override void OnRender(IDrawingContext context)
	{
		var distanceX = Math.Abs(Points[0].X - Points[1].X);
		var distanceY = Math.Abs(Points[0].Y - Points[1].Y);
		var radius = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

		context.DrawEllipse(Points[0], radius, radius, BackgroundColor, BorderColor, BorderThickness, BorderLineStyle);
	}
}
