namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Relative Strength Index [RSI]
/// </summary>
public partial class RelativeStrengthIndex : Indicator
{
	[Parameter("Source")]
	public ISeries<double> Source { get; set; }

	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Signal Type")]
	public MovingAverageType SignalType { get; set; } = MovingAverageType.Simple;

	[Parameter("Signal Period"), NumericRange(1, int.MaxValue)]
	public int SignalPeriod { get; set; } = 14;

	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue);

	[Plot("Average")]
	public PlotSeries Average { get; set; } = new(Color.Yellow);

	[Plot("Overbought level")]
	public PlotLevel OverboughtLevel { get; set; } = new(70, Color.Red, LineStyle.Dash, 1);

	[Plot("Middle level")]
	public PlotLevel MiddleLevel { get; set; } = new(50, Color.Gray, LineStyle.Dash, 1);

	[Plot("Oversold level")]
	public PlotLevel OversoldLevel { get; set; } = new(30, Color.Green, LineStyle.Dash, 1);

	private DataSeries _gains, _losses;
	private ExponentialMovingAverage _averageGains, _averageLosses;
	private MovingAverage _signal;

	public RelativeStrengthIndex()
	{
		Name = "Relative Strength Index";
		ShortName = "RSI";
		IsOverlay = false;
	}

	protected override void Initialize()
	{
		var period = 2 * Period - 1;

		_gains = new DataSeries();
		_losses = new DataSeries();
		_averageGains = new ExponentialMovingAverage(_gains, period);
		_averageLosses = new ExponentialMovingAverage(_losses, period);
		_signal = new MovingAverage(Result, SignalPeriod, SignalType);
	}

	protected override void Calculate(int index)
	{
		if (index == 0)
		{
			_gains[index] = 0;
			_losses[index] = 0;

			Result[index] = 50;
		}
		else
		{
			var source0 = Source[index];
			var source1 = Source[index - 1];

			_gains[index] = Math.Max(source0 - source1, 0);
			_losses[index] = Math.Max(source1 - source0, 0);

			var avgGain = _averageGains[index];
			var avgLoss = _averageLosses[index];

			Result[index] = avgGain * avgLoss == 0 ? 50 : 100 - 100 / (1 + avgGain / avgLoss);
			Average[index] = _signal[index];
		}
	}
}