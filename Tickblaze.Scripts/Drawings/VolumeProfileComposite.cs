namespace Tickblaze.Scripts.Drawings;

public sealed class CompositeVolumeProfile : Drawing
{
	[Parameter("Profile Timeframe")]
	public VolumeProfileTimeframe Timeframe { get; set; } = VolumeProfileTimeframe.Daily; 
	
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

	[Parameter("VWAP Line Thickness"), NumericRange(1, 5)]
	public int VWAPLineThickness { get => _bandSettingsDict[VWAPIds.VWAP].Thickness; set => _bandSettingsDict[VWAPIds.VWAP].Thickness = value; }

	[Parameter("VWAP Line Style")]
	public LineStyle VWAPLineStyle { get => _bandSettingsDict[VWAPIds.VWAP].LineStyle; set => _bandSettingsDict[VWAPIds.VWAP].LineStyle = value; }

	[Parameter("Band 1 deviations"), NumericRange(0, double.MaxValue)]
	public double Band1Mult { get => _bandSettingsDict[VWAPIds.Band1].Multiplier; set => _bandSettingsDict[VWAPIds.Band1].Multiplier = value; }

	[Parameter("Band 1 Color")]
	public Color Band1Color { get => _bandSettingsDict[VWAPIds.Band1].Color; set => _bandSettingsDict[VWAPIds.Band1].Color = value; }

	[Parameter("Band 1 Line Thickness"), NumericRange(0, 5)]
	public int Band1LineThickness { get => _bandSettingsDict[VWAPIds.Band1].Thickness; set => _bandSettingsDict[VWAPIds.Band1].Thickness = value; }

	[Parameter("Band 1 Line Style")]
	public LineStyle Band1LineStyle { get => _bandSettingsDict[VWAPIds.Band1].LineStyle; set => _bandSettingsDict[VWAPIds.Band1].LineStyle = value; }

	[Parameter("Band 2 deviations"), NumericRange(0, double.MaxValue)]
	public double Band2Mult { get => _bandSettingsDict[VWAPIds.Band2].Multiplier; set => _bandSettingsDict[VWAPIds.Band2].Multiplier = value; }

	[Parameter("Band 2 Color")]
	public Color Band2Color { get => _bandSettingsDict[VWAPIds.Band2].Color; set => _bandSettingsDict[VWAPIds.Band2].Color = value; }

	[Parameter("Band 2 Line Thickness"), NumericRange(0, 5)]
	public int Band2LineThickness { get => _bandSettingsDict[VWAPIds.Band2].Thickness; set => _bandSettingsDict[VWAPIds.Band2].Thickness = value; }

	[Parameter("Band 2 Line Style")]
	public LineStyle Band2LineStyle { get => _bandSettingsDict[VWAPIds.Band2].LineStyle; set => _bandSettingsDict[VWAPIds.Band2].LineStyle = value; }

	[Parameter("Band 3 deviations"), NumericRange(0, double.MaxValue)]
	public double Band3Mult { get => _bandSettingsDict[VWAPIds.Band3].Multiplier; set => _bandSettingsDict[VWAPIds.Band3].Multiplier = value; }

	[Parameter("Band 3 Color")]
	public Color Band3Color { get => _bandSettingsDict[VWAPIds.Band3].Color; set => _bandSettingsDict[VWAPIds.Band3].Color = value; }

	[Parameter("Band 3 Line Thickness"), NumericRange(0, 5)]
	public int Band3LineThickness { get => _bandSettingsDict[VWAPIds.Band3].Thickness; set => _bandSettingsDict[VWAPIds.Band3].Thickness = value; }

	[Parameter("Band 3 Line Style")]
	public LineStyle Band3LineStyle { get => _bandSettingsDict[VWAPIds.Band3].LineStyle; set => _bandSettingsDict[VWAPIds.Band3].LineStyle = value; }

	public override int PointsCount => 1;

