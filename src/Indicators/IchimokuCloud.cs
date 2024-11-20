using System.ComponentModel;

namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Ichimoku Cloud
/// </summary>
[Browsable(false)]
public partial class IchimokuCloud : Indicator
{
	[Parameter("Tenkan Sen Period"), NumericRange(1)]
	public int TenkanSenPeriod { get; set; } = 9;

	[Parameter("Kijun Sen Period"), NumericRange(1)]
	public int KijunSenPeriod { get; set; } = 26;

	[Parameter("Senkou Span B Period"), NumericRange(1)]
	public int SenkouSpanBPeriod { get; set; } = 52;

	[Parameter("Lagging Span")]
	public int LaggingSpan { get; set; } = 26;

	[Plot("Tenkan Sen")]
	public PlotSeries TenkanSen { get; set; } = new("#2962ff");

	[Plot("Kijun Sen")]
	public PlotSeries KijunSen { get; set; } = new("#b71c1c");

	[Plot("Chikou Span")]
	public PlotSeries ChikouSpan { get; set; } = new("#43a047");

	[Plot("Senkou Span A")]
	public PlotSeries SenkouSpanA { get; set; } = new("#a5d6a7");

	[Plot("Senkou Span B")]
	public PlotSeries SenkouSpanB { get; set; } = new("#ef9a9a");

	private DonchianChannel[] _donchianChannel;

	public IchimokuCloud()
	{
		Name = "Ichimoku Cloud";
		ShortName = "Ichimoku";
		IsOverlay = true;
	}

	protected override void Initialize()
	{
		_donchianChannel =
		[
			new(TenkanSenPeriod),
			new(KijunSenPeriod),
			new(SenkouSpanBPeriod)
		];
	}

	protected override void Calculate(int index)
	{
		if (index < TenkanSenPeriod || index < SenkouSpanBPeriod)
		{
			return;
		}

		var tenkanSen = _donchianChannel[0].Middle[index];
		var kijunSen = _donchianChannel[1].Middle[index];
		var senkouSpanB = _donchianChannel[2].Middle[index];

		TenkanSen[index] = tenkanSen;
		KijunSen[index] = kijunSen;
		ChikouSpan[index - LaggingSpan] = Bars.Close[index];
		SenkouSpanA[index + LaggingSpan] = (tenkanSen + kijunSen) / 2;
		SenkouSpanB[index + LaggingSpan] = senkouSpanB;
	}
}