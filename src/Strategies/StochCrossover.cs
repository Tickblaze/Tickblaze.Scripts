using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class StochCrossover : CrossoverStrategyBase
{
	[Parameter("%K Periods"), NumericRange(1, int.MaxValue)]
	public int KPeriods { get; set; } = 9;

	[Parameter("%K Slowing"), NumericRange(1, int.MaxValue)]
	public int KSlowing { get; set; } = 3;

	[Parameter("%D Periods"), NumericRange(1, int.MaxValue)]
	public int DPeriods { get; set; } = 9;

	[Parameter("MA Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	protected override ISeries<double> FastSeries => _stoch.PercentK;
	protected override ISeries<double> SlowSeries => _stoch.PercentD;

	private StochasticOscillator _stoch;

	public StochCrossover()
	{
		Name = "Stochastic Crossover";
		Description = "Stochastic Oscillator - Crossover Strategy";
	}

	protected override void Initialize()
	{
		_stoch = new StochasticOscillator(KPeriods, KSlowing, DPeriods, SmoothingType)
		{
			ShowOnChart = true
		};
	}
}
