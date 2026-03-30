using PriceWatch.Modules.Cards.Domain.Enums;
using PriceWatch.Modules.Cards.Domain.Errors;
using PriceWatch.Modules.Cards.Domain.Events;
using PriceWatch.Modules.Cards.Domain.ValueObjects;
using PriceWatch.SharedKernel.Domain.Primitives;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Domain.Entities;

/// <summary>
/// Aggregate root for a virtual fuel card.
/// Owns a collection of Transactions. All mutations go through domain methods.
/// </summary>
public sealed class Card : AggregateRoot<CardId>
{
    private readonly List<Transaction> _transactions = [];

    public CardNumber CardNumber { get; private set; }
    public string HolderName { get; private set; }
    public decimal Balance { get; private set; }
    public CardStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    private Card(CardId id, CardNumber cardNumber, string holderName) : base(id)
    {
        CardNumber = cardNumber;
        HolderName = holderName;
        Balance = 0m;
        Status = CardStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Card> Create(string holderName)
    {
        if (string.IsNullOrWhiteSpace(holderName))
            return Error.Validation("Card.HolderNameRequired", "Holder name is required.");

        var card = new Card(CardId.New(), CardNumber.Generate(), holderName.Trim());
        card.RaiseDomainEvent(new CardCreatedEvent(card.Id, card.CardNumber, card.HolderName));

        return card;
    }

    public Result TopUp(decimal amount)
    {
        if (amount <= 0)
            return CardErrors.InvalidTopUpAmount;

        if (Status == CardStatus.Blocked)
            return CardErrors.CardBlocked;

        Balance += amount;
        _transactions.Add(Transaction.CreateTopUp(amount));
        RaiseDomainEvent(new CardToppedUpEvent(Id, amount, Balance));

        return Result.Success();
    }

    public Result RecordFuelTransaction(
        string fuelType,
        decimal liters,
        decimal pricePerLiter,
        string stationId)
    {
        if (liters <= 0)
            return CardErrors.InvalidFuelAmount;

        if (pricePerLiter <= 0)
            return CardErrors.InvalidPricePerLiter;

        if (Status == CardStatus.Blocked)
            return CardErrors.CardBlocked;

        var totalCharged = liters * pricePerLiter;

        if (Balance < totalCharged)
            return CardErrors.InsufficientBalance;

        Balance -= totalCharged;
        _transactions.Add(Transaction.CreateFuelPayment(totalCharged, liters, fuelType, stationId));
        RaiseDomainEvent(new FuelTransactionRecordedEvent(Id, fuelType, liters, pricePerLiter, totalCharged, stationId));

        return Result.Success();
    }

    public Result Block()
    {
        if (Status == CardStatus.Blocked)
            return CardErrors.AlreadyBlocked;

        Status = CardStatus.Blocked;

        return Result.Success();
    }

#pragma warning disable CS8618
    private Card() : base(default!) { }
#pragma warning restore CS8618
}
