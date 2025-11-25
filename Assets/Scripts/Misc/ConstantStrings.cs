using UnityEngine;

public static class ConstantStrings
{
    //player prefs
    public const string MyUserName = nameof(MyUserName);
    public const string LastRoom = nameof(LastRoom);

    //player properties
    public const string Position = nameof(Position);
    public const string Waiting = nameof(Waiting);
    public const string MyHealth = nameof(MyHealth);
    public const string MyHand = nameof(MyHand);
    public const string MyDeck = nameof(MyDeck);
    public const string MyDiscard = nameof(MyDiscard);
    public const string MyTroops = nameof(MyTroops);
    public const string Shield = nameof(Shield);
    public const string Sword = nameof(Sword);
    public const string Action = nameof(Action);
    public const string NextRoundShield = nameof(NextRoundShield);
    public const string NextRoundSword = nameof(NextRoundSword);
    public const string NextRoundAction = nameof(NextRoundAction);
    public const string AllCardsPlayed = nameof(AllCardsPlayed);

    //room properties
    public const string GameName = nameof(GameName);
    public const string CanPlay = nameof(CanPlay);
    public const string GameOver = nameof(GameOver);
    public const string JoinAsSpec = nameof(JoinAsSpec);
    public const string MasterDeck = nameof(MasterDeck);
    public const string MasterDiscard = nameof(MasterDiscard);
    public const string CurrentPhase = nameof(CurrentPhase);
    public const string CurrentRound = nameof(CurrentRound);
}
