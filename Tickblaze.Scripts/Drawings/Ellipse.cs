﻿namespace Tickblaze.Scripts.Drawings;

public class Ellipse : Drawing
{
	[Parameter("Border thickness"), NumericRange(1)]
	public int BorderThickness { get; set; } = 1;

	[Parameter("Border style")]
	public LineStyle BorderLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Border color")]
	public Color BorderColor { get; set; } = "#ff9800";

	[Parameter("Background")]
	public Color BackgroundColor { get; set; } = "#33ff9800";

	public override int PointsCount => 2;

	public Ellipse()
	{
		Name = "Ellipse";
	}

	public override void OnRender(IDrawingContext context)
	{
		var center = Points[0];
		var radiusX = Math.Abs(Points[0].X - Points[1].X);
		var radiusY = Math.Abs(Points[0].Y - Points[1].Y);

		context.DrawEllipse(center, radiusX, radiusY, BackgroundColor, BorderColor, BorderThickness, BorderLineStyle);
	}
}