namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Simple Moving Average [SMA]
/// </summary>
public partial class SimpleMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid);

	public SimpleMovingAverage()
	{
		Name = "Simple Moving Average";
		ShortName = "SMA";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		var period = Math.Min(Period, index + 1);
		var sum = 0.0;

		for (var i = 0; i < period; i++)
		{
			sum += Source[index - i];
		}

		Result[index] = sum / period;
	}
}
