using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;

public class PlayerManager{
    public Dictionary<string, int> playerScores = [];
    public ImmutableArray<string> players = [];
    public int currentPlayerTurn = 0;

    public PlayerManager(string player)
    {
        playerScores = new (){{player, 0}};
    }

    public PlayerManager(List<string> players)
    {
        players.ForEach(p => playerScores.Add(p, 0));
    }

    public void AddPlayer(string player)
    {
        playerScores.Add(player, 0);
    }

    public int AddToPlayerScore(string player, int scoreToAdd)
    {
        playerScores[player] += scoreToAdd;
        return playerScores[player];
    }

    public void ResetPlayerScores()
    {
        playerScores = [];
    }

    public void StartGame()
    {
        players = [.. playerScores.Keys];
    }

    public string GetWhoseTurnItIs()
    {
        return players[currentPlayerTurn];
    }

    public string AdvanceTurn()
    {
        currentPlayerTurn = (currentPlayerTurn + 1) % players.Length;
        return players[currentPlayerTurn];
    }
}