namespace BandKeeper.Widget.Models;

public sealed record AdapterOption(string Id, string Name)
{
    public override string ToString() => Name;
}
