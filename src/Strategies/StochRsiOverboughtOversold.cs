using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class StochRsiOverboughtOversold : OverboughtOversoldStrategyBase
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	protected override ISeries<double> Series => _stochRsi.Result;

	private StochasticRelativeStrengthIndex _stochRsi;

	public StochRsiOverboughtOversold()
	{
		Name = "Stoch RSI Overbought/Oversold";
		Description = "Stochastic Relative Strength Index [Stoch RSI] - Overbought/Oversold Strategy";
		OverboughtLevel = 0.8;
		OversoldLevel = 0.2;
	}

	protected override void Initialize()
	{
		_stochRsi = new StochasticRelativeStrengthIndex(Bars.Close, Period) { ShowOnChart = true };
		_stochRsi.OverboughtLevel.Value = OverboughtLevel;
		_stochRsi.OversoldLevel.Value = OversoldLevel;
	}
}
