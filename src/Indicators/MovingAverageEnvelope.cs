namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Moving Average Envelope [MAE]
/// </summary>
public partial class MovingAverageEnvelope : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Percentage"), NumericRange(0.00001, int.MaxValue, 0.1)]
	public double Percentage { get; set; } = 1.5;

	[Parameter("Type")]
	public MovingAverageType Type { get; set; } = MovingAverageType.Simple;

	[Plot("Main")]
	public PlotSeries Main { get; set; } = new(Color.Gray, LineStyle.Solid);

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue, LineStyle.Solid);

	private double _percentage;
	private MovingAverage _movingAverage;

	public MovingAverageEnvelope()
	{
		Name = "Moving Average Envelope";
		ShortName = "MAE";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_percentage = Percentage / 100.0;
		_movingAverage = new MovingAverage(Source, Period, Type);
	}

	protected override void Calculate(int index)
	{
		var movingAverage = _movingAverage[index];
		var deviation = Source[index] * _percentage;

		Main[index] = movingAverage;
		Upper[index] = movingAverage + deviation;
		Lower[index] = movingAverage - deviation;
	}
}