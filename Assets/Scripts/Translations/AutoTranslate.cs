using System.Collections.Generic;
public static class AutoTranslate
{

public static string Player_Count (string Current,string Max)  { return(Translator.inst.Translate("Player_Count", new(){("Current", Current),("Max", Max)})); }

public static string Attempt_to_reconnect (string Room)  { return(Translator.inst.Translate("Attempt_to_reconnect", new(){("Room", Room)})); }

public static string Failed_to_reconnect (string Room)  { return(Translator.inst.Translate("Failed_to_reconnect", new(){("Room", Room)})); }

public static string Player_Playing (string Player)  { return(Translator.inst.Translate("Player_Playing", new(){("Player", Player)})); }

public static string Player_Spectating (string Player)  { return(Translator.inst.Translate("Player_Spectating", new(){("Player", Player)})); }

public static string Player_Reconnected (string Player)  { return(Translator.inst.Translate("Player_Reconnected", new(){("Player", Player)})); }

public static string Player_Disconnected (string Player)  { return(Translator.inst.Translate("Player_Disconnected", new(){("Player", Player)})); }

public static string Player_Quit (string Player)  { return(Translator.inst.Translate("Player_Quit", new(){("Player", Player)})); }

public static string Waiting_on_Players (string Num)  { return(Translator.inst.Translate("Waiting_on_Players", new(){("Num", Num)})); }

public static string Choose_One_Instruction (string Card)  { return(Translator.inst.Translate("Choose_One_Instruction", new(){("Card", Card)})); }

public static string Discard_Instruction (string Card)  { return(Translator.inst.Translate("Discard_Instruction", new(){("Card", Card)})); }

public static string Target_Instruction (string Player,string Card)  { return(Translator.inst.Translate("Target_Instruction", new(){("Player", Player),("Card", Card)})); }

public static string Pick_Player (string Player)  { return(Translator.inst.Translate("Pick_Player", new(){("Player", Player)})); }

public static string Draw_Card (string Player,string Card)  { return(Translator.inst.Translate("Draw_Card", new(){("Player", Player),("Card", Card)})); }

public static string Draw_Card_Others (string Player)  { return(Translator.inst.Translate("Draw_Card_Others", new(){("Player", Player)})); }

public static string Discard_Card (string Player,string Card)  { return(Translator.inst.Translate("Discard_Card", new(){("Player", Player),("Card", Card)})); }

public static string Discard_Card_Others (string Player)  { return(Translator.inst.Translate("Discard_Card_Others", new(){("Player", Player)})); }

public static string Add_Sword (string Player,string Num)  { return(Translator.inst.Translate("Add_Sword", new(){("Player", Player),("Num", Num)})); }

public static string Lose_Sword (string Player,string Num)  { return(Translator.inst.Translate("Lose_Sword", new(){("Player", Player),("Num", Num)})); }

public static string Add_Shield (string Player,string Num)  { return(Translator.inst.Translate("Add_Shield", new(){("Player", Player),("Num", Num)})); }

public static string Lose_Shield (string Player,string Num)  { return(Translator.inst.Translate("Lose_Shield", new(){("Player", Player),("Num", Num)})); }

public static string Add_Action (string Player,string Num)  { return(Translator.inst.Translate("Add_Action", new(){("Player", Player),("Num", Num)})); }

public static string Lose_Action (string Player,string Num)  { return(Translator.inst.Translate("Lose_Action", new(){("Player", Player),("Num", Num)})); }

public static string Add_Health_Player (string Player,string Num)  { return(Translator.inst.Translate("Add_Health_Player", new(){("Player", Player),("Num", Num)})); }

public static string Lose_Health_Player (string Player,string Num)  { return(Translator.inst.Translate("Lose_Health_Player", new(){("Player", Player),("Num", Num)})); }

public static string Add_Health_Card (string Player,string Card,string Num)  { return(Translator.inst.Translate("Add_Health_Card", new(){("Player", Player),("Card", Card),("Num", Num)})); }

public static string Lose_Health_Card (string Player,string Card,string Num)  { return(Translator.inst.Translate("Lose_Health_Card", new(){("Player", Player),("Card", Card),("Num", Num)})); }

public static string Use_Green (string Num)  { return(Translator.inst.Translate("Use_Green", new(){("Num", Num)})); }

public static string Use_Red (string Num)  { return(Translator.inst.Translate("Use_Red", new(){("Num", Num)})); }

public static string Play_Card (string Player,string Card)  { return(Translator.inst.Translate("Play_Card", new(){("Player", Player),("Card", Card)})); }

public static string Resolve_Card (string Player,string Card)  { return(Translator.inst.Translate("Resolve_Card", new(){("Player", Player),("Card", Card)})); }

public static string Card_Failed (string Card)  { return(Translator.inst.Translate("Card_Failed", new(){("Card", Card)})); }

public static string End_Turn (string Player)  { return(Translator.inst.Translate("End_Turn", new(){("Player", Player)})); }

public static string Stun_Card (string Card,string Num)  { return(Translator.inst.Translate("Stun_Card", new(){("Card", Card),("Num", Num)})); }

public static string Protect_Card (string Card,string Num)  { return(Translator.inst.Translate("Protect_Card", new(){("Card", Card),("Num", Num)})); }

public static string Played_Card_Info (string Card,string Num)  { return(Translator.inst.Translate("Played_Card_Info", new(){("Card", Card),("Num", Num)})); }

public static string Player_Resigned (string Player)  { return(Translator.inst.Translate("Player_Resigned", new(){("Player", Player)})); }

public static string Player_Lost (string Player)  { return(Translator.inst.Translate("Player_Lost", new(){("Player", Player)})); }

public static string DoEnum(ToTranslate thing) {return(Translator.inst.Translate(thing.ToString()));}
}
public enum ToTranslate {
Game_Designer,Last_Update,Translator_Credit,Language,Loading,Select_Region,US_West_Coast,US_East_Coast,Europe,Asia,Offline,Connect,Enter_username,Disconnect,Disconnected_from_server,Failed_to_connect_to_server,Reconnect,Online_Tutorial_1,Online_Tutorial_2,Create,Create_Room_with_players,Enter_hostname,Join,Type_in_username,Encyclopedia,Close,Starting_Health,Type_1,Type_2,Any,Defend,Attack,Play,Type_into_chat,Undo,Short,Long,Confirm,Decline,Use_Green_Instruction,Use_Red_Instruction,Pause_to_Read,Pause_to_Undo,Done,Card,Sword,Shield,Action,Health,Blank,Stunned,Stunned_Text,Protected,Protected_Text,Game_Over,Leave,Tie_Game,Resigned,Skirmisher,Skirmisher_TextOne,Skirmisher_TextTwo,Trader,Trader_TextOne,Trader_TextTwo,Archer,Archer_TextOne,Archer_TextTwo,Dragon,Dragon_TextOne,Dragon_TextTwo,Bee,Bee_TextOne,Ninja,Ninja_TextOne,Squire,Squire_TextOne,Squire_TextTwo,Cannon,Cannon_TextOne,Cannon_TextTwo,Angel,Angel_TextOne,Partier,Partier_TextOne,Partier_TextTwo,Trickster,Trickster_TextOne,Minstrel,Minstrel_TextOne,Minstrel_TextTwo,Acolyte,Acolyte_TextOne,Acolyte_TextTwo,Coven,Coven_TextOne,Coven_TextTwo,Demon,Demon_TextOne,Demon_TextTwo,Security,Security_TextOne,Security_TextTwo,Investor,Investor_TextOne,Gladiator,Gladiator_TextOne,Raider,Raider_TextOne,Guardian,Guardian_TextOne,Guardian_TextTwo,Vampire,Vampire_TextOne,Vampire_TextTwo,Innkeeper,Innkeeper_TextOne,Bureaucrat,Bureaucrat_TextOne,Blacksmith,Blacksmith_TextOne,Vassal,Vassal_TextOne,Mercenary,Mercenary_TextOne,Mercenary_TextTwo,Leprechaun,Leprechaun_TextOne,Berserker,Berserker_TextOne,Berserker_TextTwo,Barbarian,Barbarian_TextOne,Recruiter,Recruiter_TextOne,Recruiter_TextTwo,Mob,Mob_TextOne,Bishop,Bishop_TextOne,Hunter,Hunter_TextOne,Researcher,Researcher_TextOne,Researcher_TextTwo,Golem,Golem_TextOne,Golem_TextTwo,Balancer,Balancer_TextOne,Balancer_TextTwo,Farmer,Farmer_TextOne,Captain,Captain_TextOne,Storyteller,Storyteller_TextOne,Royalty,Royalty_TextOne,Update_0,Update_0_Text,Update_History,Upload_Translation,Download_English
}
