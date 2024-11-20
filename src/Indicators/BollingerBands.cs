namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Bollinger Bands [BB]
/// </summary>
public partial class BollingerBands : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Parameter("Multiplier"), NumericRange(0, int.MaxValue, 0.01)]
	public double Multiplier { get; set; } = 2;

	[Parameter("Smoothing Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Plot("Main")]
	public PlotSeries Main { get; set; } = new(Color.Gray, LineStyle.Solid);

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue, LineStyle.Solid);

	private MovingAverage _movingAverage;
	private StandardDeviation _standardDeviation;

	public BollingerBands()
	{
		Name = "Bollinger Bands";
		ShortName = "BB";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_movingAverage = new MovingAverage(Source, Period, SmoothingType);
		_standardDeviation = new StandardDeviation(Source, Period, SmoothingType);
	}

	protected override void Calculate(int index)
	{
		var movingAverage = _movingAverage[index];
		var bandDistance = _standardDeviation[index] * Multiplier;

		Main[index] = movingAverage;
		Upper[index] = movingAverage + bandDistance;
		Lower[index] = movingAverage - bandDistance;
	}
}
