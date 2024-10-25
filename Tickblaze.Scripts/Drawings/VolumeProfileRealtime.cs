namespace Tickblaze.Scripts.Drawings;

public sealed class RealtimeVolumeProfile : VolumeProfileBase
{
	public override int PointsCount => 1;

	public RealtimeVolumeProfile()
	{
		Name = "Volume Profile - Realtime";
		HistoWidthPercent = 20;
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		var fromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		if (fromIndex == -1)
		{
			return;
		}

		var toIndex = Bars.Count - 1;
		var maximum = double.MinValue;
		var minimum = double.MaxValue;
		var hasRange = false;

		for (var barIndex = fromIndex; barIndex < toIndex; barIndex++)
		{
			var bar = Bars[barIndex];
			if (bar is null)
			{
				continue;
			}

			maximum = Math.Max(maximum, bar.High);
			minimum = Math.Min(minimum, bar.Low);
			hasRange = true;
		}

		if (hasRange)
		{
			Points[index].Value = (maximum + minimum) / 2;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		var startPoint = Points[0];

		var validBarIndexes = Enumerable.Range(0, Bars.Count - 1)
			.Where(i => Bars[i] != null)
			.ToArray();

		var endPoint = new ChartPoint()
		{
			Time = Bars[validBarIndexes.Last()].Time,
			X = Chart.GetXCoordinateByBarIndex(validBarIndexes.Last()),
		};

		OnRender(context, startPoint, endPoint);
	}
}