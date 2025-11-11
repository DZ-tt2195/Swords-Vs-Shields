using UnityEngine;

public class Trader : CardType
{
    public Trader(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetAction() >= 1)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(-1, logged);
        player.ChooseButtonInPopup(new() { new("2 Sword", Sword), new("2 Shield", Shield) }, "Choose One Instruction", new(0, 325));

        void Sword()
        {
            player.SwordRPC(2, logged);
        }
        void Shield()
        {
            player.ShieldRPC(2, logged);
        }
    }
}
