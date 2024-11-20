namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Average True Range [ATR]
/// </summary>
public partial class AverageTrueRange : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	private TrueRange _trueRange;
	private MovingAverage _movingAverage;

	public AverageTrueRange()
	{
		Name = "Average True Range";
		ShortName = "ATR";
	}

	protected override void Initialize()
	{
		_trueRange = new TrueRange();
		_movingAverage = new MovingAverage(_trueRange.Result, Period, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		Result[index] = _movingAverage[index];
	}
}
