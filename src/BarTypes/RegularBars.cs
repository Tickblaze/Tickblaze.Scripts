using System.ComponentModel;

namespace Tickblaze.Scripts.BarTypes;

[Browsable(false)]
public sealed class RegularBars : BarType
{
	public RegularBars()
	{
		Name = "Regular Bars";
	}

	public override void OnDataPoint(Bar bar)
    {
        var (time, open, high, low, close, volume) = bar;

        var lastBar = Bars.Count > 0 ? Bars[^1] : null;
		if (lastBar is null || lastBar.Time < time)
		{
			AddBar(bar);
		}
		else
		{
			UpdateBar(bar);
		}
	}
}
