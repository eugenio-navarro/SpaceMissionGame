// Campaña del README: los contratos se juegan en orden (Tierra → Telescopio → Luna).
// Completar uno con éxito desbloquea el siguiente.
public static class Campaign
{
    // Color de cada destino (tarjetas de contratos y archivo).
    public static UnityEngine.Color DestinationColor(ContractSpec c)
    {
        switch (IndexOf(c))
        {
            case 0: return Palette.Planet;
            case 1: return Palette.Hex("#6B4FB0");
            default: return Palette.Hex("#7D8794");
        }
    }

    public static int IndexOf(ContractSpec c) => TestCatalog.Contracts.IndexOf(c);

    public static ContractSpec Previous(ContractSpec c)
    {
        int i = IndexOf(c);
        return i > 0 ? TestCatalog.Contracts[i - 1] : null;
    }

    public static ContractSpec Next(ContractSpec c)
    {
        int i = IndexOf(c);
        return i >= 0 && i < TestCatalog.Contracts.Count - 1 ? TestCatalog.Contracts[i + 1] : null;
    }

    public static bool IsCompleted(ContractSpec c) => SaveData.BestRank(c.title) == 3;

    public static bool IsUnlocked(ContractSpec c)
    {
        if (c.unlocked) return true; // abierto desde el principio
        var previous = Previous(c);
        return previous != null && IsCompleted(previous);
    }

    public static string LockedReason(ContractSpec c)
    {
        var previous = Previous(c);
        return previous != null ? $"Complete \"{previous.title}\" to unlock" : "Locked";
    }
}
