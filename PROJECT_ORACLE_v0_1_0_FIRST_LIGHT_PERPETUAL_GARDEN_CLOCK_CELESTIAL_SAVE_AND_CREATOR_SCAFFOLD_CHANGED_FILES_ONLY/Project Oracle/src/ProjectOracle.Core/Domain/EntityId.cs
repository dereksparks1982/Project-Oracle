namespace ProjectOracle.Domain;

public readonly record struct EntityId(string Value)
{
    public override string ToString() => Value;
}
