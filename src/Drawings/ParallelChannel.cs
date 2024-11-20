namespace Tickblaze.Scripts.Drawings;

public sealed class ParallelChannel : TrendLine
{
	public override int PointsCount => 3;

	private double _channelWidth;

	public ParallelChannel()
	{
		Name = "Parallel Channel";
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count == PointsCount)
		{
			if (index == 2)
			{
				_channelWidth = (double)Points[2].Value - (double)Points[1].Value;
			}
			else
			{
				Points[2].Value = (double)Points[1].Value + _channelWidth;
			}

			Points[2].X = Points[1].X;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		IPoint pointA, pointB, pointC, pointD;

		pointA = PointA;
		pointB = PointB;
		pointC = null;
		pointD = null;

		var hasThreePoints = Points.Count >= 3;
		if (hasThreePoints)
		{
			pointC = new Point(pointA.X, pointA.Y + (Points[2].Y - pointB.Y));
			pointD = Points[2];
		}

		if (ExtendLeft && ExtendRight)
		{
			context.DrawExtendedLine(pointA, pointB, Color, Thickness, LineStyle);

			if (hasThreePoints)
			{
				context.DrawExtendedLine(pointC, pointD, Color, Thickness, LineStyle);
			}
		}
		else if (ExtendRight)
		{
			context.DrawRay(pointA, pointB, Color, Thickness, LineStyle);

			if (hasThreePoints)
			{
				context.DrawRay(pointC, pointD, Color, Thickness, LineStyle);
			}
		}
		else if (ExtendLeft)
		{
			context.DrawRay(pointB, pointA, Color, Thickness, LineStyle);

			if (hasThreePoints)
			{
				context.DrawRay(pointD, pointC, Color, Thickness, LineStyle);
			}
		}
		else
		{
			context.DrawLine(pointA, pointB, Color, Thickness, LineStyle);

			if (hasThreePoints)
			{
				context.DrawLine(pointC, pointD, Color, Thickness, LineStyle);
			}
		}
	}
}
