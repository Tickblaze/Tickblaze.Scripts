namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Stoller Average Range Channel [SAC]
/// </summary>
public partial class StollerAverageRangeChannel : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, 999, 1)]
	public int Period { get; set; } = 20;

	[Parameter("Multiplier"), NumericRange(0, int.MaxValue, 0.01)]
	public double Multiplier { get; set; } = 2;

	[Plot("Main")]
	public PlotSeries Main { get; set; } = new(Color.Gray, LineStyle.Dash);

	[Plot("Upper")]
	public PlotSeries Upper { get; set; } = new(Color.Blue, LineStyle.Solid);

	[Plot("Lower")]
	public PlotSeries Lower { get; set; } = new(Color.Blue, LineStyle.Solid);

	private SimpleMovingAverage _sma;
	private AverageTrueRange _atr;
	public StollerAverageRangeChannel()
	{
		Name = "Stoller Average Range Channel";
		ShortName = "SAC";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_sma = new SimpleMovingAverage(Source, Period);
		_atr = new AverageTrueRange(Period, MovingAverageType.Simple);
	}

	protected override void Calculate(int index)
	{
		var movingAverage = _sma[index];
		var channelSize = _atr[index] * Multiplier;

		Main[index] = movingAverage;
		Upper[index] = movingAverage + channelSize;
		Lower[index] = movingAverage - channelSize;
	}
}