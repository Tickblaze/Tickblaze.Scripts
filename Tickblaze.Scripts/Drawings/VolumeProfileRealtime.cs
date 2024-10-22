namespace Tickblaze.Scripts.Drawings;

public sealed class RealtimeVolumeProfile : Drawing
{
	public enum HistoEdge { Left, Right }
	public enum AnchorType { Line, Rectangle, Both, Hidden }

	[Parameter("Histo Thickness (ticks)"), NumericRange(1, int.MaxValue)]
	public int HistoThicknessTicks { get; set; } = 1;

	[Parameter("Value Area %"), NumericRange(0, 100)]
	public double ValueAreaPercent { get; set; } = 70;

	[Parameter("Value Area Histo Color")]
	public Color VAHistoColor { get; set; } = new Color(127, Color.Gray.R, Color.Gray.G, Color.Gray.B);

	[Parameter("Show Out-of-VA")]
	public bool ShowOutVA { get; set; } = true;

	[Parameter("Outside VA Histo Color")]
	public Color OutsideVAHistoColor { get; set; } = new Color(127, Color.DeepPurple.R, Color.DeepPurple.G, Color.DeepPurple.B);

	[Parameter("Histo Location")]
	public HistoEdge HistoLocation { get; set; } = HistoEdge.Left;

	[Parameter("Histo Width (%)"), NumericRange(0, 100)]
	public int HistoWidthPercent { get; set; } = 20;

	[Parameter("Show POC Level")]
	public bool ShowPOCLevel { get; set; } = false;

	[Parameter("POC Line Color")]
	public Color POCLineColor { get; set; } = new Color(250, Color.Yellow.R, Color.Yellow.G, Color.Yellow.B);

	[Parameter("POC Line Thickness"), NumericRange(1, 5)]
	public int POCLineThickness { get; set; } = 1;

	[Parameter("Show VAH Level")]
	public bool ShowVAHLevel { get; set; } = false;

	[Parameter("VAH Line Color")]
	public Color VAHLineColor { get; set; } = new Color(250, Color.Blue.R, Color.Blue.G, Color.Blue.B);

	[Parameter("VAH Line Thickness"), NumericRange(1, 5)]
	public int VAHLineThickness { get; set; } = 1;

	[Parameter("Show VAL Level")]
	public bool ShowVALLevel { get; set; } = false;

	[Parameter("VAL Line Color")]
	public Color VALLineColor { get; set; } = new Color(250, Color.Purple.R, Color.Purple.G, Color.Purple.B);

	[Parameter("VAL Line Thickness"), NumericRange(1, 5)]
	public int VALLineThickness { get; set; } = 1;

	[Parameter("Show Level Prices")]
	public bool ShowLevelPrices { get; set; } = false;

	[Parameter("Level Font")]
	public Font LevelFont { get; set; } = new Font("Arial", 12);

	[Parameter("Enable VWAP")]
	public bool EnableVWAP { get; set; } = true;

	[Parameter("VWAP Line Color")]
	public Color VWAPLineColor { get => _bandSettingsDict[VWAPIds.VWAP].Color; set => _bandSettingsDict[VWAPIds.VWAP].Color = value; }

	[Parameter("VWAP Line Thickness"), NumericRange(0, 5)]
	public int VWAPLineThickness { get => _bandSettingsDict[VWAPIds.VWAP].Thickness; set => _bandSettingsDict[VWAPIds.VWAP].Thickness = value; }

	[Parameter("VWAP Line Style")]
	public LineStyle VWAPLineStyle { get => _bandSettingsDict[VWAPIds.VWAP].LineStyle; set => _bandSettingsDict[VWAPIds.VWAP].LineStyle = value; }

	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Multiplier { get => _bandSettingsDict[VWAPIds.Band1].Multiplier; set => _bandSettingsDict[VWAPIds.Band1].Multiplier = value; }

	[Parameter("Band 1 Color")]
	public Color Band1Color { get => _bandSettingsDict[VWAPIds.Band1].Color; set => _bandSettingsDict[VWAPIds.Band1].Color = value; }

	[Parameter("Band 1 Line Thickness"), NumericRange(0, 5)]
	public int Band1LineThickness { get => _bandSettingsDict[VWAPIds.Band1].Thickness; set => _bandSettingsDict[VWAPIds.Band1].Thickness = value; }

