namespace PriceWatch.Modules.Prices.Domain;

/// <summary>
/// Regroupe les alias de carburants pour le filtrage.
/// L'API gouv retourne parfois "SP95" et "SP95-E10" comme deux types distincts
/// alors qu'on veut les traiter comme une seule famille côté utilisateur.
/// </summary>
internal static class FuelTypes
{
    public static List<string> Resolve(string fuelType) =>
        fuelType == "SP95"
            ? new List<string> { "SP95", "SP95-E10" }
            : new List<string> { fuelType };
}
