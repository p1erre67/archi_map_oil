using PriceWatch.SharedKernel.Domain.Primitives;

namespace PriceWatch.Modules.Prices.Domain.Entities;

public sealed class Brand
{
    public int ExternalId { get; private set; }
    public string Name { get; private set; }
    public string ShortName { get; private set; }
    public int NbStations { get; private set; }

    private Brand(int externalId, string name, string shortName, int nbStations)
    {
        ExternalId = externalId;
        Name = name;
        ShortName = shortName;
        NbStations = nbStations;
    }

    public static Brand Create(int externalId, string name, string shortName, int nbStations)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNullOrWhiteSpace(shortName, nameof(shortName));
        Guard.AgainstNegative(nbStations, nameof(nbStations));

        return new(externalId, name, shortName, nbStations);
    }

    public void UpdateInfo(string name, string shortName, int nbStations)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNullOrWhiteSpace(shortName, nameof(shortName));
        Guard.AgainstNegative(nbStations, nameof(nbStations));

        Name = name;
        ShortName = shortName;
        NbStations = nbStations;
    }

#pragma warning disable CS8618
    private Brand() { }
#pragma warning restore CS8618
}
