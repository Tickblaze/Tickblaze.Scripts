
using System.ComponentModel;

namespace Tickblaze.Scripts.Drawings;

[Browsable(false)]
public sealed class ManualVolumeProfile : VolumeProfileBase
{
	public ManualVolumeProfile()
	{
		Name = "Volume Profile - Manual";
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		if (Points.Count < 2)
		{
			return;
		}

		var fromIndex = Chart.GetBarIndexByXCoordinate(Points[0].X);
		var toIndex = Chart.GetBarIndexByXCoordinate(Points[1].X);

		if (fromIndex == -1)
		{
			fromIndex = Points[1].X < Points[0].X ? Bars.Count - 1 : 0;
		}

		if (toIndex == -1)
		{
			toIndex = Points[1].X > Points[0].X ? Bars.Count - 1 : 0;
		}

		if (fromIndex > toIndex)
		{
			(fromIndex, toIndex) = (toIndex, fromIndex);
		}

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
			Points[0].Value = Points[1].Value = (maximum + minimum) / 2;
		}
	}

	public override void OnRender(IDrawingContext context)
	{
		if (Points.Count < 2)
		{
			return;
		}

		OnRender(context, Points[0], Points[1]);
	}
}