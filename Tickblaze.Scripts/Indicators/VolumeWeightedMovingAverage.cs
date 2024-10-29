namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Volume Weighted Moving Average [VWMA]
/// </summary>
public partial class VolumeWeightedMovingAverage : Indicator
{
	[Parameter("Price")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	public VolumeWeightedMovingAverage()
	{
		Name = "Volume Weighted Moving Average";
		ShortName = "VWMA";
		IsOverlay = true;
		AutoRescale = false;
	}

	protected override void Calculate(int index)
	{
		var period = Math.Min(Period, index + 1);
		var weightedSum = 0.0;
		var volumeSum = 0.0;

		for (var i = 0; i < period; i++)
		{
			var volume = Bars.Volume[index - i];

			weightedSum += Source[index - i] * volume;
			volumeSum += volume;
		}

		Result[index] = weightedSum / (volumeSum > double.Epsilon ? volumeSum : 1);

	}
}
