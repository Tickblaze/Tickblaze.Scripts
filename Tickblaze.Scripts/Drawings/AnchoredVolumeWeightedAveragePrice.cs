namespace Tickblaze.Scripts.Drawings;

public sealed class AnchoredVolumeWeightedAveragePrice : Drawing
{
	[Parameter("VWAP Line Color")]
	public Color VWAPLineColor
	{
		get => _bandSettingsDict[VWAPIds.VWAP].Color;
		set => _bandSettingsDict[VWAPIds.VWAP].Color = value;
	}

	[Parameter("VWAP Line Thickness"), NumericRange(1, 5)]
	public int VWAPLineThickness
	{
		get => _bandSettingsDict[VWAPIds.VWAP].Thickness;
		set => _bandSettingsDict[VWAPIds.VWAP].Thickness = value;
	}

	[Parameter("VWAP Line Style")]
	public LineStyle VWAPLineStyle
	{
		get => _bandSettingsDict[VWAPIds.VWAP].LineStyle;
		set => _bandSettingsDict[VWAPIds.VWAP].LineStyle = value;
	}

	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Multiplier
	{
		get => _bandSettingsDict[VWAPIds.Band1].Multiplier;
		set => _bandSettingsDict[VWAPIds.Band1].Multiplier = value;
	}

	[Parameter("Band 1 Color")]
	public Color Band1Color
	{
		get => _bandSettingsDict[VWAPIds.Band1].Color;
		set => _bandSettingsDict[VWAPIds.Band1].Color = value;
	}

	[Parameter("Band 1 Line Thickness"), NumericRange(0, 5)]
	public int Band1LineThickness
	{
		get => _bandSettingsDict[VWAPIds.Band1].Thickness;
		set => _bandSettingsDict[VWAPIds.Band1].Thickness = value;
	}

	[Parameter("Band 1 Line Style")]
	public LineStyle Band1LineStyle
	{
		get => _bandSettingsDict[VWAPIds.Band1].LineStyle;
		set => _bandSettingsDict[VWAPIds.Band1].LineStyle = value;
	}

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Multiplier
	{
		get => _bandSettingsDict[VWAPIds.Band2].Multiplier;
		set => _bandSettingsDict[VWAPIds.Band2].Multiplier = value;
	}

	[Parameter("Band 2 Color")]
	public Color Band2Color
	{
		get => _bandSettingsDict[VWAPIds.Band2].Color;
		set => _bandSettingsDict[VWAPIds.Band2].Color = value;
	}

	[Parameter("Band 2 Line Thickness"), NumericRange(0, 5)]
	public int Band2LineThickness
	{
		get => _bandSettingsDict[VWAPIds.Band2].Thickness;
		set => _bandSettingsDict[VWAPIds.Band2].Thickness = value;
	}

	[Parameter("Band 2 Line Style")]
	public LineStyle Band2LineStyle
	{
		get => _bandSettingsDict[VWAPIds.Band2].LineStyle;
		set => _bandSettingsDict[VWAPIds.Band2].LineStyle = value;
	}

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Multiplier
	{
		get => _bandSettingsDict[VWAPIds.Band3].Multiplier;
		set => _bandSettingsDict[VWAPIds.Band3].Multiplier = value;
	}

	[Parameter("Band 3 Color")]
	public Color Band3Color
	{
		get => _bandSettingsDict[VWAPIds.Band3].Color;
		set => _bandSettingsDict[VWAPIds.Band3].Color = value;
	}

	[Parameter("Band 3 Line Thickness"), NumericRange(0, 5)]
	public int Band3LineThickness
	{
		get => _bandSettingsDict[VWAPIds.Band3].Thickness;
		set => _bandSettingsDict[VWAPIds.Band3].Thickness = value;
	}

	[Parameter("Band 3 Line Style")]
	public LineStyle Band3LineStyle
	{
		get => _bandSettingsDict[VWAPIds.Band3].LineStyle;
		set => _bandSettingsDict[VWAPIds.Band3].LineStyle = value;
	}

	public override int PointsCount => 1;

	private enum VWAPIds
	{
		VWAP,
		Band1,
		Band2,
		Band3
	}

	private class BandSettings
	{
		public double Multiplier { get; set; }
		public Color Color { get; set; }
		public int Thickness { get; set; } = 0;
		public LineStyle LineStyle { get; set; } = LineStyle.Solid;
	}

