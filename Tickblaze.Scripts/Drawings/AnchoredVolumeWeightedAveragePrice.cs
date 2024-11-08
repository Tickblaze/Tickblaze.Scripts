namespace Tickblaze.Scripts.Drawings;

public class AnchoredVolumeWeightedAveragePrice : Drawing
{
	[Parameter("VWAP Line Color")]
	public Color Color { get; set; } = Color.Cyan;

	[Parameter("VWAP Line Thickness"), NumericRange(1, 5)]
	public int Thickness { get; set; } = 1;

	[Parameter("VWAP Line Style")]
	public LineStyle LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("1. Band Visible?")]
	public bool BandEnabled1 { get; set; } = false;

	[Parameter("1. Band deviations"), NumericRange(0, double.MaxValue)]
	public double BandMultiplier1 { get; set; } = 0.75;

	[Parameter("1. Band Color")]
	public Color BandColor1 { get; set; } = Color.Green;

	[Parameter("1. Band Line Thickness"), NumericRange(1, 5)]
	public int BandThickness1 { get; set; } = 1;

	[Parameter("1. Band Line Style")]
	public LineStyle BandLineStyle1 { get; set; } = LineStyle.Solid;

	[Parameter("2. Band Visible?")]
	public bool BandEnabled2 { get; set; } = false;

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double BandMultiplier2 { get; set; } = 1.75;

	[Parameter("Band 2 Color")]
	public Color BandColor2 { get; set; } = Color.Yellow;

	[Parameter("Band 2 Line Thickness"), NumericRange(1, 5)]
	public int BandThickness2 { get; set; } = 1;

	[Parameter("Band 2 Line Style")]
	public LineStyle BandLineStyle2 { get; set; } = LineStyle.Solid;

	[Parameter("3. Band Visible?")]
	public bool BandEnabled3 { get; set; } = false;

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double BandMultiplier3 { get; set; } = 2.75;

	[Parameter("Band 3 Color")]
	public Color BandColor3 { get; set; } = Color.Red;

	[Parameter("Band 3 Line Thickness"), NumericRange(1, 5)]
	public int BandThickness3 { get; set; } = 1;

	[Parameter("Band 3 Line Style")]
	public LineStyle BandLineStyle3 { get; set; } = LineStyle.Solid;

	public IChartPoint Point => Points[0];
	public override int PointsCount => 1;

	private record LineSettings(double Multiplier, Color Color, int Thickness, LineStyle LineStyle);

	private int? _fromIndex;
	private Series<double> _cumulativeVolume, _cumulativeTypicalVolume, _cumulativeVariance;
	private Series<double> _vwap, _deviation;
	private List<LineSettings> _lineSettings;

	public AnchoredVolumeWeightedAveragePrice()
	{
		Name = "Anchored VWAP";
	}

	protected override void Initialize()
	{
		_fromIndex = null;
		_cumulativeVolume = new();
		_cumulativeTypicalVolume = new();
		_cumulativeVariance = new();
		_vwap = new();
		_deviation = new();

		_lineSettings = [new(0, Color, Thickness, LineStyle)];

		if (BandEnabled1)
		{
			_lineSettings.Add(new(BandMultiplier1, BandColor1, BandThickness1, BandLineStyle1));
			_lineSettings.Add(new(-BandMultiplier1, BandColor1, BandThickness1, BandLineStyle1));
		}

		if (BandEnabled2)
		{
			_lineSettings.Add(new(BandMultiplier2, BandColor2, BandThickness2, BandLineStyle2));
			_lineSettings.Add(new(-BandMultiplier2, BandColor2, BandThickness2, BandLineStyle2));
		}

		if (BandEnabled3)
		{
			_lineSettings.Add(new(BandMultiplier3, BandColor3, BandThickness3, BandLineStyle3));
			_lineSettings.Add(new(-BandMultiplier3, BandColor3, BandThickness3, BandLineStyle3));
		}
	}

	public override void SetPoint(IComparable xDataValue, IComparable yDataValue, int index)
	{
		var barIndex = Chart.GetBarIndexByXCoordinate(Point.X);
		var bar = Bars[barIndex];

		Point.Value = (bar.High + bar.Low + bar.Close) / 3;
	}

	public override void OnRender(IDrawingContext context)
	{
		var fromIndex = Chart.GetBarIndexByXCoordinate((int)Point.X);
		var toIndex = Bars.Count - 1;

		if (_fromIndex is null || _fromIndex != fromIndex)
		{
			_fromIndex = fromIndex;

			for (var index = fromIndex; index < toIndex; index++)
			{
				Calculate(index);
			}
		}

		Calculate(toIndex);

		var firstVisibleBarIndex = Chart.FirstVisibleBarIndex;
		if (firstVisibleBarIndex > toIndex)
		{
			return;
		}

		var lastVisibleBarIndex = Chart.LastVisibleBarIndex;
		if (lastVisibleBarIndex < fromIndex)
		{
			return;
		}

		fromIndex = Math.Max(fromIndex, Chart.FirstVisibleBarIndex - 1);
		toIndex = Math.Min(toIndex, Chart.LastVisibleBarIndex + 1);

		var pointsCount = toIndex - fromIndex + 1;
		var linePoints = new Dictionary<LineSettings, Point[]>();

		foreach (var key in _lineSettings)
		{
			linePoints[key] = new Point[pointsCount];
		}

		for (var index = fromIndex; index <= toIndex; index++)
		{
			var i = index - fromIndex;
			var x = Chart.GetXCoordinateByBarIndex(index);
			var vwap = _vwap[index];
			var deviation = _deviation[index];

			foreach (var (line, points) in linePoints)
			{
				points[i] = new(x, ChartScale.GetYCoordinateByValue(vwap + deviation * line.Multiplier));
			}
		}

		foreach (var (line, points) in linePoints)
		{
			context.DrawPolygon(points, null, line.Color, line.Thickness, line.LineStyle);
		}

		if ((DateTime)Point.Time >= Bars[^1].Time)
		{
			context.DrawEllipse(Point, 5, 5, "#80808080");
		}
	}

	private void Calculate(int index)
	{
		var bar = Bars[index];
		var volume = bar.Volume;
		var typicalPrice = (bar.High + bar.Low + bar.Close) / 3;

		if (index == _fromIndex)
		{
			_cumulativeVolume[index] = volume;
			_cumulativeTypicalVolume[index] = volume * typicalPrice;
			_vwap[index] = typicalPrice;
			_cumulativeVariance[index] = 0;
			_deviation[index] = 0;

			Point.Value = typicalPrice;
		}
		else
		{
			_cumulativeVolume[index] = _cumulativeVolume[index - 1] + volume;
			_cumulativeTypicalVolume[index] = _cumulativeTypicalVolume[index - 1] + volume * typicalPrice;
			_vwap[index] = _cumulativeTypicalVolume[index] / _cumulativeVolume[index];
			_cumulativeVariance[index] = _cumulativeVariance[index - 1] + Math.Pow(typicalPrice - _vwap[index], 2);
			_deviation[index] = Math.Sqrt(_cumulativeVariance[index] / (index + 1 - _fromIndex.Value));
		}
	}
}
