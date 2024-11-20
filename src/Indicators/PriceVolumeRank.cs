namespace Tickblaze.Scripts.Indicators;

/// <summary>
/// Price Volume Rank [PVR]
/// </summary>
public partial class PriceVolumeRank : Indicator
{
	[Plot("Result")]
	public PlotSeries Result { get; set; } = new(Color.Blue, PlotStyle.Histogram);

	public PriceVolumeRank()
	{
		Name = "Price Volume Rank";
		ShortName = "PVR";
		IsOverlay = false;
	}

	protected override void Calculate(int index)
	{
		if (index < 1)
		{
			Result[index] = 0;
			return;
		}

		var currentBar = Bars[index];
		var previousBar = Bars[index - 1];

		if (currentBar.Close > previousBar.Close && currentBar.Volume > previousBar.Volume)
		{
			Result[index] = 1;
		}
		else if (currentBar.Close > previousBar.Close && currentBar.Volume < previousBar.Volume)
		{
			Result[index] = 2;
		}
		else if (currentBar.Close < previousBar.Close && currentBar.Volume < previousBar.Volume)
		{
			Result[index] = 3;
		}
		else if (currentBar.Close < previousBar.Close && currentBar.Volume > previousBar.Volume)
		{
			Result[index] = 4;
		}
		else
		{
			Result[index] = 0;
		}
	}
}
