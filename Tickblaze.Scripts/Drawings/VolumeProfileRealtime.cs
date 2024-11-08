using System.ComponentModel;

namespace Tickblaze.Scripts.Drawings;

[Browsable(false)]
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
		var lastBarIndex = -1;
		var lastBar = (Bar)null;

		for (var barIndex = Bars.Count - 1; barIndex >= 0; barIndex--)
		{
			var bar = Bars[barIndex];
			if (bar is not null)
			{
				lastBarIndex = barIndex;
				lastBar = bar;

				break;
			}
		}

		if (lastBar is null)
		{
			return;
		}

		var lastBarX = Chart.GetXCoordinateByBarIndex(lastBarIndex);
		if (lastBarX <= Points[0].X)
		{
			return;
		}

		var endPoint = new ChartPoint()
		{
			Time = lastBar.Time,
			X = lastBarX,
		};

		OnRender(context, startPoint, endPoint);
	}
}