namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Smoothed Moving Average [SMMA]
/// </summary>
public partial class SmoothedMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid, 1);

	public SmoothedMovingAverage()
	{
		Name = "Smoothed Moving Average";
		ShortName = "SMMA";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var period = Math.Min(Period, index + 1);

		if (index == period - 1)
		{
			var sum = 0.0;

			for (var i = 0; i < period; i++)
			{
				sum += Source[index - i];
			}

			Result[index] = sum / period;
		}
		else
		{
			Result[index] = (Result[index - 1] * (period - 1) + Source[index]) / period;
		}
	}
}