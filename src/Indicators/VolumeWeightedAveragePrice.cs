using Tickblaze.Scripts.Misc;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Volume Weighted Average Price [VWAP]
/// </summary>
public partial class VolumeWeightedAveragePrice : Indicator
{
	[Parameter("Show Band 1")]
	public bool ShowBand1 { get; set; } = false;
	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Multiplier { get; set; } = 0.75;
	[NumericRange(0, 100)]
	[Parameter("Band 1 Fill Shading Opacity %", Description = "Opacity of the shading between VWAP and band 1")]
	public int Band1FillShadingOpacity { get; set; } = 20;

	[Parameter("Show Band 2")]
	public bool ShowBand2 { get; set; } = false;
	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Multiplier { get; set; } = 1.75;
	[NumericRange(0, 100)]
	[Parameter("Band 2 Fill Shading Opacity %", Description = "Opacity of the shading between band 1 and 2")]
	public int Band2FillShadingOpacity { get; set; } = 10;

	[Parameter("Show Band 3")]
	public bool ShowBand3 { get; set; } = false;
	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Multiplier { get; set; } = 2.75;
	[NumericRange(0, 100)]
	[Parameter("Band 3 Fill Shading Opacity %", Description = "Opacity of the shading between band 2 and 3")]
	public int Band3FillShadingOpacity { get; set; } = 5;

	[Plot("Vwap")]
	public PlotSeries Result { get; set; } = new(Color.Cyan);

	[Plot("Band1 Upper")]
	public PlotSeries Band1Upper { get; set; } = new(Color.Green);

	[Plot("Band1 Lower")]
	public PlotSeries Band1Lower { get; set; } = new(Color.Green);

	[Plot("Band2 Upper")]
	public PlotSeries Band2Upper { get; set; } = new(Color.Yellow);

	[Plot("Band2 Lower")]
	public PlotSeries Band2Lower { get; set; } = new(Color.Yellow);

	[Plot("Band3 Upper")]
	public PlotSeries Band3Upper { get; set; } = new(Color.Red);

	[Plot("Band3 Lower")]
	public PlotSeries Band3Lower { get; set; } = new(Color.Red);

	private VwapCalculator? _vwapTracker;

	public VolumeWeightedAveragePrice()
	{
		Name = "Volume Weighted Average Price";
		ShortName = "VWAP";
		IsOverlay = true;
		AutoRescale = false;
	}

	private PlotSeries GetBand(int band, int side)
	{
		return band switch
		{
			0 => Result,
			1 => side == 1 ? Band1Upper : Band1Lower,
			2 => side == 1 ? Band2Upper : Band2Lower,
			3 => side == 1 ? Band3Upper : Band3Lower
		};
	}
	
	protected override void Initialize()
	{
		_vwapTracker ??= new VwapCalculator(Bars, Symbol, true);

		for (var i = 1; i <= 3; i++)
		{
			for (var side = -1; side <= 1; side += 2)
			{
				var showOuterBand = i switch
				{
					1 => ShowBand1,
					2 => ShowBand2,
					3 => ShowBand3
				};

				if (!showOuterBand)
				{
					continue;
				}

				var outerBand = GetBand(i, side);
				var opacity = i switch
				{
					1 => Band1FillShadingOpacity,
					2 => Band2FillShadingOpacity,
					3 => Band3FillShadingOpacity
				};

				ShadeBetween(GetBand(i - 1, side), outerBand, outerBand.Color, outerBand.Color, opacity / 100f);
			}
		}
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];
		if (bar == null)
		{
			return;
		}

		_vwapTracker!.Update(index);
		Result[index] = _vwapTracker.VWAP;

		for (var i = 0; i < 3; i++)
		{
			var showBand = i switch
			{
				0 => ShowBand1,
				1 => ShowBand2,
				2 => ShowBand3
			};

			if (!showBand)
			{
				continue;
			}

			var upperBand = i switch
			{
				0 => Band1Upper,
				1 => Band2Upper,
				2 => Band3Upper
			};
			
			var lowerBand = i switch
			{
				0 => Band1Lower,
				1 => Band2Lower,
				2 => Band3Lower
			};

			var multiplier = i switch
			{
				0 => Band1Multiplier,
				1 => Band2Multiplier,
				2 => Band3Multiplier
			};

			upperBand[index] = _vwapTracker.VWAP + _vwapTracker.Deviation * multiplier;
			lowerBand[index] = _vwapTracker.VWAP - _vwapTracker.Deviation * multiplier;
		}
	}
}