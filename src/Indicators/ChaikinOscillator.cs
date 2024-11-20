namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Chaikin Oscillator [CO]
/// </summary>
public partial class ChaikinOscillator : Indicator
{
	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Exponential;

	[Parameter("Fast Period"), NumericRange(1, int.MaxValue)]
	public int FastPeriod { get; set; } = 3;

	[Parameter("Slow Period"), NumericRange(1, int.MaxValue)]
	public int SlowPeriod { get; set; } = 10;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Red, PlotStyle.Line);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, Color.Gray, LineStyle.Dash);

	private AccumulationDistributionLine _accumulationDistribution;
	private MovingAverage _movingAverageFast, _movingAverageSlow;

	public ChaikinOscillator()
	{
		Name = "Chaikin Oscillator";
		ShortName = "CO";
	}

	protected override void Initialize()
	{
		_accumulationDistribution = new AccumulationDistributionLine();
		_movingAverageFast = new MovingAverage(_accumulationDistribution.Result, FastPeriod, SmoothingType);
		_movingAverageSlow = new MovingAverage(_accumulationDistribution.Result, SlowPeriod, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		Result[index] = _movingAverageFast[index] - _movingAverageSlow[index];
	}
}
