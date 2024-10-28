namespace Tickblaze.Scripts.Drawings;

public sealed class Pitchfork : Line
{
	//[Parameter("Style")]
	//public StyleType Style { get; set; }

	[Parameter("Anchor line color", Description = "Color and opacity of the anchor line")]
	public Color AnchorLineColor { get; set; } = Color.Gray;

	[Parameter("Anchor line thickness", Description = "Thickness of the anchor line")]
	public int AnchorLineThickness { get; set; } = 1;

	[Parameter("Anchor line style", Description = "Style of the anchor line")]
	public LineStyle AnchorLineStyle { get; set; } = LineStyle.Dash;

	public override int PointsCount => 3;

	//public enum StyleType
	//{
	//	Original,
	//	Schiff,
	//	ModifiedSchiff,
	//	Inside
	//}

	public Pitchfork()
	{
		Name = "Pitchfork";
	}

	public override void OnRender(IDrawingContext context)
	{
		var rayA = Points[0];
		var rayB = new Point(Points[1].X, Points[1].Y);

		if (Points.Count == PointsCount)
		{
			var pointA = Points[1];
			var pointB = Points[2];
			var pointC = new Point(pointA.X, pointA.Y);
			var pointD = new Point(pointB.X, pointB.Y);

			rayB.X = (pointA.X + pointB.X) / 2;
			rayB.Y = (pointA.Y + pointB.Y) / 2;

			if (rayA.X == rayB.X)
			{
				if (rayA.Y < rayB.Y)
				{
					pointC.Y++;
					pointD.Y++;
				}
				else
				{
					pointC.Y--;
					pointD.Y--;
				}
			}
			else
			{
				var slope = (rayB.Y - rayA.Y) / (rayB.X - rayA.X);
				var offsetX = rayB.X > rayA.X ? 100 : -100;

				pointC.X = pointA.X + offsetX;
				pointC.Y = slope * (pointA.X + offsetX) + (pointA.Y - slope * pointA.X);

				pointD.X = pointB.X + offsetX;
				pointD.Y = slope * (pointB.X + offsetX) + (pointB.Y - slope * pointB.X);
			}

			context.DrawLine(pointA, pointB, AnchorLineColor, AnchorLineThickness, AnchorLineStyle);
			context.DrawRay(pointA, pointC, Color, Thickness, LineStyle);
			context.DrawRay(pointB, pointD, Color, Thickness, LineStyle);
		}

		context.DrawRay(rayA, rayB, AnchorLineColor, AnchorLineThickness, AnchorLineStyle);
	}
}
