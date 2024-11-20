namespace Tickblaze.Scripts.Indicators;

public partial class ZeroLagExponentialMovingAverage : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#4caf50", LineStyle.Solid, 1);

	private double _smoothFactor;
	private int _lagPeriod;

	public ZeroLagExponentialMovingAverage()
	{
		Name = "Zero-Lag Exponential Moving Average";
		ShortName = "ZLEMA";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_smoothFactor = 2.0 / (Period + 1);
		_lagPeriod = (int)Math.Ceiling((Period - 1) / 2.0);
	}

	protected override void Calculate(int index)
	{
		Result[index] = index >= _lagPeriod
			? (2 * Source[index] - Source[index - _lagPeriod]) * _smoothFactor + Result[index - 1] * (1 - _smoothFactor)
			: Source[index];
	}
}
