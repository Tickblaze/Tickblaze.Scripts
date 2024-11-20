using System.ComponentModel;

namespace Tickblaze.Scripts.Indicators;

[Browsable(false)]
public class BarIndex : Indicator
{
	[Plot("Bar Index")]
	public PlotSeries Result { get; set; } = new(Color.Gray, PlotStyle.Histogram);

	public BarIndex()
	{
		Name = "Bar Index";
		ScalePrecision = 0;
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];

		Result[index] = index;
		Result.Colors[index] = bar.Close > bar.Open ? Color.Green : bar.Close < bar.Open ? Color.Red : Color.White;
	}
}
