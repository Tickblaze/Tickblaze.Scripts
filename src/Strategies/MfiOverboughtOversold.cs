using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class MfiOverboughtOversold : OverboughtOversoldStrategyBase
{
	[Parameter("Period"), NumericRange(2, int.MaxValue)]
	public int Period { get; set; } = 14;

	protected override ISeries<double> Series => _mfi.Result;

	private MoneyFlowIndex _mfi;

	public MfiOverboughtOversold()
	{
		Name = "MFI Overbought/Oversold";
		Description = "Money Flow Index [MFI] - Overbought/Oversold Strategy";
		OverboughtLevel = 80;
		OversoldLevel = 20;
	}

	protected override void Initialize()
	{
		_mfi = new MoneyFlowIndex(Period) { ShowOnChart = true };
		_mfi.OverboughtLevel.Value = OverboughtLevel;
		_mfi.OversoldLevel.Value = OversoldLevel;
	}
}
