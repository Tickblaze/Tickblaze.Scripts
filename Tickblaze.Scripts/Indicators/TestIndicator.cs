using System.ComponentModel;
using System.Diagnostics;

namespace Tickblaze.Scripts.Indicators;

//[Browsable(false)]
public partial class TestIndicator : Indicator
{
	[Plot("Plot #1")]
	public PlotSeries Plot1 { get; set; } = new(Color.Red);

	[Plot("Plot #2")]
	public PlotSeries Plot2 { get; set; } = new(Color.Green);

	[Plot("Plot #3")]
	public PlotSeries Plot3 { get; set; } = new(Color.Blue);

	[Parameter("Color #1")]
	public Color Color1 { get; set; } = Color.Red;

	[Parameter("Color #2")]
	public Color Color2 { get; set; } = Color.Green;

	//[Plot("Bar Index")]
	//public PlotSeries Result { get; set; } = new(Color.Transparent, PlotStyle.Histogram);

	//private readonly Color[] _colors = [Color.Red, Color.Green, Color.Blue, Color.White];
	//private int _index, _colorIndex;
	//private int _lastIndex = -1;

	protected override void Initialize()
	{
		ShadeBetween(Plot1, Plot2, Color1, Color2, 0.5f);
		ShadeBetween(Plot3, 0, Color.Yellow, Color.Blue, 0.8f);
	}

	protected override void Calculate(int index)
	{
		Plot1[index] = GetSinusoidValue(index);
		Plot2[index] = GetSinusoidValue(index, 2);
		Plot3[index] = GetSinusoidValue(index, 0.25, 0.04);

		//var bar = Bars[index];

		//Result[index] = bar.Close;
		//Result.Colors[index] = _colors[_colorIndex];

		//if (bar.Close > bar.Open)
		//{
		//	Result.Colors[index] = Color.Green;
		//}
		//else if (bar.Close < bar.Open)
		//{
		//	Result.Colors[index] = Color.Red;
		//}
		//else
		//{
		//	Result.Colors[index] = Color.White;
		//}
	}

	private static double GetSinusoidValue(int index, double amplitude = 1, double frequency = 0.01, double phaseShift = 0, double verticalOffset = 0)
	{
		return amplitude * Math.Sin(2 * Math.PI * frequency * index + phaseShift) + verticalOffset;
	}
}
