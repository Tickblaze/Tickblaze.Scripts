namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Range Indicator [RI]
/// </summary>
public partial class RangeIndicator : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Histogram);

	public RangeIndicator()
	{
		Name = "Range Indicator";
		ShortName = "RI";
		IsOverlay = false;
	}

	protected override void Calculate(int index)
	{
		Result[index] = Bars[index].High - Bars[index].Low;
	}
}
