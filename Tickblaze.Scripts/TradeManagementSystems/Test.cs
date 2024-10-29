using System.Diagnostics;
using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.TradeManagementSystems;

public class Test : TradeManagementSystem
{
	[NumericRange(0, int.MaxValue)]
	[Parameter("Stop-loss distance (ticks)", Description = "Enter the distance from your entry in ticks, that you would like to set your stop loss. Entering zero (0) means you have no stop loss.")]
	public int StopLossTicks { get; set; } = 16;

	[Parameter("Position size type")]
	public SizeType PositionSizeType { get; set; }

	[NumericRange(0, double.MaxValue)]
	[Parameter("Position size value")]
	public double PositionSize { get; set; } = 500;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #1 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #1. Entering zero (0) means you have no Target #1.")]
	public int FirstTakeProfitTicks { get; set; } = 8;

	[NumericRange(0, double.MaxValue)]
	[Parameter("Take-profit #1 size (%)", Description = "Enter how much % of your position you would like to close at Target #1.")]
	public double FirstTakeProfitSizePercent { get; set; } = 70;

	[NumericRange(0, int.MaxValue)]
	[Parameter("Take-profit #2 distance (ticks)", Description = "Enter the distance from your entry, that you would like to set Target #2. Entering zero (0) means you have no Target #2.")]
	public int SecondTakeProfitTicks { get; set; } = 24;

	[NumericRange(0, double.MaxValue)]
	[Parameter("Take-profit #2 size (%)", Description = "Enter how much % of your position you would like to close at Target #2.")]
	public double SecondTakeProfitSizePercent { get; set; } = 30;

	[Parameter("Breakeven stop-loss enabled?", Description = "To make the stop loss move automatically once a certain amount of profit has been gained, set this to True.")]
	public bool EnableAutoBreakeven { get; set; } = false;

	[Parameter("Breakeven trigger (ticks)", Description = "Enter the number of ticks of profit to trigger the stop loss to move.")]
	public int BreakevenTriggerTicks { get; set; } = 8;

	[Parameter("Breakeven offset (ticks)", Description = "Enter the number of ticks of profit you want to protect, once the Breakeven Trigger In Ticks has been reached. A positive number will protect that many ticks, a negative number will leave that many ticks at risk, and allow the trade more room to move before closing.")]
	public int BreakevenOffsetTicks { get; set; } = 1;

	public enum SizeType
	{
		Units,
		EquityRisk,
		EquityRiskPercent,
	}

	private IOrder _entryOrder, _stopLossOrder, _firstTakeProfitOrder, _secondTakeProfitOrder;
	private bool _isBreakEvenTriggered;

	public Test()
	{
		Name = "OCO - Ticks (NEW)";
		Description = @"This TMS script calculates the size of your position based on your Stop Loss Distance (in ticks) and your risk (Calculated in terms of Dollar amount, Percentage Risk or Static Units Risk). It can place 1 or 2 take-profit orders (specified in ticks). You also have the option to set an automatic break-even stop loss function.";
	}

