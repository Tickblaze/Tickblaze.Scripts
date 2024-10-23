namespace Tickblaze.Scripts.Drawings;

public sealed class AnchoredVWAP : Drawing
{
	[Parameter("Extend to current bar")]
	public bool ExtendToCurrentBar { get; set; } = false;

	[Parameter("VWAP Line Color")]
	public Color VWAPLineColor { get; set; } = Color.Cyan;

	[Parameter("VWAP Line Thickness"), NumericRange(1, 5)]
	public int VWAPLineThickness { get; set; } = 1;

	[Parameter("VWAP Line Style")]
	public LineStyle VWAPLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Mult { get; set; } = 0.75;

	[Parameter("Band 1 Color")]
	public Color Band1Color { get; set; } = Color.Green;

	[Parameter("Band 1 Line Thickness"), NumericRange(0, 5)]
	public int Band1LineThickness { get; set; } = 1;

	[Parameter("Band 1 Line Style")]
	public LineStyle Band1LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Mult { get; set; } = 1.75;

	[Parameter("Band 2 Color")]
	public Color Band2Color { get; set; } = Color.Yellow;

	[Parameter("Band 2 Line Thickness"), NumericRange(0, 5)]
	public int Band2LineThickness { get; set; } = 1;

	[Parameter("Band 2 Line Style")]
	public LineStyle Band2LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Mult { get; set; } = 2.75;

	[Parameter("Band 3 Color")]
	public Color Band3Color { get; set; } = Color.Red;

	[Parameter("Band 3 Line Thickness"), NumericRange(0, 5)]
	public int Band3LineThickness { get; set; } = 1;

	[Parameter("Band 3 Line Style")]
	public LineStyle Band3LineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Anchor line Color")]
	public Color AnchorLineColor { get; set; } = Color.DimGray;

	[Parameter("Anchor Line Thickness"), NumericRange(0, 5)]
	public int AnchorLineThickness { get; set; } = 1;

	[Parameter("Anchor Line Style")]
	public LineStyle AnchorLineStyle { get; set; } = LineStyle.Solid;

	public override int PointsCount => 2;

	private class VWAPlinedata(double mult, Color lineColor, int lineThickness, LineStyle lineStyle)
	{
		public double Mult = mult;
		public double PriorUpperY = 0;
		public double PriorLowerY = 0;
		public Color LineColor = lineColor;
		public int LineThickness = lineThickness;
		public LineStyle Style = lineStyle;
	}

	private enum VWAP_ids { VWAP, Band1, Band2, Band3 };
	private Dictionary<VWAP_ids, VWAPlinedata> _vwaps = [];
	private Dictionary<int, double> _typ = [];
	private Random _randp;
	private double _typPrice = double.MinValue;

	public AnchoredVWAP()
	{
		Name = "Anchored VWAP";
	}

	private Dictionary<int, double> _v = [];
	private Random _randv;

	private double GetVolume(int i)
	{
		if (Bars != null)
		{
			return Bars[i].Volume;
		}

		if (!_v.ContainsKey(i))
		{
			if (_v.Count == 0)
			{
				_randv = new Random(i);
			}

			_v[i] = _randv.Next(100, 2000);
		}

		return _v[i];
	}

	private double GetTypicalPrice(int i)
	{
		if (Bars != null)
		{
			return Bars.TypicalPrice[i];
		}

		if (!_typ.ContainsKey(i))
		{
			if (_typ.Count == 0)
			{
				_randp = new Random(i);
			}

			_typ[i] = _typPrice;
			_typPrice -= 0.25 * _randp.Next(-1, 7);
		}

		return _typ[i];
	}

	public override void OnRender(IDrawingContext context)
	{
		if (_typPrice == double.MinValue)
		{
			_typPrice = (double)Points[0].Value;
		}

		if (AnchorLineThickness > 0)
		{
			context.DrawLine(Points[0], Points[1], AnchorLineColor, AnchorLineThickness, AnchorLineStyle);
		}

		var leftIndex = Chart.GetBarIndexByXCoordinate(Math.Min(Points[0].X, Points[1].X));
		var rightIndex = ExtendToCurrentBar ? (Bars == null ? 100 : Bars.Count - 1) : Chart.GetBarIndexByXCoordinate(Math.Max(Points[0].X, Points[1].X));

		var volumeSum = 0.0;
		var typicalVolumeSum = 0.0;
		var varianceSum = 0.0;
		var pointL = new Point(0, 0);
		var pointR = new Point(0, 0);

		_vwaps[VWAP_ids.VWAP] = new VWAPlinedata(0, VWAPLineColor, VWAPLineThickness, VWAPLineStyle);

		if (Band1Mult > 0 && Band1LineThickness > 0)
		{
			_vwaps[VWAP_ids.Band1] = new VWAPlinedata(Band1Mult, Band1Color, Band1LineThickness, Band1LineStyle);
		}
		else
		{
			_vwaps.Remove(VWAP_ids.Band1);
		}

		if (Band2Mult > 0 && Band2LineThickness > 0)
		{
			_vwaps[VWAP_ids.Band2] = new VWAPlinedata(Band2Mult, Band2Color, Band2LineThickness, Band2LineStyle);
		}
		else
		{
			_vwaps.Remove(VWAP_ids.Band2);
		}

		if (Band3Mult > 0 && Band3LineThickness > 0)
		{
			_vwaps[VWAP_ids.Band3] = new VWAPlinedata(Band3Mult, Band3Color, Band3LineThickness, Band3LineStyle);
		}
		else
		{
			_vwaps.Remove(VWAP_ids.Band3);
		}

		for (var i = leftIndex; i <= rightIndex && leftIndex < rightIndex - 10; i++)
		{
			var vol = GetVolume(i);
			var typPrice = GetTypicalPrice(i);
			typicalVolumeSum += vol * typPrice;

			if (i == leftIndex)
			{
				volumeSum = vol;
				pointR = new Point(Chart.GetXCoordinateByBarIndex(i), ChartScale.GetYCoordinateByValue(typPrice));

				foreach (var kvp in _vwaps)
				{
					//left-edge of all plots start out at the same Y pixel
					kvp.Value.PriorUpperY = kvp.Value.PriorLowerY = pointR.Y;
				}
			}
			else
			{
				volumeSum += vol;
				var curVWAP = typicalVolumeSum / volumeSum;
				var diff = typPrice - curVWAP;
				varianceSum += diff * diff;
				var deviation = Math.Sqrt(Math.Max(varianceSum / (i - leftIndex), 0));

				//left-edge X pixel is set to the last print X pixel
				pointL.X = pointR.X;
				pointR.X = Chart.GetXCoordinateByBarIndex(i);

				foreach (var kvp in _vwaps)
				{
					pointL.Y = kvp.Value.PriorUpperY;
					pointR.Y = ChartScale.GetYCoordinateByValue(curVWAP + deviation * kvp.Value.Mult);
					kvp.Value.PriorUpperY = pointR.Y;
					context.DrawLine(pointL, pointR, kvp.Value.LineColor, kvp.Value.LineThickness, kvp.Value.Style);

					//Draw the lower line plot only if this is band 1, 2 or 3
					if (kvp.Key != VWAP_ids.VWAP)
					{
						pointL.Y = kvp.Value.PriorLowerY;
						pointR.Y = ChartScale.GetYCoordinateByValue(curVWAP - deviation * kvp.Value.Mult);
						kvp.Value.PriorLowerY = pointR.Y;
						context.DrawLine(pointL, pointR, kvp.Value.LineColor, kvp.Value.LineThickness, kvp.Value.Style);
					}
				}
			}
		}
	}
}