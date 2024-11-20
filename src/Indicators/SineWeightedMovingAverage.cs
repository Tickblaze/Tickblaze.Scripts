namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Sine Weighted Moving Average [SWMA]
/// </summary>
public partial class SineWeightedMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid, 1);

	public SineWeightedMovingAverage()
	{
		Name = "Sine Weighted Moving Average";
		ShortName = "SWMA";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var period = Math.Min(Period, index + 1);
		var sum = 0.0;
		var sumWeights = 0.0;

		for (var i = 0; i < period; i++)
		{
			var weight = Math.Sin(Math.PI * (i + 1) / (period + 1));

			sum += Source[index - i] * weight;
			sumWeights += weight;
		}

		Result[index] = sum / sumWeights;
	}
}
