using Tickblaze.Scripts.Indicators;

namespace Tickblaze.Scripts.Strategies;

public class VwapCrossover : CrossoverStrategyBase
{
	[Parameter("Price")]
	public PriceType Price { get; set; } = PriceType.Close;

	public enum PriceType
	{
		Open,
		High,
		Low,
		Close
	}

	protected override ISeries<double> FastSeries => _priceSeries;
	protected override ISeries<double> SlowSeries => _vwap.Result;

	private ISeries<double> _priceSeries;
	private VolumeWeightedAveragePrice _vwap;

	public VwapCrossover()
	{
		Name = "VWAP Crossover";
		Description = "Volume Weighted Average Price [VWAP] - Crossover Strategy";
	}

	protected override void Initialize()
	{
		_priceSeries = Price switch
		{
			PriceType.Open => Bars.Open,
			PriceType.High => Bars.High,
			PriceType.Low => Bars.Low,
			_ => Bars.Close
		};
		
		_vwap = new VolumeWeightedAveragePrice(false, 1, 0, false, 2, 0, false, 3, 0)
		{
			ShowOnChart = true
		};
	}
}