	private readonly Dictionary<VWAPIds, BandSettings> _bandSettingsDict = new()
	{
		[VWAPIds.VWAP] = new BandSettings
		{
			Color = Color.Cyan,
			Thickness = 1
		},
		[VWAPIds.Band1] = new BandSettings
		{
			Multiplier = 0.75,
			Color = Color.Green
		},
		[VWAPIds.Band2] = new BandSettings
		{
			Multiplier = 1.75,
			Color = Color.Yellow
		},
		[VWAPIds.Band3] = new BandSettings
		{
			Multiplier = 2.75,
			Color = Color.Red
		}
	};
	private readonly Dictionary<VWAPIds, double> _priorLowerY = [];
	private readonly Dictionary<VWAPIds, double> _priorUpperY = [];

	public AnchoredVolumeWeightedAveragePrice()
	{
		Name = "Anchored VWAP";
	}

	public override void OnRender(IDrawingContext context)
	{
		var leftIndex = 0;
		var rightIndex = 0;
		var validBarIndexes = CalculateLeftAndRightIndexes(ref leftIndex, ref rightIndex, Points[0].X);

		if (validBarIndexes == null || validBarIndexes.Length == 0 || leftIndex >= rightIndex - 1 || Points[0].X > Chart.GetXCoordinateByBarIndex(Bars.Count - 1))
		{
			return;
		}

		var volumeSum = 0.0;
		var typicalVolumeSum = 0.0;
		var varianceSum = 0.0;
		var vwapPointLeft = new Point(0, 0);
		var vwapPointRight = new Point(0, 0);

		for (var i = 0; i < validBarIndexes.Length; i++)
		{
			var barIndex = validBarIndexes[i];
			var bar = Bars[barIndex];
			var volume = bar.Volume;
			var typicalPrice = (bar.High + bar.Low + bar.Close) / 3;
			typicalVolumeSum += volume * typicalPrice;

			if (i == 0)
			{
				volumeSum = bar.Volume;
				vwapPointRight = new Point(Chart.GetXCoordinateByBarIndex(barIndex), ChartScale.GetYCoordinateByValue(typicalPrice));
				//update the Y value of the anchor point only if necessary - has performance gains
				if (Points[0].Y != vwapPointRight.Y)
				{
					Points[0].Y = vwapPointRight.Y;
				}

				//left-edge of all plots start out at the same Y pixel
				foreach (var id in Enum.GetValues<VWAPIds>())
				{
					_priorUpperY[id] = _priorLowerY[id] = vwapPointRight.Y;
				}

				continue;
			}

			volumeSum += volume;
			var curVWAP = typicalVolumeSum / volumeSum;
			var diff = typicalPrice - curVWAP;
			varianceSum += diff * diff;
			var deviation = Math.Sqrt(Math.Max(varianceSum / (i + 1), 0));

			//left-edge X pixel is set to the last print X pixel
			vwapPointLeft.X = vwapPointRight.X;
			vwapPointRight.X = Chart.GetXCoordinateByBarIndex(barIndex);

			foreach (var id in Enum.GetValues<VWAPIds>())
			{
				vwapPointLeft.Y = _priorUpperY[id];
				vwapPointRight.Y = ChartScale.GetYCoordinateByValue(curVWAP + deviation * _bandSettingsDict[id].Multiplier);
				_priorUpperY[id] = vwapPointRight.Y;
				context.DrawLine(vwapPointLeft, vwapPointRight, _bandSettingsDict[id].Color, _bandSettingsDict[id].Thickness, _bandSettingsDict[id].LineStyle);

				//Draw the lower line plot only if this is band 1, 2 or 3
				if (id == VWAPIds.VWAP)
				{
					continue;
				}

				vwapPointLeft.Y = _priorLowerY[id];
				vwapPointRight.Y = ChartScale.GetYCoordinateByValue(curVWAP - deviation * _bandSettingsDict[id].Multiplier);
				_priorLowerY[id] = vwapPointRight.Y;
				context.DrawLine(vwapPointLeft, vwapPointRight, _bandSettingsDict[id].Color, _bandSettingsDict[id].Thickness, _bandSettingsDict[id].LineStyle);
			}
		}
	}

	private int[] CalculateLeftAndRightIndexes(ref int leftIndex, ref int rightIndex, double x1)
	{
		leftIndex = Math.Max(0, Math.Min(Bars.Count - 1, Chart.GetBarIndexByXCoordinate(x1)));
		rightIndex = Bars.Count - 1;

		//NOTE:  GetBarIndexByXCoordinate() returns -1 if the X coord exceeds the X of the rightmost bar
		if (rightIndex == -1)
		{
			rightIndex = Bars.Count - 1;
		}

		// Gap bars are null
		var validBarIndexes = Enumerable.Range(leftIndex, rightIndex - leftIndex)
			.Where(i => Bars[i] != null)
			.ToArray();

		var index = leftIndex;
		leftIndex = validBarIndexes.FirstOrDefault(k => k >= index);
		index = rightIndex;
		rightIndex = validBarIndexes.LastOrDefault(k => k <= index);
		return validBarIndexes;
	}
}