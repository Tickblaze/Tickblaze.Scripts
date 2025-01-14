using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class WprOverboughtOversold : OverboughtOversoldStrategyBase
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 14;

	protected override ISeries<double> Series => _wpr.Result;

	private WilliamsPercentR _wpr;

	public WprOverboughtOversold()
	{
		Name = "Williams %R Overbought/Oversold";
		Description = "Williams %R - Overbought/Oversold Strategy";
		OverboughtLevel = -20;
		OversoldLevel = -80;
	}

	protected override void Initialize()
	{
		_wpr = new WilliamsPercentR(Period) { ShowOnChart = true };
		_wpr.OverboughtLevel.Value = OverboughtLevel;
		_wpr.OversoldLevel.Value = OversoldLevel;
	}
}