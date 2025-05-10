using Mech.Models;

public interface IBattleStrategy
{
    void Execute(Mecha player, Mecha opponent);
}

/// <summary>
/// An aggressive strategy that focuses on increasing attack while reducing the opponent's defense.
/// </summary>
public class AggressiveStrategy : IBattleStrategy
{
    public void Execute(Mecha player, Mecha opponent)
    {
        Console.WriteLine("Using Aggressive Strategy...");
        player.ModifyStats(attackDelta: 10, defenseDelta: 0);
        opponent.ModifyStats(attackDelta: 0, defenseDelta: -5);
    }
}

/// <summary>
/// A defensive strategy that focuses on increasing defense while reducing the opponent's attack.
/// </summary>
public class DefensiveStrategy : IBattleStrategy
{
    public void Execute(Mecha player, Mecha opponent)
    {
        Console.WriteLine("Using Defensive Strategy...");
        player.ModifyStats(attackDelta: 0, defenseDelta: 10);
        opponent.ModifyStats(attackDelta: -5, defenseDelta: 0);
    }
}

/// <summary>
/// A balanced strategy that provides a moderate boost to both attack and defense.
/// </summary>
public class BalancedStrategy : IBattleStrategy
{
    public void Execute(Mecha player, Mecha opponent)
    {
        Console.WriteLine("Using Balanced Strategy...");
        player.ModifyStats(attackDelta: 5, defenseDelta: 5);
    }
}