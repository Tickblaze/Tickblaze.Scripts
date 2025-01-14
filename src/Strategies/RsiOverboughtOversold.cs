using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class RsiOverboughtOversold : OverboughtOversoldStrategyBase
{
	[NumericRange(1, int.MaxValue)]
	[Parameter("Period")]
	public int Period { get; set; } = 14;

	protected override ISeries<double> Series => _rsi.Result;

	private RelativeStrengthIndex _rsi;

	public RsiOverboughtOversold()
	{
		Name = "RSI Overbought/Oversold";
		Description = "Relative Strength Index [RSI] - Overbought/Oversold Strategy";
		OverboughtLevel = 70;
		OversoldLevel = 30;
	}

	protected override void Initialize()
	{
		_rsi = new RelativeStrengthIndex(Bars.Close, Period, MovingAverageType.Simple, 1);
		_rsi.Average.IsVisible = false;
		_rsi.OverboughtLevel.Value = OverboughtLevel;
		_rsi.OversoldLevel.Value = OversoldLevel;
		_rsi.ShowOnChart = true;
	}
}
