namespace Tickblaze.Scripts.Indicators;

public sealed partial class BarVolume : Indicator
{
	[Parameter("Up Color")]
	public Color UpColor { get; set; } = Color.Green;

	[Parameter("Down Color")]
	public Color DownColor { get; set; } = Color.Red;

	[Plot("Volume")]
	public PlotSeries Volume { get; set; } = new(Color.Gray, PlotStyle.Histogram);

	public BarVolume()
	{
		Name = "Bar Volume";
		ShortName = "Vol";
	}

	protected override void Calculate(int index)
	{
		var bar = Bars[index];

		Volume[index] = bar.Volume;
		Volume.Colors[index] = bar.Close > bar.Open ? UpColor : DownColor;
	}
}
