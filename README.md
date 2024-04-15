# Zombie_Hunter

![Test Image 3](/Image/header.png)

## Overview

C# shooting game co-produced with bitken1113.

## Requirement

Windows10 / 11

## Usage

To play, download the "Game" folder and run the shooting.exe file.
The initial high score is by bitKen1113.
NOTE : There is a confirmed issue with zombies stopping, but a fix is scheduled for 2024 4/15.
The sound may sound choppy, but this is a system specification.

## Description

[Attack]
Left mouse click
[Move]
(against the screen, the line of sight is the mouse cursor)
WASD
[Switch Weapon]
Q : Knife
1 : handgun
2 : Shotgun
3 : Assault Rifle
4 : Sniper Rifle
[Reload]
R
[Play]
When you touch the green floor, you will progress to the next stage.
If a zombie is alive on the stage, it is red and cannot be touched.
Weapons and ammunition are acquired by touching the icons on the stage.
Ammunition is not reloaded automatically.
Green zombies will claw and attack if the hunter is within a certain distance.
Zombies with red edges will fire blood bullets in addition to scratching.
Bosses...? You'll have to play to find out.
The hunter's health will recover in stages over a certain period of time after being hit.
If the hunter is standing still at this time, his speed will increase.
Sniper rifle bullets have the characteristic of penetrating zombies.
I wish you good luck.

[Program]
<Text>
All_Points : Point type data with a new line for each stage of points used for the WayPoint algorithm.
Points included in All_Rect_ex described below are excluded.
All_Rect : Rectangle type data which is a wall for each stage.
All_Rect_ex : Rectangle type data that is extended from All_Rect data by the radius of zombie.
All_Table : 2D array to the target point for WayPoint algorithm.
<WayPoint>
The program calculates the path points by WayPoint algorithm based on the size of the stage and wall information set in the array and outputs them to the Text folder.
Since it takes time to execute, it is separated from the game itself.
<shooting>
Game Manager. reads the files in the Text folder and creates the stage.
Manage and move players, zombies, weapons, bullets, etc.

## Author

Porosuke
bitKen1113

## Licence

This project is licensed under the MIT License, see the LICENSE file for details.