	[Parameter("Band 1 Line Style")]
	public LineStyle Band1LineStyle { get => _bandSettingsDict[VWAPIds.Band1].LineStyle; set => _bandSettingsDict[VWAPIds.Band1].LineStyle = value; }

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Multiplier { get => _bandSettingsDict[VWAPIds.Band2].Multiplier; set => _bandSettingsDict[VWAPIds.Band2].Multiplier = value; }

	[Parameter("Band 2 Color")]
	public Color Band2Color { get => _bandSettingsDict[VWAPIds.Band2].Color; set => _bandSettingsDict[VWAPIds.Band2].Color = value; }

	[Parameter("Band 2 Line Thickness"), NumericRange(0, 5)]
	public int Band2LineThickness { get => _bandSettingsDict[VWAPIds.Band2].Thickness; set => _bandSettingsDict[VWAPIds.Band2].Thickness = value; }

	[Parameter("Band 2 Line Style")]
	public LineStyle Band2LineStyle { get => _bandSettingsDict[VWAPIds.Band2].LineStyle; set => _bandSettingsDict[VWAPIds.Band2].LineStyle = value; }

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Multiplier { get => _bandSettingsDict[VWAPIds.Band3].Multiplier; set => _bandSettingsDict[VWAPIds.Band3].Multiplier = value; }

	[Parameter("Band 3 Color")]
	public Color Band3Color { get => _bandSettingsDict[VWAPIds.Band3].Color; set => _bandSettingsDict[VWAPIds.Band3].Color = value; }

	[Parameter("Band 3 Line Thickness"), NumericRange(0, 5)]
	public int Band3LineThickness { get => _bandSettingsDict[VWAPIds.Band3].Thickness; set => _bandSettingsDict[VWAPIds.Band3].Thickness = value; }

	[Parameter("Band 3 Line Style")]
	public LineStyle Band3LineStyle { get => _bandSettingsDict[VWAPIds.Band3].LineStyle; set => _bandSettingsDict[VWAPIds.Band3].LineStyle = value; }

	[Parameter("Anchor Line Color")]
	public Color AnchorLineColor { get; set; } = Color.DimGray;

	[Parameter("Anchor Line Thickness"), NumericRange(1, 5)]
	public int AnchorLineThickness { get; set; } = 1;

	[Parameter("Anchor Line Style")]
	public LineStyle AnchorLineStyle { get; set; } = LineStyle.Solid;

	[Parameter("Anchor Type")]
	public AnchorType AnchorBoundsType { get; set; } = AnchorType.Rectangle;

	public override int PointsCount => 1;

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

	private class HistoData
	{
		public double MaxPrice;
		public double MidPrice;
		public double MinPrice;
		public SortedDictionary<int, double> VolBySession = [];
		public double VolTotal;
		public int SessionId;
		public bool IsInVA;
		public HistoData(double maxPrice, double minPrice, int initialSession)
		{
			MaxPrice = maxPrice;
			MinPrice = minPrice;
			MidPrice = (MaxPrice + MinPrice) / 2.0;
			SessionId = initialSession;
			VolTotal = 0;
		}
		public void AddVolume(int session, double volume)
		{
			VolTotal += volume;
			if (!VolBySession.TryGetValue(session, out _))
			{
				VolBySession[session] = volume;
			}
			else
			{
				var v0 = VolBySession[session];
				VolBySession[session] = volume + v0;
			}
		}
	}
	private SortedDictionary<int, HistoData> _histo = [];

	private double _profileHigh;
	private double _profileLow;
	private double _priorProfileHigh;
	private double _priorProfileLow;
	private double _priorLeftIndex = -1;
	private int _priorIndex = int.MinValue;
	private bool _isFirstTickOfBar;

	public RealtimeVolumeProfile()
	{
		Name = "Volume Profile - Realtime";
	}

	private readonly Dictionary<VWAPIds, double> _priorLowerY = [];
	private readonly Dictionary<VWAPIds, double> _priorUpperY = [];

