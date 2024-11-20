namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Double Exponential Moving Average [DEMA]
/// </summary>
public partial class DoubleExponentialMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid);

	private ExponentialMovingAverage _ema1, _ema2;

	public DoubleExponentialMovingAverage()
	{
		Name = "Double Exponential Moving Average";
		ShortName = "DEMA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_ema1 = new ExponentialMovingAverage(Source, Period);
		_ema2 = new ExponentialMovingAverage(_ema1.Result, Period);
	}

	protected override void Calculate(int index)
	{
		Result[index] = 2 * _ema1[index] - _ema2[index];
	}
}
