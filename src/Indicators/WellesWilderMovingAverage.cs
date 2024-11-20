namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Welles Wilder Moving Average [WWMA]
/// </summary>
public partial class WellesWilderMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#4caf50", LineStyle.Solid, 1);

	public WellesWilderMovingAverage()
	{
		Name = "Welles Wilder Moving Average";
		ShortName = "WWMA";
		IsOverlay = true;
	}

	protected override void Calculate(int index)
	{
		Result[index] = index < Period ? Bars[index].Close : Result[index - 1] + (Source[index] - Result[index - 1]) / Period;
	}
}