	private readonly Dictionary<VWAPIds, BandSettings> _bandSettingsDict = new()
	{
		[VWAPIds.VWAP] = new BandSettings
		{
			Color = Color.Cyan
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
	private SortedDictionary<int, SortedDictionary<int, HistoData>> _histos = [];

	private int _priorIndex = int.MinValue;
	private double _tickSize;
	private int _profileId;

	public CompositeVolumeProfile()
	{
		Name = "Volume Profile - Composite";
	}

	public override void OnRender(IDrawingContext context)
	{
		_tickSize = Bars == null || Bars.Symbol == null ? 0.25 : Bars.Symbol.TickSize;
		var leftIndex = 0;
		var rightIndex = Bars == null ? leftIndex + 5000 : Bars.Count - 1;

		// Gap bars are null
		var validBarIndexes = Enumerable.Range(leftIndex, rightIndex - leftIndex)
			.Where(i => Bars[i] != null)
			.ToArray();

		Bar priorBar = null;
		//Calculate _histo initially, adding the most recently finished bar to the histo (not the unfinished, developing bar)
		for (var i = 1; i < validBarIndexes.Length; i++)
		{
			var barIndex = validBarIndexes[i];
			var bar = Bars[barIndex];
			var typicalPrice = (bar.High + bar.Low + bar.Close) / 3.0;
			if (priorBar == null)
			{
				_profileId = barIndex;
			}
			else if (Timeframe == VolumeProfileTimeframe.Daily && bar.Time.Day != priorBar.Time.Day)
			{
				_profileId = barIndex;
			}
			else if (Timeframe == VolumeProfileTimeframe.Weekly && bar.Time.DayOfWeek < priorBar.Time.DayOfWeek)
			{
				_profileId = barIndex;
			}
			else if (Timeframe == VolumeProfileTimeframe.Monthly && bar.Time.Month != priorBar.Time.Month)
			{
				_profileId = barIndex;
			}

			priorBar = bar;

			//==============================================================================
			var pHigh = new Point(Chart.GetXCoordinateByBarIndex(barIndex), ChartScale.GetYCoordinateByValue(bar.High));
			var pLow = new Point(pHigh.X, ChartScale.GetYCoordinateByValue(bar.Low));
			context.DrawLine(pHigh, pLow, Color.White);
			//==============================================================================

			if (!_histos.TryGetValue(_profileId, out _))
			{
				_histos[_profileId] = [];
			}

			var sessionId = _profileId * 10000 + ToInteger(bar.Time, 60.0 / 2);
			var volPerHisto = bar.Volume / (Math.Max(_tickSize, RoundToTick(bar.High - bar.Low)) / _tickSize);
			var tickPtr = bar.Low;
			while (tickPtr <= bar.High)
			{
				var key = (int)Math.Round(tickPtr / _tickSize);
				if (!_histos[_profileId].TryGetValue(key, out _))
				{
					_histos[_profileId][key] = new HistoData(tickPtr + _tickSize / 2.0, tickPtr - _tickSize / 2.0, sessionId);
				}

				_histos[_profileId][key].AddVolume(sessionId, volPerHisto);
				tickPtr += _tickSize;
			}
		}

		_priorIndex = rightIndex;

		var firstVisibleBar = Chart.GetBarIndexByXCoordinate(0);
		var lastVisibleBar = Math.Max(Chart.LastVisibleBarIndex, Chart.GetBarIndexByXCoordinate(Chart.Width));
		var profileIDLeft = _histos.Keys.Where(n => n <= firstVisibleBar).OrderByDescending(n => n).FirstOrDefault();
		if (profileIDLeft == 0)
		{
			profileIDLeft = _histos.Keys.Min();
		}

		var profileIDRight = _histos.Keys.LastOrDefault(n => n <= lastVisibleBar);
		if (profileIDRight == 0)
		{
			profileIDRight = _histos.Keys.Max();
		}

		foreach (var profile in _histos.Where(k => k.Key >= profileIDLeft && k.Key <= profileIDRight))
		{
			var profileStartingX = Chart.GetXCoordinateByBarIndex(profile.Key);
			var profileMaxIndex = _histos.Keys.FirstOrDefault(k => k > profile.Key);
			profileMaxIndex = profileMaxIndex != 0 ? profileMaxIndex : (Bars == null ? 1000 : Bars.Count - 1);
			var profileEndingX = Chart.GetXCoordinateByBarIndex(profileMaxIndex);
			var isPrintable1 = profileStartingX < Chart.Width;
			var isPrintable2 = profileEndingX > 0;

			var pointLeft = new Point(profileStartingX, 0);
			var pointRight = new Point(profileEndingX, 0);

			if (EnableVWAP && isPrintable1 && isPrintable2 && (Band1Mult > 0 || Band2Mult > 0 || Band3Mult > 0))
			{
				var volumeSum = 0.0;
				var varianceSum = 0.0;
				var typicalVolumeSum = 0.0;
				// Gap bars are null
				validBarIndexes = Enumerable.Range(profile.Key, profileMaxIndex - profile.Key)
					.Where(i => Bars[i] != null)
					.ToArray();

				Dictionary<VWAPIds, double> priorLowerY = [];
				Dictionary<VWAPIds, double> priorUpperY = [];
				for (var i = 1; i < validBarIndexes.Length; i++)
				{
					var barIndex = validBarIndexes[i];
					var bar = Bars[barIndex]; 
					volumeSum += bar.Volume;
					var typicalPrice = (bar.High + bar.Low + bar.Close) / 3.0;
					typicalVolumeSum += bar.Volume * typicalPrice;

					if (i == 1)
					{
						pointRight = new Point(Chart.GetXCoordinateByBarIndex(barIndex), ChartScale.GetYCoordinateByValue(typicalPrice));
						volumeSum = bar.Volume;
						foreach (var id in Enum.GetValues<VWAPIds>())
						{
							priorUpperY[id] = pointRight.Y;
							priorLowerY[id] = pointRight.Y;
						}
					}
					else
					{
						var curVWAP = typicalVolumeSum / volumeSum;
						var diff = typicalPrice - curVWAP;
						varianceSum += diff * diff;
						var deviation = Math.Sqrt(Math.Max(varianceSum / (barIndex - profile.Key), 0));

						//left-edge X pixel is set to the last print X pixel
						pointLeft.X = pointRight.X;
						pointRight.X = Chart.GetXCoordinateByBarIndex(barIndex);
						foreach (var id in Enum.GetValues<VWAPIds>())
						{
							pointLeft.Y = priorUpperY[id];
							pointRight.Y = ChartScale.GetYCoordinateByValue(curVWAP + deviation * _bandSettingsDict[id].Multiplier);
							priorUpperY[id] = pointRight.Y;
							//This draws the VWAP line, and the upper line for the bands
							context.DrawLine(pointLeft, pointRight, _bandSettingsDict[id].Color, _bandSettingsDict[id].Thickness, _bandSettingsDict[id].LineStyle);

							//Draw the lower line plot only if this is band 1, 2 or 3
							if (id != VWAPIds.VWAP)
							{
								pointLeft.Y = priorLowerY[id];
								pointRight.Y = ChartScale.GetYCoordinateByValue(curVWAP - deviation * _bandSettingsDict[id].Multiplier);
								priorLowerY[id] = pointRight.Y;
								context.DrawLine(pointLeft, pointRight, _bandSettingsDict[id].Color, _bandSettingsDict[id].Thickness, _bandSettingsDict[id].LineStyle);
							}
						}
					}
				}
			}

			if (isPrintable1 && isPrintable2 && _histos.Count > 0 && _histos[_profileId].Count > 2)
			{
				SortedDictionary<int, HistoData> renderHisto = null;
				if (HistoThicknessTicks == 1)
				{
					renderHisto = profile.Value;
				}
				else
				{
					renderHisto = [];
					var groupMinPrice = profile.Value[profile.Value.Keys.Min()].MinPrice;
					var groupMaxPrice = groupMinPrice + HistoThicknessTicks * _tickSize;
					var histoGroup = profile.Value.Where(k => k.Value.MaxPrice <= groupMaxPrice).Select(k => k.Key).ToList();
					var key = (int)Math.Round(groupMinPrice / _tickSize);

					while (key <= profile.Value.Keys.Max())
					{
						renderHisto[key] = new HistoData(groupMaxPrice, groupMinPrice, 0);
						var histoGroupVol = profile.Value.Where(kvp => histoGroup.Contains(kvp.Key)).Sum(kvp => kvp.Value.VolTotal);
						renderHisto[key].VolTotal = histoGroupVol;

						groupMinPrice = groupMaxPrice;
						groupMaxPrice = groupMinPrice + HistoThicknessTicks * _tickSize;
						histoGroup = profile.Value.Where(k => k.Value.MinPrice >= groupMinPrice && k.Value.MaxPrice <= groupMaxPrice).Select(k => k.Key).ToList();
						key = (int)Math.Round(groupMinPrice / _tickSize);
					}
				}

				var isLeftHisto = true;
				var maxHistoSizePx = isLeftHisto ? (Chart.Width - Math.Max(0, profileStartingX)) * (HistoWidthPercent / 100.0) : Math.Min(Chart.Width - profileStartingX, Chart.Width * HistoWidthPercent / 100.0);
				if (profile.Key == profileIDLeft)
				{
					maxHistoSizePx = Math.Min(profileEndingX, maxHistoSizePx);
				}

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
	}

	private void PrintLevelLineAndText(IDrawingContext context, char Align, Point pointLeft, double displayPrice, double linePrice, Color color, int lineThickness, double maxHistoSizePx)
	{
		if (color == Color.Empty || color.A == 0)
		{
			return;
		}

		var pointRight = new Point(0, 0);
		displayPrice = RoundToTick(displayPrice);
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
	private double RoundToTick(double p)
	{
		if (Bars == null || Bars.Symbol == null)
		{
			var tk = (int)Math.Round(p / _tickSize);
			return tk * _tickSize;
		}
		else
		{
			return Bars.Symbol.RoundToTick(p);
		}
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
	public enum VolumeProfileTimeframe 
	{
		Daily, 
		Weekly, 
		Monthly
	}
	private class BandSettings
	{
		public double Multiplier { get; set; }
		public Color Color { get; set; }
		public int Thickness { get; set; } = 1;
		public LineStyle LineStyle { get; set; } = LineStyle.Solid;
	}
}