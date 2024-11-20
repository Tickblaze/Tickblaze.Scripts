namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Standard Deviation [StdDev]
/// </summary>
public partial class StandardDeviation : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new("#2962ff", LineStyle.Solid, 1);

	private MovingAverage _movingAverage;

	public StandardDeviation()
	{
		Name = "Standard Deviation";
		ShortName = "StdDev";
	}

	protected override void Initialize()
	{
		_movingAverage = new MovingAverage(Source, Period, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		var period = Math.Min(Period, Source.Count);
		var sum = 0.0;
		var sma = _movingAverage[index];

		for (var i = 0; i < period; i++)
		{
			sum += Math.Pow(Source[index - i] - sma, 2.0);
		}

		Result[index] = Math.Sqrt(sum / period);
	}
}
