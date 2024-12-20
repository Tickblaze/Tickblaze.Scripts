using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class DssOverboughtOversold : OverboughtOversoldStrategyBase
{
	[Parameter("Period 1"), NumericRange(1, int.MaxValue)]
	public int Period1 { get; set; } = 10;

	[Parameter("Period 2"), NumericRange(1, int.MaxValue)]
	public int Period2 { get; set; } = 3;

	[Parameter("Period 3"), NumericRange(1, int.MaxValue)]
	public int Period3 { get; set; } = 3;

	protected override ISeries<double> Series => _dss.Result;

	private DoubleSmoothStochastics _dss;

	public DssOverboughtOversold()
	{
		Name = "DSS Overbought/Oversold";
		Description = "Double Smoothed Stochastic [DDS] - Overbought/Oversold Strategy";
		OverboughtLevel = 90;
		OversoldLevel = 10;
	}

	protected override void Initialize()
	{
		_dss = new DoubleSmoothStochastics(Period1, Period2, Period3) { ShowOnChart = true };
		_dss.OverboughtLevel.Value = OverboughtLevel;
		_dss.OversoldLevel.Value = OversoldLevel;
	}
}
