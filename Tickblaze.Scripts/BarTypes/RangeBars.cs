using System.ComponentModel;

namespace Tickblaze.Scripts.BarTypes;

[Browsable(false)]
public sealed class RangeBars : BarType
{
	public RangeBars()
	{
		Source = SourceDataType.Tick;
	}

	public override void OnDataPoint(Bar bar)
	{
        var (time, open, high, low, close, volume) = bar;

        if (Bars.Count is 0)
		{
			AddBar(bar);
		}
		else
		{
			var cachedBar = Bars[^1];
			var maximum = cachedBar.Low + BarSize;
			var minimum = cachedBar.High - BarSize;

			if (close > maximum)
			{
				UpdateBar(new Bar(cachedBar.Time, cachedBar.Open, maximum, cachedBar.Low, maximum, cachedBar.Volume));
				AddBar(new Bar(time, close, close, close, close, volume));
			}
			else if (close < minimum)
			{
				UpdateBar(new Bar(cachedBar.Time, cachedBar.Open, cachedBar.High, minimum, minimum, cachedBar.Volume));
				AddBar(new Bar(time, close, close, close, close, volume));
			}
			else
			{
				UpdateBar(new Bar(cachedBar.Time, cachedBar.Open, Math.Max(cachedBar.High, close), Math.Min(cachedBar.Low, close), close, cachedBar.Volume + volume));
			}
		}
	}
}
