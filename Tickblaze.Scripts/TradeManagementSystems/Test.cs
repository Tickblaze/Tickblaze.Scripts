using System.ComponentModel;
using Tickblaze.Scripts.Api.Interfaces.Orders;

namespace Tickblaze.Scripts.TradeManagementSystems;

[Browsable(false)]
public class Test : TradeManagementSystem
{
	[Parameter("Take-profit ticks"), NumericRange(0, int.MaxValue)]
	public int TakeProfitTicks { get; set; } = 10;

	[Parameter("Stop-loss ticks"), NumericRange(0, int.MaxValue)]
	public int StopLossTicks { get; set; } = 20;

	[Parameter("Break-even trigger ticks")]
	public int BreakevenTriggerTicks { get; set; } = 5;

	private IOrder _entryOrder, _stopLossOrder, _takeProfitOrder;
	private bool _isBreakEvenTriggered;

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void OnEntryOrder(IOrder order)
	{
		_entryOrder = order;

		var direction = order.Direction is OrderDirection.Long ? 1 : -1;
		var exitAction = direction == 1 ? OrderAction.Sell : OrderAction.BuyToCover;
		var stopLossPrice = order.Price - StopLossTicks * Symbol.TickSize * direction;
		var takeProfitPrice = order.Price + TakeProfitTicks * Symbol.TickSize * direction;

		_stopLossOrder = PlaceStopOrder(exitAction, OrderType.Stop, order.Quantity, stopLossPrice, TimeInForce.GoodTillCancel, "SL");
		_takeProfitOrder = PlaceLimitOrder(exitAction, OrderType.Limit, order.Quantity, takeProfitPrice, TimeInForce.GoodTillCancel, "TP");

		var positions = Positions;
		var orders = Orders;
	}

	protected override void OnOrderUpdate(IOrder order)
	{
		if (order == _stopLossOrder)
		{
			CancelOrder(_takeProfitOrder);
			Stop();
		}
		else if (order == _takeProfitOrder)
		{
			CancelOrder(_stopLossOrder);
			Stop();
		}
	}

	protected override void OnBarUpdate()
	{
		var position = Positions[^1];
		if (position is null && position.Status is not PositionStatus.Open)
		{
			return;
		}

		if (_isBreakEvenTriggered is false)
		{
			var price = Bars.Close[^1];
			var ticks = (int)Math.Round((price - position.EntryPrice) / Symbol.TickSize);

			if (position.Direction is OrderDirection.Long)
			{
				if (ticks >= BreakevenTriggerTicks)
				{
					ModifyOrder(_stopLossOrder, _stopLossOrder.Quantity, position.EntryPrice, null);

					_isBreakEvenTriggered = true;
				}
			}
			else
			{
				if (ticks <= -BreakevenTriggerTicks)
				{
					ModifyOrder(_stopLossOrder, _stopLossOrder.Quantity, position.EntryPrice, null);

					_isBreakEvenTriggered = true;
				}
			}
		}
	}

	protected override void OnShutdown()
	{
	}

	protected override void OnDestroy()
	{
	}
}
