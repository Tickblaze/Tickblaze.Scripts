namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Triple Exponential Moving Average [TEMA]
/// </summary>
public partial class TripleExponentialMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, LineStyle.Solid, 1);

	private ExponentialMovingAverage _ema1, _ema2, _ema3;

	public TripleExponentialMovingAverage()
	{
		Name = "Triple Exponential Moving Average";
		ShortName = "TEMA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_ema1 = new ExponentialMovingAverage(Source, Period);
		_ema2 = new ExponentialMovingAverage(_ema1.Result, Period);
		_ema3 = new ExponentialMovingAverage(_ema2.Result, Period);
	}

	protected override void Calculate(int index)
	{
		Result[index] = 3 * (_ema1[index] - _ema2[index]) + _ema3[index];
	}
}
