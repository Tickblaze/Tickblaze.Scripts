using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class StochOverboughtOversold : OverboughtOversoldStrategyBase
{
	[Parameter("%K Periods"), NumericRange(1, int.MaxValue)]
	public int KPeriods { get; set; } = 9;

	[Parameter("%K Slowing"), NumericRange(1, int.MaxValue)]
	public int KSlowing { get; set; } = 3;

	[Parameter("%D Periods"), NumericRange(1, int.MaxValue)]
	public int DPeriods { get; set; } = 9;

	[Parameter("MA Type")]
	public MovingAverageType SmoothingType { get; set; } = MovingAverageType.Simple;

	[Parameter("Output")]
	public StochOutputType Output { get; set; } = StochOutputType.PercentK;

	protected override ISeries<double> Series => Output is StochOutputType.PercentK ? _stoch.PercentK : _stoch.PercentD;

	public enum StochOutputType
	{
		[DisplayName("%K")]
		PercentK,

		[DisplayName("%D")]
		PercentD,
	}

	private StochasticOscillator _stoch;

	public StochOverboughtOversold()
	{
		Name = "Stochastic Overbought/Oversold";
		Description = "Stochastic Oscillator [Stoch] - Overbought/Oversold Strategy";
		OverboughtLevel = 80;
		OversoldLevel = 20;
	}

	protected override void Initialize()
	{
		_stoch = new StochasticOscillator(KPeriods, KSlowing, DPeriods, SmoothingType) { ShowOnChart = true };
		_stoch.OverboughtLevel.Value = OverboughtLevel;
		_stoch.OversoldLevel.Value = OversoldLevel;
	}
}
