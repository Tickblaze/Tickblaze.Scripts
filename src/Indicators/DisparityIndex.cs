namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Disparity Index [DISI]
/// </summary>
public partial class DisparityIndex : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Line);

	[Plot("Zero")]
	public PlotLevel ZeroLevel { get; set; } = new(0, Color.Gray, LineStyle.Dash, 1);

	private MovingAverage _movingAverage;

	public DisparityIndex()
	{
		Name = "Disparity Index";
		ShortName = "DISI";
	}

	protected override void Initialize()
	{
		_movingAverage = new MovingAverage(Source, Period, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		var movingAverage = _movingAverage[index];

		Result[index] = movingAverage != 0 ? 100 * (Source[index] - movingAverage) / movingAverage : 0;
	}
}
