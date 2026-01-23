public static class OnlineTranslate 
{
public static string Online_Player_Playing (string Player) => $"Online_Player_Playing\tPlayer\t{Player}";
public static string Online_Player_Spectating (string Player) => $"Online_Player_Spectating\tPlayer\t{Player}";
public static string Online_Player_Reconnected (string Player) => $"Online_Player_Reconnected\tPlayer\t{Player}";
public static string Online_Player_Disconnected (string Player) => $"Online_Player_Disconnected\tPlayer\t{Player}";
public static string Online_Player_Quit (string Player) => $"Online_Player_Quit\tPlayer\t{Player}";
public static string Online_Waiting_on_Players (string Num) => $"Online_Waiting_on_Players\tNum\t{Num}";
public static string Online_Draw_Card (string Player,string Card) => $"Online_Draw_Card\tPlayer\t{Player}\tCard\t{Card}";
public static string Online_Draw_Card_Others (string Player) => $"Online_Draw_Card_Others\tPlayer\t{Player}";
public static string Online_Discard_Card (string Player,string Card) => $"Online_Discard_Card\tPlayer\t{Player}\tCard\t{Card}";
public static string Online_Discard_Card_Others (string Player) => $"Online_Discard_Card_Others\tPlayer\t{Player}";
public static string Online_Add_Sword (string Player,string Num) => $"Online_Add_Sword\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Lose_Sword (string Player,string Num) => $"Online_Lose_Sword\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Add_Shield (string Player,string Num) => $"Online_Add_Shield\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Lose_Shield (string Player,string Num) => $"Online_Lose_Shield\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Add_Action (string Player,string Num) => $"Online_Add_Action\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Lose_Action (string Player,string Num) => $"Online_Lose_Action\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Add_Health_Player (string Player,string Num) => $"Online_Add_Health_Player\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Lose_Health_Player (string Player,string Num) => $"Online_Lose_Health_Player\tPlayer\t{Player}\tNum\t{Num}";
public static string Online_Add_Health_Card (string Player,string Card,string Num) => $"Online_Add_Health_Card\tPlayer\t{Player}\tCard\t{Card}\tNum\t{Num}";
public static string Online_Lose_Health_Card (string Player,string Card,string Num) => $"Online_Lose_Health_Card\tPlayer\t{Player}\tCard\t{Card}\tNum\t{Num}";
public static string Online_Use_Green (string Num) => $"Online_Use_Green\tNum\t{Num}";
public static string Online_Use_Red (string Num) => $"Online_Use_Red\tNum\t{Num}";
public static string Online_Play_Card (string Player,string Card) => $"Online_Play_Card\tPlayer\t{Player}\tCard\t{Card}";
public static string Online_Resolve_Card (string Player,string Card) => $"Online_Resolve_Card\tPlayer\t{Player}\tCard\t{Card}";
public static string Online_Card_Failed (string Card) => $"Online_Card_Failed\tCard\t{Card}";
public static string Online_End_Turn (string Player) => $"Online_End_Turn\tPlayer\t{Player}";
public static string Online_Stun_Card (string Card,string Num) => $"Online_Stun_Card\tCard\t{Card}\tNum\t{Num}";
public static string Online_Protect_Card (string Card,string Num) => $"Online_Protect_Card\tCard\t{Card}\tNum\t{Num}";
public static string Online_Played_Card_Info (string Card,string Num) => $"Online_Played_Card_Info\tCard\t{Card}\tNum\t{Num}";
public static string Online_Tie_Game () => $"Online_Tie_Game";
public static string Online_Player_Resigned (string Player) => $"Online_Player_Resigned\tPlayer\t{Player}";
public static string Online_Player_Lost (string Player) => $"Online_Player_Lost\tPlayer\t{Player}";
}
