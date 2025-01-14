using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class MacdCrossover : CrossoverStrategyBase
{
	[Parameter("Fast Period"), NumericRange(1, int.MaxValue)]
	public int FastPeriod { get; set; } = 12;

	[Parameter("Slow Period"), NumericRange(1, int.MaxValue)]
	public int SlowPeriod { get; set; } = 26;

	[Parameter("Signal Period"), NumericRange(1, int.MaxValue)]
	public int SignalPeriod { get; set; } = 9;

	protected override ISeries<double> FastSeries => _macd.Result;
	protected override ISeries<double> SlowSeries => _macd.Signal;

	private MovingAverageConvergenceDivergence _macd;

	public MacdCrossover()
	{
		Name = "MACD Crossover";
		Description = "Moving Average Convergence/Divergence [MACD] - Crossover Strategy";
	}

	protected override void Initialize()
	{
		_macd = new(Bars.Close, FastPeriod, SlowPeriod, SignalPeriod, Color.Green, Color.Red)
		{
			ShowOnChart = true
		};
	}
}
