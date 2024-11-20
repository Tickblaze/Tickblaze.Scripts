namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Weighted Moving Average [WMA]
/// </summary>
public partial class WeightedMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#2962ff", LineStyle.Solid, 1);

	public WeightedMovingAverage()
	{
		Name = "Weighted Moving Average";
		ShortName = "WMA";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var back = Math.Min(Period - 1, index);
		var val = 0.0;
		var weight = 0;

		for (var idx = back; idx >= 0; idx--)
		{
			val += (idx + 1) * Source[index - (back - idx)];
			weight += idx + 1;
		}

		Result[index] = val / weight;
	}
}