	protected override void Initialize()
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}
	}

	protected override void OnEntryOrder(IOrder order)
	{
		CancelOrder(order);

		_entryOrder = OpenPosition(order.Direction, order.TimeInForce);
	}

	private IOrder OpenPosition(OrderDirection direction, TimeInForce timeInForce)
	{
		var action = direction is OrderDirection.Long ? OrderAction.Buy : OrderAction.SellShort;
		var quantity = CalculateQuantity(PositionSizeType, PositionSize, StopLossTicks, RoundingMode.Down);

		return ExecuteMarketOrder(action, (double)quantity, timeInForce, $"{direction.ToString()[0]}E");
	}

	private decimal CalculateQuantity(SizeType sizeType, double size, int distanceTicks, RoundingMode roundingMode)
	{
		decimal volume;

		if (sizeType is SizeType.Units)
		{
			volume = Symbol.NormalizeVolume(size, roundingMode);
		}
		else
		{
			var risk = sizeType is SizeType.EquityRisk ? size : Account.BuyingPower * (size / 100);

			volume = Math.Max(Symbol.MinimumVolume, Symbol.NormalizeVolume(risk / (distanceTicks * Symbol.TickValue), roundingMode));
		}

		return volume;
	}

	protected override void OnOrderUpdate(IOrder order)
	{
		if (_entryOrder == order && order.Status is OrderStatus.Executed)
		{
			PlaceExitOrders();
		}

		//if (_position?.Status is PositionStatus.Close)
		//{
		//	Stop();
		//}

		return;

		if (_firstTakeProfitOrder?.Status is OrderStatus.Pending)
		{
			CancelOrder(_firstTakeProfitOrder);
		}

		if (_secondTakeProfitOrder?.Status is OrderStatus.Pending)
		{
			CancelOrder(_secondTakeProfitOrder);
		}

		if (_stopLossOrder?.Status is OrderStatus.Pending)
		{
			CancelOrder(_stopLossOrder);
		}

		Stop();
	}

	private void PlaceExitOrders()
	{
		Debugger.Break();

		var direction = _entryOrder.Direction is OrderDirection.Long ? 1 : -1;
		var exitAction = direction == 1 ? OrderAction.Sell : OrderAction.BuyToCover;
		var timeInForce = _entryOrder.TimeInForce;

		if (StopLossTicks > 0)
		{
			var stopPrice = _entryOrder.Price - StopLossTicks * Symbol.TickSize * direction;
			var quantity = _entryOrder.Quantity;

			_stopLossOrder = SetStopLoss(_entryOrder, stopPrice, "SL");
		}

		if (FirstTakeProfitTicks > 0 && FirstTakeProfitSizePercent > 0)
		{
			var limitPrice = _entryOrder.Price + FirstTakeProfitTicks * Symbol.TickSize * direction;
			var quantity = Symbol.NormalizeVolume(_entryOrder.Quantity * (FirstTakeProfitSizePercent / 100), RoundingMode.Up);

			_firstTakeProfitOrder = PlaceLimitOrder(exitAction, (double)quantity, limitPrice, timeInForce, "TP1");
		}

		if (SecondTakeProfitTicks > 0 && SecondTakeProfitSizePercent > 0)
		{
			var limitPrice = _entryOrder.Price + SecondTakeProfitTicks * Symbol.TickSize * direction;
			var quantity = (double)Symbol.NormalizeVolume(_entryOrder.Quantity * (SecondTakeProfitSizePercent / 100), RoundingMode.Up);

			if (_firstTakeProfitOrder is not null && _firstTakeProfitOrder.Quantity + quantity > _entryOrder.Quantity)
			{
				quantity = _entryOrder.Quantity - _firstTakeProfitOrder.Quantity;
			}

			_secondTakeProfitOrder = PlaceLimitOrder(exitAction, (double)quantity, limitPrice, timeInForce, "TP2");
		}
	}

	protected override void OnBarUpdate()
	{
		if (Position is null || Position.Status is not PositionStatus.Open)
		{
			return;
		}

		if (_isBreakEvenTriggered is false)
		{
			var price = Bars.Close[^1];
			var ticks = (int)Math.Round((price - Position.EntryPrice) / Symbol.TickSize);
			var stopPrice = (double?)null;

			if (Position.Direction is OrderDirection.Long)
			{
				if (ticks >= BreakevenTriggerTicks)
				{
					stopPrice = Position.EntryPrice + BreakevenOffsetTicks * Symbol.TickSize;
				}
			}
			else
			{
				if (ticks <= -BreakevenTriggerTicks)
				{
					stopPrice = Position.EntryPrice - BreakevenOffsetTicks * Symbol.TickSize;
				}
			}

			if (stopPrice != null)
			{
				ModifyOrder(_stopLossOrder, _stopLossOrder.Quantity, stopPrice, null);
				_isBreakEvenTriggered = true;
			}
		}
	}

	protected override void OnShutdown()
	{
		var orders = new[] { _entryOrder, _stopLossOrder, _firstTakeProfitOrder, _secondTakeProfitOrder };

		foreach (var order in orders)
		{
			if (order?.Status is OrderStatus.Pending)
			{
				CancelOrder(order);
			}
		}
	}

	protected override void OnDestroy()
	{
	}
}
