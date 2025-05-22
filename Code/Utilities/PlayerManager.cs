using System.Collections.Generic;
using System.Linq;

public class PlayerManager
{
    public Dictionary<string, int> playerScores = [];
    public List<string> players = [];
    public int currentPlayerTurnIndex = 0;

    private bool isLastRound = false;
    private int lastRoundStartingIndex;

    public PlayerManager() { }

    public PlayerManager(string player)
    {
        playerScores.Add(player, 0);
    }

    public PlayerManager(List<string> players)
    {
        players.ForEach(p => playerScores.Add(p, 0));
    }

    public PlayerManager(PlayerManager pm)
    {
        playerScores = pm.playerScores;
        players = pm.players;
        currentPlayerTurnIndex = pm.currentPlayerTurnIndex;
    }

    public PlayerManager GetLastRoundPlayerManager(string winningPlayer)
    {
        var lrpm = GetPlayerManagerExceptPlayer(winningPlayer);
        lrpm.MakeLastRound();
        return lrpm;
    }

    public void MakeLastRound()
    {
        isLastRound = true;
        lastRoundStartingIndex = currentPlayerTurnIndex;
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

    public int AddToPlayerScore(PlayerScore ps) => AddToPlayerScore(ps.Player, ps.Score);

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
        return players[currentPlayerTurnIndex];
    }

    public bool TryAdvanceTurnOnLastRound(out string nextPlayer)
    {
        if (GetIncrementedCurrentPlayerTurn() == lastRoundStartingIndex)
        {
            nextPlayer = null;
            return false;
        }

        nextPlayer = AdvanceTurn();
        return true;
    }

    public string AdvanceTurn()
    {
        currentPlayerTurnIndex = GetIncrementedCurrentPlayerTurn();
        return players[currentPlayerTurnIndex];
    }

    public bool TryGetPlayerAtScore(int score, out PlayerScore player)
    {
        var playerAtScore = playerScores.SingleOrDefault(ps => ps.Value >= score);
        if (playerAtScore.Equals(default(KeyValuePair<string, int>))) //no null, use default
        {
            player = new PlayerScore("", 0);
            return false;
        }

        player = new PlayerScore(playerAtScore.Key, playerAtScore.Value);
        return true;
    }

    private int GetIncrementedCurrentPlayerTurn() => (currentPlayerTurnIndex + 1) % players.Count;
    
    private PlayerManager GetPlayerManagerExceptPlayer(string player)
    {
        var newPlayerScores = playerScores.Where(ps => ps.Key.Equals(player)).ToDictionary();
        List<string> newPlayers = [.. newPlayerScores.Keys];
        var playerNext = players[(currentPlayerTurnIndex + 1) % players.Count];
        return new()
        {
            playerScores = newPlayerScores,
            players = newPlayers,
            currentPlayerTurnIndex = newPlayers.IndexOf(playerNext)
        };
    }
}

public record PlayerScore(string Player, int Score);