using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class RsiCrossover : CrossoverStrategyBase
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	[Parameter("Signal Type")]
	public MovingAverageType SignalType { get; set; } = MovingAverageType.Simple;

	[Parameter("Signal Period"), NumericRange(1, int.MaxValue)]
	public int SignalPeriod { get; set; } = 14;

	protected override ISeries<double> FastSeries => _rsi.Result;
	protected override ISeries<double> SlowSeries => _rsi.Average;

	private RelativeStrengthIndex _rsi;

	public RsiCrossover()
	{
		Name = "RSI Crossover";
		Description = "Relative Strength Index [RSI] - Crossover Strategy";
	}

	protected override void Initialize()
	{
		_rsi = new RelativeStrengthIndex(Bars.Close, Period, SignalType, SignalPeriod)
		{
			ShowOnChart = true
		};
	}
}