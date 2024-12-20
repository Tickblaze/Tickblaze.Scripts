using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class CciOverboughtOversold : OverboughtOversoldStrategyBase
{
	[Parameter("Period"), NumericRange(1, int.MaxValue)]
	public int Period { get; set; } = 20;

	protected override ISeries<double> Series => _cci.Result;

	private CommodityChannelIndex _cci;

	public CciOverboughtOversold()
	{
		Name = "CCI Overbought/Oversold";
		Description = "Commodity Channel Index [CCI] - Overbought/Oversold Strategy";
		OverboughtLevel = 100;
		OversoldLevel = -100;
	}

	protected override void Initialize()
	{
		_cci = new CommodityChannelIndex(Period) { ShowOnChart = true };
		_cci.OverboughtLevel.Value = OverboughtLevel;
		_cci.OversoldLevel.Value = OversoldLevel;
	}
}
