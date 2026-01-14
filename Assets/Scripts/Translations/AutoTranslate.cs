public static class AutoTranslate 
{ 

public static string Player_Count (string Current,string Max) => Translator.inst.Translate("Player_Count", new() {("Current", Current),("Max", Max)});

public static string Attempt_to_reconnect (string Room) => Translator.inst.Translate("Attempt_to_reconnect", new() {("Room", Room)});

public static string Failed_to_reconnect (string Room) => Translator.inst.Translate("Failed_to_reconnect", new() {("Room", Room)});

public static string Player_Playing (string Player) => Translator.inst.Translate("Player_Playing", new() {("Player", Player)});

public static string Player_Spectating (string Player) => Translator.inst.Translate("Player_Spectating", new() {("Player", Player)});

public static string Player_Reconnected (string Player) => Translator.inst.Translate("Player_Reconnected", new() {("Player", Player)});

public static string Player_Disconnected (string Player) => Translator.inst.Translate("Player_Disconnected", new() {("Player", Player)});

public static string Player_Quit (string Player) => Translator.inst.Translate("Player_Quit", new() {("Player", Player)});

public static string Waiting_on_Players (string Num) => Translator.inst.Translate("Waiting_on_Players", new() {("Num", Num)});

public static string Choose_One_Instruction (string Card) => Translator.inst.Translate("Choose_One_Instruction", new() {("Card", Card)});

public static string Discard_Instruction (string Card) => Translator.inst.Translate("Discard_Instruction", new() {("Card", Card)});

public static string Target_Instruction (string Player,string Card) => Translator.inst.Translate("Target_Instruction", new() {("Player", Player),("Card", Card)});

public static string Pick_Player (string Player) => Translator.inst.Translate("Pick_Player", new() {("Player", Player)});

public static string Draw_Card (string Player,string Card) => Translator.inst.Translate("Draw_Card", new() {("Player", Player),("Card", Card)});

public static string Draw_Card_Others (string Player) => Translator.inst.Translate("Draw_Card_Others", new() {("Player", Player)});

public static string Discard_Card (string Player,string Card) => Translator.inst.Translate("Discard_Card", new() {("Player", Player),("Card", Card)});

public static string Discard_Card_Others (string Player) => Translator.inst.Translate("Discard_Card_Others", new() {("Player", Player)});

public static string Add_Sword (string Player,string Num) => Translator.inst.Translate("Add_Sword", new() {("Player", Player),("Num", Num)});

public static string Lose_Sword (string Player,string Num) => Translator.inst.Translate("Lose_Sword", new() {("Player", Player),("Num", Num)});

public static string Add_Shield (string Player,string Num) => Translator.inst.Translate("Add_Shield", new() {("Player", Player),("Num", Num)});

public static string Lose_Shield (string Player,string Num) => Translator.inst.Translate("Lose_Shield", new() {("Player", Player),("Num", Num)});

public static string Add_Action (string Player,string Num) => Translator.inst.Translate("Add_Action", new() {("Player", Player),("Num", Num)});

public static string Lose_Action (string Player,string Num) => Translator.inst.Translate("Lose_Action", new() {("Player", Player),("Num", Num)});

public static string Add_Health_Player (string Player,string Num) => Translator.inst.Translate("Add_Health_Player", new() {("Player", Player),("Num", Num)});

public static string Lose_Health_Player (string Player,string Num) => Translator.inst.Translate("Lose_Health_Player", new() {("Player", Player),("Num", Num)});

public static string Add_Health_Card (string Player,string Card,string Num) => Translator.inst.Translate("Add_Health_Card", new() {("Player", Player),("Card", Card),("Num", Num)});

public static string Lose_Health_Card (string Player,string Card,string Num) => Translator.inst.Translate("Lose_Health_Card", new() {("Player", Player),("Card", Card),("Num", Num)});

public static string Use_Green (string Num) => Translator.inst.Translate("Use_Green", new() {("Num", Num)});

public static string Use_Red (string Num) => Translator.inst.Translate("Use_Red", new() {("Num", Num)});

public static string Play_Card (string Player,string Card) => Translator.inst.Translate("Play_Card", new() {("Player", Player),("Card", Card)});

public static string Resolve_Card (string Player,string Card) => Translator.inst.Translate("Resolve_Card", new() {("Player", Player),("Card", Card)});

public static string Card_Failed (string Card) => Translator.inst.Translate("Card_Failed", new() {("Card", Card)});

public static string End_Turn (string Player) => Translator.inst.Translate("End_Turn", new() {("Player", Player)});

public static string Stun_Card (string Card,string Num) => Translator.inst.Translate("Stun_Card", new() {("Card", Card),("Num", Num)});

public static string Protect_Card (string Card,string Num) => Translator.inst.Translate("Protect_Card", new() {("Card", Card),("Num", Num)});

public static string Played_Card_Info (string Card,string Num) => Translator.inst.Translate("Played_Card_Info", new() {("Card", Card),("Num", Num)});

public static string Player_Resigned (string Player) => Translator.inst.Translate("Player_Resigned", new() {("Player", Player)});

public static string Player_Lost (string Player) => Translator.inst.Translate("Player_Lost", new() {("Player", Player)});

public static string Game_Designer() => Translator.inst.Translate("Game_Designer");
public static string Last_Update() => Translator.inst.Translate("Last_Update");
public static string Translator_Credit() => Translator.inst.Translate("Translator_Credit");
public static string Language() => Translator.inst.Translate("Language");
public static string Loading() => Translator.inst.Translate("Loading");
public static string Select_Region() => Translator.inst.Translate("Select_Region");
public static string US_West_Coast() => Translator.inst.Translate("US_West_Coast");
public static string US_East_Coast() => Translator.inst.Translate("US_East_Coast");
public static string Europe() => Translator.inst.Translate("Europe");
public static string Asia() => Translator.inst.Translate("Asia");
public static string Offline() => Translator.inst.Translate("Offline");
public static string Connect() => Translator.inst.Translate("Connect");
public static string Enter_username() => Translator.inst.Translate("Enter_username");
public static string Disconnect() => Translator.inst.Translate("Disconnect");
public static string Disconnected_from_server() => Translator.inst.Translate("Disconnected_from_server");
public static string Failed_to_connect_to_server() => Translator.inst.Translate("Failed_to_connect_to_server");
public static string Reconnect() => Translator.inst.Translate("Reconnect");
public static string Online_Tutorial_1() => Translator.inst.Translate("Online_Tutorial_1");
public static string Online_Tutorial_2() => Translator.inst.Translate("Online_Tutorial_2");
public static string Create() => Translator.inst.Translate("Create");
public static string Create_Room_with_players() => Translator.inst.Translate("Create_Room_with_players");
public static string Enter_hostname() => Translator.inst.Translate("Enter_hostname");
public static string Join() => Translator.inst.Translate("Join");
public static string Type_in_username() => Translator.inst.Translate("Type_in_username");
public static string Encyclopedia() => Translator.inst.Translate("Encyclopedia");
public static string Close() => Translator.inst.Translate("Close");
public static string Starting_Health() => Translator.inst.Translate("Starting_Health");
public static string Type_1() => Translator.inst.Translate("Type_1");
public static string Type_2() => Translator.inst.Translate("Type_2");
public static string Any() => Translator.inst.Translate("Any");
public static string Defend() => Translator.inst.Translate("Defend");
public static string Attack() => Translator.inst.Translate("Attack");
public static string Play() => Translator.inst.Translate("Play");
public static string Type_into_chat() => Translator.inst.Translate("Type_into_chat");
public static string Undo() => Translator.inst.Translate("Undo");
public static string Short() => Translator.inst.Translate("Short");
public static string Long() => Translator.inst.Translate("Long");
public static string Confirm() => Translator.inst.Translate("Confirm");
public static string Decline() => Translator.inst.Translate("Decline");
public static string Use_Green_Instruction() => Translator.inst.Translate("Use_Green_Instruction");
public static string Use_Red_Instruction() => Translator.inst.Translate("Use_Red_Instruction");
public static string Pause_to_Read() => Translator.inst.Translate("Pause_to_Read");
public static string Pause_to_Undo() => Translator.inst.Translate("Pause_to_Undo");
public static string Done() => Translator.inst.Translate("Done");
public static string Card() => Translator.inst.Translate("Card");
public static string Sword() => Translator.inst.Translate("Sword");
public static string Shield() => Translator.inst.Translate("Shield");
public static string Action() => Translator.inst.Translate("Action");
public static string Health() => Translator.inst.Translate("Health");
public static string Blank() => Translator.inst.Translate("Blank");
public static string Stunned() => Translator.inst.Translate("Stunned");
public static string Stunned_Text() => Translator.inst.Translate("Stunned_Text");
public static string Protected() => Translator.inst.Translate("Protected");
public static string Protected_Text() => Translator.inst.Translate("Protected_Text");
public static string Game_Over() => Translator.inst.Translate("Game_Over");
public static string Leave() => Translator.inst.Translate("Leave");
public static string Tie_Game() => Translator.inst.Translate("Tie_Game");
public static string Resigned() => Translator.inst.Translate("Resigned");
public static string Skirmisher() => Translator.inst.Translate("Skirmisher");
public static string Skirmisher_TextOne() => Translator.inst.Translate("Skirmisher_TextOne");
public static string Skirmisher_TextTwo() => Translator.inst.Translate("Skirmisher_TextTwo");
public static string Trader() => Translator.inst.Translate("Trader");
public static string Trader_TextOne() => Translator.inst.Translate("Trader_TextOne");
public static string Trader_TextTwo() => Translator.inst.Translate("Trader_TextTwo");
public static string Archer() => Translator.inst.Translate("Archer");
public static string Archer_TextOne() => Translator.inst.Translate("Archer_TextOne");
public static string Archer_TextTwo() => Translator.inst.Translate("Archer_TextTwo");
public static string Dragon() => Translator.inst.Translate("Dragon");
public static string Dragon_TextOne() => Translator.inst.Translate("Dragon_TextOne");
public static string Dragon_TextTwo() => Translator.inst.Translate("Dragon_TextTwo");
public static string Bee() => Translator.inst.Translate("Bee");
public static string Bee_TextOne() => Translator.inst.Translate("Bee_TextOne");
public static string Ninja() => Translator.inst.Translate("Ninja");
public static string Ninja_TextOne() => Translator.inst.Translate("Ninja_TextOne");
public static string Squire() => Translator.inst.Translate("Squire");
public static string Squire_TextOne() => Translator.inst.Translate("Squire_TextOne");
public static string Squire_TextTwo() => Translator.inst.Translate("Squire_TextTwo");
public static string Cannon() => Translator.inst.Translate("Cannon");
public static string Cannon_TextOne() => Translator.inst.Translate("Cannon_TextOne");
public static string Cannon_TextTwo() => Translator.inst.Translate("Cannon_TextTwo");
public static string Angel() => Translator.inst.Translate("Angel");
public static string Angel_TextOne() => Translator.inst.Translate("Angel_TextOne");
public static string Partier() => Translator.inst.Translate("Partier");
public static string Partier_TextOne() => Translator.inst.Translate("Partier_TextOne");
public static string Partier_TextTwo() => Translator.inst.Translate("Partier_TextTwo");
public static string Trickster() => Translator.inst.Translate("Trickster");
public static string Trickster_TextOne() => Translator.inst.Translate("Trickster_TextOne");
public static string Minstrel() => Translator.inst.Translate("Minstrel");
public static string Minstrel_TextOne() => Translator.inst.Translate("Minstrel_TextOne");
public static string Minstrel_TextTwo() => Translator.inst.Translate("Minstrel_TextTwo");
public static string Acolyte() => Translator.inst.Translate("Acolyte");
public static string Acolyte_TextOne() => Translator.inst.Translate("Acolyte_TextOne");
public static string Acolyte_TextTwo() => Translator.inst.Translate("Acolyte_TextTwo");
public static string Coven() => Translator.inst.Translate("Coven");
public static string Coven_TextOne() => Translator.inst.Translate("Coven_TextOne");
public static string Coven_TextTwo() => Translator.inst.Translate("Coven_TextTwo");
public static string Demon() => Translator.inst.Translate("Demon");
public static string Demon_TextOne() => Translator.inst.Translate("Demon_TextOne");
public static string Demon_TextTwo() => Translator.inst.Translate("Demon_TextTwo");
public static string Security() => Translator.inst.Translate("Security");
public static string Security_TextOne() => Translator.inst.Translate("Security_TextOne");
public static string Security_TextTwo() => Translator.inst.Translate("Security_TextTwo");
public static string Investor() => Translator.inst.Translate("Investor");
public static string Investor_TextOne() => Translator.inst.Translate("Investor_TextOne");
public static string Gladiator() => Translator.inst.Translate("Gladiator");
public static string Gladiator_TextOne() => Translator.inst.Translate("Gladiator_TextOne");
public static string Raider() => Translator.inst.Translate("Raider");
public static string Raider_TextOne() => Translator.inst.Translate("Raider_TextOne");
public static string Guardian() => Translator.inst.Translate("Guardian");
public static string Guardian_TextOne() => Translator.inst.Translate("Guardian_TextOne");
public static string Guardian_TextTwo() => Translator.inst.Translate("Guardian_TextTwo");
public static string Vampire() => Translator.inst.Translate("Vampire");
public static string Vampire_TextOne() => Translator.inst.Translate("Vampire_TextOne");
public static string Vampire_TextTwo() => Translator.inst.Translate("Vampire_TextTwo");
public static string Innkeeper() => Translator.inst.Translate("Innkeeper");
public static string Innkeeper_TextOne() => Translator.inst.Translate("Innkeeper_TextOne");
public static string Bureaucrat() => Translator.inst.Translate("Bureaucrat");
public static string Bureaucrat_TextOne() => Translator.inst.Translate("Bureaucrat_TextOne");
public static string Blacksmith() => Translator.inst.Translate("Blacksmith");
public static string Blacksmith_TextOne() => Translator.inst.Translate("Blacksmith_TextOne");
public static string Vassal() => Translator.inst.Translate("Vassal");
public static string Vassal_TextOne() => Translator.inst.Translate("Vassal_TextOne");
public static string Mercenary() => Translator.inst.Translate("Mercenary");
public static string Mercenary_TextOne() => Translator.inst.Translate("Mercenary_TextOne");
public static string Mercenary_TextTwo() => Translator.inst.Translate("Mercenary_TextTwo");
public static string Leprechaun() => Translator.inst.Translate("Leprechaun");
public static string Leprechaun_TextOne() => Translator.inst.Translate("Leprechaun_TextOne");
public static string Berserker() => Translator.inst.Translate("Berserker");
public static string Berserker_TextOne() => Translator.inst.Translate("Berserker_TextOne");
public static string Berserker_TextTwo() => Translator.inst.Translate("Berserker_TextTwo");
public static string Barbarian() => Translator.inst.Translate("Barbarian");
public static string Barbarian_TextOne() => Translator.inst.Translate("Barbarian_TextOne");
public static string Recruiter() => Translator.inst.Translate("Recruiter");
public static string Recruiter_TextOne() => Translator.inst.Translate("Recruiter_TextOne");
public static string Recruiter_TextTwo() => Translator.inst.Translate("Recruiter_TextTwo");
public static string Mob() => Translator.inst.Translate("Mob");
public static string Mob_TextOne() => Translator.inst.Translate("Mob_TextOne");
public static string Bishop() => Translator.inst.Translate("Bishop");
public static string Bishop_TextOne() => Translator.inst.Translate("Bishop_TextOne");
public static string Hunter() => Translator.inst.Translate("Hunter");
public static string Hunter_TextOne() => Translator.inst.Translate("Hunter_TextOne");
public static string Researcher() => Translator.inst.Translate("Researcher");
public static string Researcher_TextOne() => Translator.inst.Translate("Researcher_TextOne");
public static string Researcher_TextTwo() => Translator.inst.Translate("Researcher_TextTwo");
public static string Golem() => Translator.inst.Translate("Golem");
public static string Golem_TextOne() => Translator.inst.Translate("Golem_TextOne");
public static string Golem_TextTwo() => Translator.inst.Translate("Golem_TextTwo");
public static string Balancer() => Translator.inst.Translate("Balancer");
public static string Balancer_TextOne() => Translator.inst.Translate("Balancer_TextOne");
public static string Balancer_TextTwo() => Translator.inst.Translate("Balancer_TextTwo");
public static string Farmer() => Translator.inst.Translate("Farmer");
public static string Farmer_TextOne() => Translator.inst.Translate("Farmer_TextOne");
public static string Captain() => Translator.inst.Translate("Captain");
public static string Captain_TextOne() => Translator.inst.Translate("Captain_TextOne");
public static string Storyteller() => Translator.inst.Translate("Storyteller");
public static string Storyteller_TextOne() => Translator.inst.Translate("Storyteller_TextOne");
public static string Royalty() => Translator.inst.Translate("Royalty");
public static string Royalty_TextOne() => Translator.inst.Translate("Royalty_TextOne");
public static string Update_0() => Translator.inst.Translate("Update_0");
public static string Update_0_Text() => Translator.inst.Translate("Update_0_Text");
public static string Update_History() => Translator.inst.Translate("Update_History");
public static string Upload_Translation() => Translator.inst.Translate("Upload_Translation");
public static string Download_English() => Translator.inst.Translate("Download_English");
public static string Update_1() => Translator.inst.Translate("Update_1");
public static string Update_1_Text() => Translator.inst.Translate("Update_1_Text");
}
public enum ToTranslate {
Game_Designer,Last_Update,Translator_Credit,Language,Loading,Select_Region,US_West_Coast,US_East_Coast,Europe,Asia,Offline,Connect,Enter_username,Disconnect,Disconnected_from_server,Failed_to_connect_to_server,Reconnect,Online_Tutorial_1,Online_Tutorial_2,Create,Create_Room_with_players,Enter_hostname,Join,Type_in_username,Encyclopedia,Close,Starting_Health,Type_1,Type_2,Any,Defend,Attack,Play,Type_into_chat,Undo,Short,Long,Confirm,Decline,Use_Green_Instruction,Use_Red_Instruction,Pause_to_Read,Pause_to_Undo,Done,Card,Sword,Shield,Action,Health,Blank,Stunned,Stunned_Text,Protected,Protected_Text,Game_Over,Leave,Tie_Game,Resigned,Skirmisher,Skirmisher_TextOne,Skirmisher_TextTwo,Trader,Trader_TextOne,Trader_TextTwo,Archer,Archer_TextOne,Archer_TextTwo,Dragon,Dragon_TextOne,Dragon_TextTwo,Bee,Bee_TextOne,Ninja,Ninja_TextOne,Squire,Squire_TextOne,Squire_TextTwo,Cannon,Cannon_TextOne,Cannon_TextTwo,Angel,Angel_TextOne,Partier,Partier_TextOne,Partier_TextTwo,Trickster,Trickster_TextOne,Minstrel,Minstrel_TextOne,Minstrel_TextTwo,Acolyte,Acolyte_TextOne,Acolyte_TextTwo,Coven,Coven_TextOne,Coven_TextTwo,Demon,Demon_TextOne,Demon_TextTwo,Security,Security_TextOne,Security_TextTwo,Investor,Investor_TextOne,Gladiator,Gladiator_TextOne,Raider,Raider_TextOne,Guardian,Guardian_TextOne,Guardian_TextTwo,Vampire,Vampire_TextOne,Vampire_TextTwo,Innkeeper,Innkeeper_TextOne,Bureaucrat,Bureaucrat_TextOne,Blacksmith,Blacksmith_TextOne,Vassal,Vassal_TextOne,Mercenary,Mercenary_TextOne,Mercenary_TextTwo,Leprechaun,Leprechaun_TextOne,Berserker,Berserker_TextOne,Berserker_TextTwo,Barbarian,Barbarian_TextOne,Recruiter,Recruiter_TextOne,Recruiter_TextTwo,Mob,Mob_TextOne,Bishop,Bishop_TextOne,Hunter,Hunter_TextOne,Researcher,Researcher_TextOne,Researcher_TextTwo,Golem,Golem_TextOne,Golem_TextTwo,Balancer,Balancer_TextOne,Balancer_TextTwo,Farmer,Farmer_TextOne,Captain,Captain_TextOne,Storyteller,Storyteller_TextOne,Royalty,Royalty_TextOne,Update_0,Update_0_Text,Update_History,Upload_Translation,Download_English,Update_1,Update_1_Text
}