	public override void OnRender(IDrawingContext context)
	{
		var leftIndex = 0;
		var rightIndex = Bars.Count - 1;
		var validBarIndexes = CalculateLeftAndRightIndexes(ref leftIndex, ref rightIndex, Points[0].X);

		if (validBarIndexes == null || validBarIndexes.Length == 0 || leftIndex >= rightIndex - 1 || Points[0].X > Chart.GetXCoordinateByBarIndex(Bars.Count - 1))
		{
			return;
		}

		//if user moves the left point, recalculate the histo
		if (_priorLeftIndex != leftIndex)
		{
			_priorIndex = leftIndex;
			_priorLeftIndex = leftIndex;
			_profileHigh = double.MinValue;
			_profileLow = double.MaxValue;
			_histo.Clear();
		}

		_isFirstTickOfBar = _priorIndex < rightIndex;
		if (_isFirstTickOfBar)
		{
			_profileHigh = double.MinValue;
			_profileLow = double.MaxValue;
			_histo.Clear();
		}

		//Calculate _histo initially, adding the most recently finished bar to the histo (not the unfinished, developing bar)
		for (var i = 0; i < validBarIndexes.Length; i++)
		{
			var barIndex = validBarIndexes[i];
			var bar = Bars[barIndex];
			var sessionId = ToInteger(bar.Time, 60.0 / 2);
			var high = Bars.Symbol.RoundToTick(bar.High);
			_profileHigh = Math.Max(high, _profileHigh);
			var low = Bars.Symbol.RoundToTick(bar.Low);
			_profileLow = Math.Min(low, _profileLow);
			var volPerHisto = bar.Volume / (Math.Max(Bars.Symbol.TickSize, high - low) / Bars.Symbol.TickSize);
			var tickPtr = low;

			while (tickPtr <= high)
			{
				var key = (int)Math.Round(tickPtr / Bars.Symbol.TickSize);
				if (!_histo.TryGetValue(key, out _))
				{
					_histo[key] = new HistoData(tickPtr + Bars.Symbol.TickSize / 2.0, tickPtr - Bars.Symbol.TickSize / 2.0, sessionId);
				}

				_histo[key].AddVolume(sessionId, volPerHisto);
				tickPtr += Bars.Symbol.TickSize;
			}
		}

		//update the Y value of the anchor point only if necessary - has performance gains
		if (_profileHigh != _priorProfileHigh || _profileLow != _priorProfileLow)
		{
			Points[0].Y = ChartScale.GetYCoordinateByValue((_profileHigh + _profileLow) / 2.0);
			_priorProfileHigh = _profileHigh;
			_priorProfileLow = _profileLow;
		}

		_priorIndex = rightIndex;

		var profileStartingX = Chart.GetXCoordinateByBarIndex(leftIndex);
		var profileEndingX = Chart.GetXCoordinateByBarIndex(rightIndex);
		var isPrintable1 = profileStartingX < Chart.Width;
		var isPrintable2 = profileEndingX > 0;

		var pointLeft = new Point(profileStartingX, Points[0].Y);
		var pointRight = new Point(profileEndingX, Points[0].Y);

		if (isPrintable1 && isPrintable2)
		{
			if (AnchorBoundsType is AnchorType.Line or AnchorType.Both)
			{
				context.DrawLine(pointLeft, pointRight, AnchorLineColor, AnchorLineThickness, AnchorLineStyle);
			}

			if (AnchorBoundsType is AnchorType.Rectangle or AnchorType.Both)
			{
				pointLeft.Y = ChartScale.GetYCoordinateByValue(_profileHigh);
				pointRight.Y = ChartScale.GetYCoordinateByValue(_profileLow);
				context.DrawRectangle(pointLeft, pointRight, Color.Empty, AnchorLineColor, AnchorLineThickness, AnchorLineStyle);
			}
		}

		if (EnableVWAP && isPrintable1 && isPrintable2 && (Band1Multiplier > 0 || Band2Multiplier > 0 || Band3Multiplier > 0))
		{
			var volumeSum = 0.0;
			var typicalVolumeSum = 0.0;
			var varianceSum = 0.0;
			var vwapPointLeft = new Point(0, 0);
			var vwapPointRight = new Point(0, 0);

			if (leftIndex >= rightIndex - 1)
			{
				return;
			}

			for (var i = 0; i < validBarIndexes.Length; i++)
			{
				var barIndex = validBarIndexes[i];
				var bar = Bars[barIndex];
				var typicalPrice = (bar.High + bar.Low + bar.Close) / 3;
				typicalVolumeSum += bar.Volume * typicalPrice;

				if (i == 0)
				{
					vwapPointRight = new Point(Chart.GetXCoordinateByBarIndex(barIndex), ChartScale.GetYCoordinateByValue(typicalPrice));
					volumeSum = bar.Volume;
					foreach (var id in Enum.GetValues<VWAPIds>())
					{
						_priorUpperY[id] = _priorLowerY[id] = vwapPointRight.Y;
					}

					continue;
				}

				volumeSum += bar.Volume;
				var curVWAP = typicalVolumeSum / volumeSum;
				var diff = typicalPrice - curVWAP;
				varianceSum += diff * diff;
				var deviation = Math.Sqrt(Math.Max(varianceSum / (barIndex - leftIndex), 0));

				//left-edge X pixel is set to the last print X pixel
				vwapPointLeft.X = vwapPointRight.X;
				vwapPointRight.X = Chart.GetXCoordinateByBarIndex(barIndex);

				foreach (var id in Enum.GetValues<VWAPIds>())
				{
					vwapPointLeft.Y = _priorUpperY[id];
					vwapPointRight.Y = ChartScale.GetYCoordinateByValue(curVWAP + deviation * _bandSettingsDict[id].Multiplier);
					_priorUpperY[id] = vwapPointRight.Y;

					//This draws the VWAP line, and the upper line for the bands
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

		if (isPrintable1 && isPrintable2 && _histo.Count > 2)
		{
			SortedDictionary<int, HistoData> renderHisto = null;
			if (HistoThicknessTicks == 1)
			{
				renderHisto = _histo;
			}
			else
			{
				renderHisto = [];
				var groupMinPrice = _histo[_histo.Keys.Min()].MinPrice;
				var groupMaxPrice = groupMinPrice + HistoThicknessTicks * Bars.Symbol.TickSize;
				var histoGroup = _histo.Where(k => k.Value.MaxPrice <= groupMaxPrice).Select(k => k.Key).ToList();
				var key = (int)Math.Round(groupMinPrice / Bars.Symbol.TickSize);

				while (key <= _histo.Keys.Max())
				{
					renderHisto[key] = new HistoData(groupMaxPrice, groupMinPrice, 0);
					var histoGroupVol = _histo.Where(kvp => histoGroup.Contains(kvp.Key)).Sum(kvp => kvp.Value.VolTotal);
					renderHisto[key].VolTotal = histoGroupVol;

					groupMinPrice = groupMaxPrice;
					groupMaxPrice = groupMinPrice + HistoThicknessTicks * Bars.Symbol.TickSize;
					histoGroup = _histo.Where(k => k.Value.MinPrice >= groupMinPrice && k.Value.MaxPrice <= groupMaxPrice).Select(k => k.Key).ToList();
					key = (int)Math.Round(groupMinPrice / Bars.Symbol.TickSize);
				}
			}

			var isLeftHisto = HistoLocation == HistoEdge.Left;
			var maxHistoSizePx = isLeftHisto ? (Chart.Width - Math.Max(0, profileStartingX)) * (HistoWidthPercent / 100.0) : Math.Min(Chart.Width - profileStartingX, Chart.Width * HistoWidthPercent / 100.0);
			var pointOfControlKey = GetPOCKey(renderHisto);
			var pocPrice = renderHisto[pointOfControlKey].MidPrice;
			renderHisto.Values.ToList().ForEach(k => k.IsInVA = false);
			var histosInVAH = 0;
			var histosInVAL = 0;
			DetermineValueArea(pointOfControlKey, renderHisto, ref histosInVAH, ref histosInVAL);
			var volumePerPixel = renderHisto[pointOfControlKey].VolTotal / maxHistoSizePx;

			if (isLeftHisto)
			{
				pointLeft.X = Math.Max(0, profileStartingX);
			}
			else
			{
				pointRight.X = profileEndingX;
				pointLeft.X = pointRight.X - maxHistoSizePx;
			}

			if (ShowPOCLevel)
			{
				PrintLevelLineAndText(context, isLeftHisto ? 'R' : 'L', pointLeft, pocPrice, pocPrice, POCLineColor, POCLineThickness, maxHistoSizePx);
			}

			if (ShowVAHLevel)
			{
				var histosInValAreaHigh = renderHisto.Count(k => !k.Value.IsInVA && k.Value.MidPrice >= pocPrice);
				var histoKey = histosInValAreaHigh == 0
					? renderHisto.Keys.Max()
					: renderHisto.Where(k => !k.Value.IsInVA && k.Value.MidPrice >= pocPrice).Select(k => k.Key).Min();
				PrintLevelLineAndText(context, isLeftHisto ? 'R' : 'L', pointLeft, renderHisto[histoKey].MidPrice, renderHisto[histoKey].MinPrice, VAHLineColor, VAHLineThickness, maxHistoSizePx);
			}

			if (ShowVALLevel)
			{
				var histosInValAreaLow = renderHisto.Count(k => !k.Value.IsInVA && k.Value.MidPrice <= pocPrice);
				var histoKey = histosInValAreaLow == 0
					? renderHisto.Keys.Min()
					: renderHisto.Where(k => !k.Value.IsInVA && k.Value.MidPrice <= pocPrice).Select(k => k.Key).Max();
				PrintLevelLineAndText(context, isLeftHisto ? 'R' : 'L', pointLeft, renderHisto[histoKey].MidPrice, renderHisto[histoKey].MaxPrice, VALLineColor, VALLineThickness, maxHistoSizePx);
			}

			foreach (var kvp in renderHisto)
			{
				if (ShowOutVA || kvp.Value.IsInVA)
				{
					if (isLeftHisto)
					{
						pointRight.X = pointLeft.X + kvp.Value.VolTotal / volumePerPixel;
					}
					else
					{
						pointLeft.X = Math.Max(0, pointRight.X - kvp.Value.VolTotal / volumePerPixel);
					}

					pointLeft.Y = ChartScale.GetYCoordinateByValue(kvp.Value.MinPrice);
					pointRight.Y = ChartScale.GetYCoordinateByValue(kvp.Value.MaxPrice);
					context.DrawRectangle(pointLeft, pointRight, kvp.Value.IsInVA ? VAHistoColor : OutsideVAHistoColor, null, 1);
				}
			}
		}
	}

	private int[] CalculateLeftAndRightIndexes(ref int leftIndex, ref int rightIndex, double x1)
	{
		leftIndex = Math.Max(0, Math.Min(Bars.Count - 1, Chart.GetBarIndexByXCoordinate(x1)));
		rightIndex = rightIndex <= 0 ? Chart.GetBarIndexByXCoordinate(x1) : rightIndex;

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
	private void PrintLevelLineAndText(IDrawingContext context, char Align, Point pointLeft, double displayPrice, double linePrice, Color color, int lineThickness, double maxHistoSizePx)
	{
		if (color == Color.Empty || color.A == 0)
		{
			return;
		}

		var pointRight = new Point(0, 0);
		displayPrice = Bars.Symbol.RoundToTick(displayPrice);
		pointLeft.Y = pointRight.Y = ChartScale.GetYCoordinateByValue(linePrice);
		pointRight.X = pointLeft.X + maxHistoSizePx;
		context.DrawLine(pointLeft, pointRight, color, lineThickness);

		if (ShowLevelPrices)
		{
			var priceText = ChartScale.FormatPrice(displayPrice);
			var size = context.MeasureText(priceText, LevelFont);
			var point = new Point(Align == 'R' ? pointRight.X - size.Width : pointLeft.X, pointRight.Y);
			context.DrawText(point, priceText, color, LevelFont);
		}
	}

	private static int GetPOCKey(SortedDictionary<int, HistoData> histos)
	{
		return histos.Aggregate((x, y) => x.Value.VolTotal > y.Value.VolTotal ? x : y).Key;
	}
	private void DetermineValueArea(int pocKey, SortedDictionary<int, HistoData> histos, ref int HistosInVAH, ref int HistosInVAL)
	{
		var totVol = histos.Values.Sum(v => v.VolTotal);
		var vol = histos[pocKey].VolTotal;
		var ptrA = histos.Where(k => k.Key < pocKey).Select(k => k.Key).OrderByDescending(k => k).ToList();
		var ptrB = histos.Where(k => k.Key > pocKey).Select(k => k.Key).OrderBy(k => k).ToList();

		histos[pocKey].IsInVA = true;
		var targetVolume = totVol * ValueAreaPercent / 100.0;

		while (vol < targetVolume && (ptrA.Count > 0 || ptrB.Count > 0))
		{
			if (ptrA.Count > 0)
			{
				vol += histos[ptrA[0]].VolTotal;
				histos[ptrA[0]].IsInVA = true;
				ptrA.RemoveAt(0);
			}

			if (vol < targetVolume && ptrB.Count > 0)
			{
				vol += histos[ptrB[0]].VolTotal;
				histos[ptrB[0]].IsInVA = true;
				ptrB.RemoveAt(0);
			}
		}

		HistosInVAH = ptrB.Count;
		HistosInVAL = ptrA.Count;
	}

	private static int ToInteger(DateTime t, double minutesPerSession)
	{
		return (int)(t.Hour * 100 + Math.Floor(t.Minute / minutesPerSession) * minutesPerSession);
	}

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
}