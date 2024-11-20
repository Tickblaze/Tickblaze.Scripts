namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Average True Range % [ATRP]
/// </summary>
public partial class AverageTrueRangePercent : Indicator
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 5;

	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	private TrueRange _trueRange;
	private MovingAverage _movingAverage;

	public AverageTrueRangePercent()
	{
		Name = "Average True Range %";
		ShortName = "ATRP";
	}

	protected override void Initialize()
	{
		_trueRange = new TrueRange();
		_movingAverage = new MovingAverage(_trueRange.Result, Period, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		var close = Bars[index].Close;

		Result[index] = index > 0
			? close != 0 ? 100.0 * _movingAverage[index] / close : Result[index - 1]
			: close;
	}
}
