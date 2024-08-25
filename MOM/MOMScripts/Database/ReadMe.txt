Main Guide:


VALUE and Value difference
--------
"VALUE" is a value of TaxMultiplier for example.
"Value" is parameter of RaceTension for example.
Examples: 
<TOWN-BARBARIANS>
        <TaxMultiplier VALUE="1"/>
        <RaceTension Race="RACE-BARBARIANS" Value="0"/>
 </TOWN-BARBARIANS>

We can change example to:
<TOWN-BARBARIANS TaxMultiplier="1">
        <RaceTension Race="RACE-BARBARIANS" Value="0"/>
 </TOWN-BARBARIANS>

Magic Nodes Arcanus and Myrran
----------
Strong nodes in Myrran are created by multiply Arcanus nodes by 130%


Encounters Weak and Normal
----------
Weak encounters are 0.7 "normal Encounters" budget. Only "Normal Encounters" are defined.  


Priority in Skills and Enchancements
----------
Based on that parameter script decide that is order of applying changes.


Priority value:
1- is for all + adds value
11 - is for all * multipliers value


RACE-NON_RACIAL
----------
Is race used in units that can be produce bu any race.


Enchancement
---------- 
Enchancement is "skill" apply on city, wizzard etc. Enchancement types are define in prototype.
Dwarf Mine use same enchancement as normal mine.


Torin
---------- 
Torin will be perceived as a hero with only difference where his upkeep is in mana.


ExpLvl
----------
Each lvl have own skill that add extra points to unit attack, defence etc. depend on unit lvl.
Two skills (fire breath and throw) check on what lvl unit is and rise skill attribute.


TAG-THROW_BONUS and TAG-FIRE_BREATH_BONUS.
----------
That tags are added by spells/ items etc to a target od spells/items etc and they are check by SKILL_THROW and SKILL_FIRE_BREATH. If skill find them on owner it will become more powerfull.
 
Doom dmg
----------
Doom dmg is from Chaos domain but it do not have chaos icon on skills so players do not try to protect from it by spells that add def against chaos.
 
