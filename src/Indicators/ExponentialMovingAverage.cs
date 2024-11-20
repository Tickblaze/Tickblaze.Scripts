namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Exponential Moving Average [EMA]
/// </summary>
public partial class ExponentialMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#4caf50", LineStyle.Solid, 1);

	private double _smoothFactor;

	public ExponentialMovingAverage()
	{
		Name = "Exponential Moving Average";
		ShortName = "EMA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_smoothFactor = 2.0 / (Period + 1);
	}

	protected override void Calculate(int index)
	{
		Result[index] = index == 0 ? Source[index] : Source[index] * _smoothFactor + Result[index - 1] * (1 - _smoothFactor);
	}
}
