using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Helper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
#endif


namespace RPGMaker.Codebase.CoreSystem.Lib.Migration.ExecutionClasses
{
    /// <summary>1
    /// Version1.0.7へのMigration用クラス
    /// </summary>
    internal class Migration_1_0_7_Class : IExecutionClassCore
    {
        private int dataCount = 270;
        private string[,] objectNames = new string[270, 7]
        {
            {"008229ae-3500-45a0-be9b-4ae020c4c2b7", "篝火台（炎、紫）", "Object 174", "物体 174", "篝火台（炎、紫）", "Fire Pit(Fire/Purple)", "火炉(火/紫)"},
            {"01329ab5-e386-49aa-84a1-dffbd6058abc", "Weapon（剣、小、七色）", "Object 227", "物体 227", "Weapon（剣、小、七色）", "Weapon(Sword/Small/Rainbow)", "Weapon(剑/小/七种颜色)"},
            {"06065437-8f8b-418e-94dc-905fd5ddbd2c", "Sphere（緑、暗）", "Object 69", "物体 69", "Sphere（緑、暗）", "Sphere(Green/Dark)", "Sphere(绿/暗)"},
            {"0ada6636-f27d-4d74-9e0d-5b868d86ed2b", "波", "Object 155", "物体 155", "波", "Wave", "波"},
            {"0ba15718-ba79-4f87-b398-562f8d077f97", "Flame（十字、アニメ、緑）", "Object 116", "物体 116", "Flame（十字、アニメ、緑）", "Flame(Cross/Animation/Green)", "Flame(十字/动画/绿)"},
            {"0c0fa342-52d5-40aa-b34e-5636255962b6", "Vehicle（幌馬車）", "Object 259", "物体 259", "Vehicle（幌馬車）", "Vehicle(Wagon)", "Vehicle(帆马车)"},
            {"0dc2efeb-ce04-449d-b799-c7482c64415f", "Chest4", "Object 10", "物体 10", "Chest4", "Chest4", "Chest4"},
            {"0ef70722-6c7b-40a4-b6c1-132f080b7a16", "Chest5", "Object 11", "物体 11", "Chest5", "Chest5", "Chest5"},
            {"10db555f-302a-432b-b9a4-19f65127cf94", "Switch（ボタン、ゴールド、緑）", "Object 200", "物体 200", "Switch（ボタン、ゴールド、緑）", "Switch(Button/Gold/Green)", "Switch(按键/金/绿)"},
            {"11107356-2ba0-4497-8662-64bbafe30245", "Sphere（黄、明）", "Object 64", "物体 64", "Sphere（黄、明）", "Sphere(Yellow/Light)", "Sphere(黄/明)"},
            {"11490256-9d5a-4b56-a5fd-a60e8dbd9fff", "篝火台（炎）", "Object 172", "物体 172", "篝火台（炎）", "Fire Pit(Fire)", "火炉(火)"},
            {"12ab00ac-5829-481f-87e5-6dd47509ac4e", "Weapon（斧、小、七色）", "Object 239", "物体 239", "Weapon（斧、小、七色）", "Weapon(Axe/Small/Rainbow)", "Weapon(斧头/小/七种颜色)"},
            {"13ce2b89-b2e6-4822-8fcc-4b51de65945f", "Flame（炎）", "Object 110", "物体 110", "Flame（炎）", "Flame(Fire)", "Flame(火)"},
            {"14721fdb-c260-4660-953a-33798f990246", "オーブ（青、明滅）", "Object 142", "物体 142", "オーブ（青、明滅）", "Orb(Blue/Blink)", "球(蓝/闪烁)"},
            {"14c2c1cd-3d30-4327-b6c3-843cf2f9cd25", "オーブ（緑、消）", "Object 145", "物体 145", "オーブ（緑、消）", "Orb(Green/Off)", "球(绿/消灭)"},
            {"15ba3ad7-5764-4ffb-963d-1d79c295ec84", "Crystal ball（赤、明）", "Object 31", "物体 31", "Crystal ball（赤、明）", "Crystal ball(Red/Light)", "Crystal ball(红/明)"},
            {"15cb6e29-d524-44e6-8a13-47446ec4f6bd", "足跡（小、右）", "Object 136", "物体 136", "足跡（小、右）", "Footprints(Small/Right)", "脚印(小/右)"},
            {"17686bcb-8f62-4684-a3be-9e66ca4c114f", "波（炎）", "Object 159", "物体 159", "波（炎）", "Wave(Fire)", "波(火)"},
            {"1832fe16-7a68-4996-ae5e-92ce109526a8", "Waterref（水面1）", "Object 262", "物体 262", "Sparkle（水面1）", "Sparkle(Water Surface1)", "Sparkle(水面1)"},
            {"190d0eff-8a90-49f1-b0ef-9f61c2bf9d8b", "Flame（ランプ）", "Object 104", "物体 104", "Flame（ランプ）", "Flame(Lamp)", "Flame(灯)"},
            {"194efce6-78e9-44d7-8b50-6fca55dcc1e0", "Door（鉄格子、錆、破損）", "Object 82", "物体 82", "Door（鉄格子、錆、破損）", "Door(Iron Bars/Rust/Damaged)", "Door(铁栏杆/锈/破损)"},
            {"1960f0b6-b503-45eb-9f13-c049fa01b05e", "オーブ（緑、明滅）", "Object 141", "物体 141", "オーブ（緑、明滅）", "Orb(Green/Blink)", "球(绿/闪烁)"},
            {"1bfee3ad-1059-4a54-9bec-cbc22b67efe1", "Door（格子、苔、左鍵）", "Object 89", "物体 89", "Door（格子、苔、左鍵）", "Door(Lattice/Moss/Left Key)", "Door(格子/苔/左键)"},
            {"1c3677e0-0741-4f59-ae66-e17c639ab51f", "オーブ（黄、破損）", "Object 148", "物体 148", "オーブ（黄、破損）", "Orb(Yellow/Damaged)", "球(黄/破损)"},
            {"1cfa0222-9f71-488f-b8f2-c6ccdc10aa1c", "Door（格子、破損）", "Object 86", "物体 86", "Door（格子、破損）", "Door(Lattice/Damaged)", "Door(格子/破损)"},
            {"1d95da28-4bb8-4870-b323-0ff104edc77f", "Gate3", "Object 03", "物体 03", "Gate3", "Gate3", "Gate3"},
            {"1dd21a8b-d674-4788-a9ff-a83f252c8bb2", "Chest1", "Object 07", "物体 07", "Chest1", "Chest1", "Chest1"},
            {"200c2b67-515b-4484-82b1-cac9626eea5a", "篝火台（薪）", "Object 176", "物体 176", "篝火台（薪）", "Fire Pit(Firewood)", "火炉(柴)"},
            {"20f80fc9-d0fd-49f2-8eab-ed2249ac129b", "Door17", "Object 55", "物体 55", "Door17", "Door17", "Door17"},
            {"20ffc469-f49c-42d0-9a5e-69a6a8c41e87", "Switch（レバー、鉄、赤）", "Object 178", "物体 178", "Switch（レバー、鉄、赤）", "Switch(Lever/Iron/Red)", "Switch(操作杆/铁/红)"},
            {"21c5ddd0-e0e8-4c89-8d7c-7192ab836582", "Weapon（剣、大、破壊）", "Object 231", "物体 231", "Weapon（剣、大、破壊）", "Weapon(Sword/Large/Broken)", "Weapon(剑/大/破坏)"},
            {"22126cfe-4144-452a-a3a4-8a2eefb89389", "Switch（ボタン、壁、ゴールド、緑）", "Object 224", "物体 224", "Switch（ボタン、壁、ゴールド、緑）", "Switch(Button/Wall/Gold/Green)", "Switch(按键/壁/金/绿)"},
            {"24b58fcd-7ebc-4b33-ab26-90e97203b6f6", "Switch（ボタン、壁、ブロンズ、赤）", "Object 218", "物体 218", "Switch（ボタン、壁、ブロンズ、赤）", "Switch(Button/Wall/Bronze/Red)", "Switch(按键/壁/铜/红)"},
            {"25615da2-d90a-419c-8cdd-1593989df53d", "波（入り口・小）", "Object 156", "物体 156", "波（入り口・小）", "Wave(Entry Small)", "波(入口 小)"},
            {"256bbcf8-70ee-47bb-9292-b5e993de9704", "BigMonster（食人植物）", "Object 253", "物体 253", "BigMonster（食人植物）", "LargeMonster(Cannibal Plant)", "BigMonster(食人植物)"},
            {"25ce9c21-4bea-4f9a-a76b-cfb2e47f4719", "Switch（レバー、壁、鉄、赤）", "Object 202", "物体 202", "Switch（レバー、壁、鉄、赤）", "Switch(Lever/Wall/Iron/Red)", "Switch(操作杆/壁/铁/红)"},
            {"2684621e-048a-4ace-91b3-ccb80d9fc43b", "オーブ（黄、明滅）", "Object 140", "物体 140", "オーブ（黄、明滅）", "Orb(Yellow/Blink)", "球(黄/闪烁)"},
            {"26abaf29-7445-415e-9368-cc39c7e9c1b0", "Switch（ボタン、壁、鉄、赤）", "Object 214", "物体 214", "Switch（ボタン、壁、鉄、赤）", "Switch(Button/Wall/Iron/Red)", "Switch(按键/壁/铁/红)"},
            {"27f2edae-f424-4bc7-ba09-3a7656b1cddf", "Switch（レバー、ブロンズ、赤）", "Object 182", "物体 182", "Switch（レバー、ブロンズ、赤）", "Switch(Lever/Bronze/Red)", "Switch(操作杆/铜/红)"},
            {"28887b51-0d6f-4f34-90a0-650bf1cc9003", "Door10", "Object 48", "物体 48", "Door10", "Door10", "Door10"},
            {"292bc9bc-b3d2-4214-ad6d-b80906a1a13a", "Flame（八芒星、緑）", "Object 114", "物体 114", "Flame（八芒星、緑）", "Flame(Octagram/Green)", "Flame(八角星/绿)"},
            {"2936c18c-d643-4c90-bedd-436d9477dbaa", "大玉（黒）", "Object 123", "物体 123", "大玉（黒）", "Large Ball(Black)", "大球(黒)"},
            {"2a499f23-6e6e-4c31-a960-6bd2f16389c0", "Door（魔方陣、青、破損）", "Object 98", "物体 98", "Door（魔方陣、青、破損）", "Door(Magic Circle/Blue/Damaged)", "Door(魔方阵/蓝/破损)"},
            {"2ac563af-3744-4b3e-b29c-8f960ab3c344", "Crystal ball（青、明）", "Object 34", "物体 34", "Crystal ball（青、明）", "Crystal ball(Blue/Light)", "Crystal ball(蓝/明)"},
            {"2b1b8094-330d-4eb7-8535-3b4da30c5331", "Crystal（緑、暗）", "Object 17", "物体 17", "Crystal（緑、暗）", "Crystal(Green/Dark)", "Crystal(绿/暗)"},
            {"2c0c294c-f3cc-45ae-aab9-f493858247e7", "Door13", "Object 51", "物体 51", "Door13", "Door13", "Door13"},
            {"2d51ccb6-9887-4ea4-ba3a-0e250a2b4cca", "Switch（レバー、壁、ブロンズ、緑）", "Object 208", "物体 208", "Switch（レバー、壁、ブロンズ、緑）", "Switch(Lever/Wall/Bronze/Green)", "Switch(操作杆/壁/铜/绿)"},
            {"2d6cb53b-5d61-400e-86a2-bef6464110a0", "Switch（ボタン、壁、ゴールド、青）", "Object 225", "物体 225", "Switch（ボタン、壁、ゴールド、青）", "Switch(Button/Wall/Gold/Blue)", "Switch(按键/壁/金/蓝)"},
            {"2eeef9a8-dfef-456c-bc66-7b3815f97193", "オーブ（青、破損）", "Object 150", "物体 150", "オーブ（青、破損）", "Orb(Blue/Damaged)", "球(蓝/破损)"},
            {"2ef9d52f-92f9-4646-b8c4-6253a1cbaf73", "Weapon（弓、大、七色）", "Object 242", "物体 242", "Weapon（弓、大、七色）", "Weapon(Bow/Large/Rainbow)", "Weapon(弓箭/大/七种颜色)"},
            {"30b5fcb1-1e9e-47f3-9a67-70f343febc37", "Weapon（剣、大、七色）", "Object 230", "物体 230", "Weapon（剣、大、七色）", "Weapon(Sword/Large/Rainbow)", "Weapon(剑/大/七种颜色)"},
            {"30e4c08c-9367-44f8-b18c-d78073005385", "Door（魔方陣、緑、破損）", "Object 97", "物体 97", "Door（魔方陣、緑、破損）", "Door(Magic Circle/Green/Damaged)", "Door(魔方阵/绿/破损)"},
            {"30f2f034-6697-49fe-80f6-825a9db901f2", "Door16", "Object 54", "物体 54", "Door16", "Door16", "Door16"},
            {"312dc3dc-9113-449a-b188-d498e981c1b6", "Add Tower（蜃気楼の塔）", "Add Tower（蜃気楼の塔）", "Add Tower（蜃気楼の塔）", "蜃気楼の塔", "Mirage Tower", "海市蜃楼之塔"},
            {"31d62d14-47b8-485a-8137-a903fd4b6280", "Door（鉄格子、苔、右鍵）", "Object 76", "物体 76", "Door（鉄格子、苔、右鍵）", "Door(Iron Bars/Moss/Right Key)", "Door(铁栏杆/苔/右键)"},
            {"321fcb5a-3dc3-4c91-996f-764fa09bfd08", "Door8", "Object 46", "物体 46", "Door8", "Door8", "Door8"},
            {"346b9dee-bbc9-4783-8fdb-e4695ea66710", "Door（鉄格子、苔、左鍵）", "Object 77", "物体 77", "Door（鉄格子、苔、左鍵）", "Door(Iron Bars/Moss/Left Key)", "Door(铁栏杆/苔/左键)"},
            {"34887d50-419c-458a-b657-d638d8ea9a2f", "Crystal（紫、明）", "Object 27", "物体 27", "Crystal（紫、明）", "Crystal(Purple/Light)", "Crystal(紫/明)"},
            {"368e7cd6-a1a5-4646-b192-01e79fbe7342", "足跡（大、上）", "Object 129", "物体 129", "足跡（大、上）", "Footprints(Large/Up)", "脚印(大/上)"},
            {"36e741f2-3fa4-4cf4-ad5a-71b794cd7411", "Flame（円、緑）", "Object 113", "物体 113", "Flame（円、緑）", "Flame(Round/Green)", "Flame(円/绿)"},
            {"37748f78-9f2c-42c8-b796-8b94fb8034ed", "Switch（レバー、壁、ブロンズ、赤）", "Object 206", "物体 206", "Switch（レバー、壁、ブロンズ、赤）", "Switch(Lever/Wall/Bronze/Red)", "Switch(操作杆/壁/铜/红)"},
            {"37b3bba0-f421-4e61-8839-0c288f9fa1b4", "Weapon（レイピア、小、破壊）", "Object 234", "物体 234", "Weapon（レイピア、小、破壊）", "Weapon(Rapier/Small/Broken)", "Weapon(西洋剑/小/破坏)"},
            {"39c9cbac-6638-49e9-8b8f-b81f7fb35a66", "Door15", "Object 53", "物体 53", "Door15", "Door15", "Door15"},
            {"3a5dcb8b-0046-47a3-a751-d8071bd08143", "Add Roof（屋根）", "Add Roof（屋根）", "Add Roof（屋根）", "屋根", "Roof", "屋顶"},
            {"3bb96ed4-4c71-43ce-b926-375c2723f9fd", "Door（格子、苔、右鍵）", "Object 88", "物体 88", "Door（格子、苔、右鍵）", "Door(Lattice/Moss/Right Key)", "Door(格子/苔/右键)"},
            {"3bd22424-abdf-456c-abd5-fee6c772c462", "罠（トゲ、動）", "Object 125", "物体 125", "罠（トゲ、動）", "Trap(Spike/Motion)", "诡计(刺/动)"},
            {"3bf817d1-44b0-4d1e-aba3-9d4b6fb257cd", "Gate5", "Object 05", "物体 05", "Gate5", "Gate5", "Gate5"},
            {"3d63bfdb-7cf0-4e1d-9e03-87326473e3ce", "Vehicle（帆船）", "Object 255", "物体 255", "Vehicle（帆船）", "Vehicle(Ship)", "Vehicle(帆船)"},
            {"3d92bd02-be6b-4039-a93b-58fbf7afa6ef", "Door23", "Object 61", "物体 61", "Door23", "Door23", "Door23"},
            {"3e2aa480-b648-4ea1-8fcc-ae82200b34a2", "Chest8", "Object 14", "物体 14", "Chest8", "Chest8", "Chest8"},
            {"3ef47a6b-6c41-4b9c-bd40-d31d053cdc61", "大玉（鉄）", "Object 121", "物体 121", "大玉（鉄）", "Large Ball(Iron)", "大球(铁)"},
            {"40b96ed3-54f5-4125-9ea6-324602c48fd4", "Crystal（白、暗）", "Object 26", "物体 26", "Crystal（白、暗）", "Crystal(White/Dark)", "Crystal(白/暗)"},
            {"4355cee3-179e-4eef-a6cb-6afdc54c9b03", "Door11", "Object 49", "物体 49", "Door11", "Door11", "Door11"},
            {"4366d08d-becc-4f2d-aa3b-5b7f9564bcf3", "Crystal（黄、暗）", "Object 16", "物体 16", "Crystal（黄、暗）", "Crystal(Yellow/Dark)", "Crystal(黄/暗)"},
            {"470abf93-bc2f-4f88-83a8-1806212180aa", "Switch（ボタン、壁、ブロンズ、緑）", "Object 220", "物体 220", "Switch（ボタン、壁、ブロンズ、緑）", "Switch(Button/Wall/Bronze/Green)", "Switch(按键/壁/铜/绿)"},
            {"47ca0e22-eeb3-443b-8999-3b66db4cdef2", "Crystal ball（緑、明）", "Object 33", "物体 33", "Crystal ball（緑、明）", "Crystal ball(Green/Light)", "Crystal ball(绿/明)"},
            {"4a145551-4214-4eef-97f5-9e5d51b00ac3", "Switch（レバー、ゴールド、赤）", "Object 186", "物体 186", "Switch（レバー、ゴールド、赤）", "Switch(Lever/Gold/Red)", "Switch(操作杆/金/红)"},
            {"4a68c3fb-4dc0-41ea-93b6-91baae8e836a", "Weapon（ロッド、小、七色）", "Object 248", "物体 248", "Weapon（ロッド、小、七色）", "Weapon(Rod/Small/Rainbow)", "Weapon(魔杖/小/七种颜色)"},
            {"4a98cb4a-16e4-4d97-8993-a6276636a13a", "Weapon（杖、小、輝き）", "Object 244", "物体 244", "Weapon（杖、小、輝き）", "Weapon(Staff/Small/Sparkle)", "Weapon(杖/小/辉煌)"},
            {"4aa43232-2c30-4bb1-bb39-f5471b3ea4ee", "罠（トゲ、鉄）", "Object 124", "物体 124", "罠（トゲ、鉄）", "Trap(Spike/Iron)", "诡计(刺/铁)"},
            {"4f29b4c7-91e9-4e80-af35-e87f0fd16841", "足跡（小、下）", "Object 135", "物体 135", "足跡（小、下）", "Footprints(Small/Down)", "脚印(小/下)"},
            {"510f4d1b-986d-444b-9d38-61454ffe898b", "Gate1", "Object 01", "物体 01", "Gate1", "Gate1", "Gate1"},
            {"51920d05-5040-4d30-8dec-222b948df21f", "Vehicle（トロッコ）", "Object 260", "物体 260", "Vehicle（トロッコ）", "Vehicle(Minecart)", "Vehicle(手推车)"},
            {"521a28f9-4946-4cce-b6c3-354c195dfcd9", "Add Town（黄砂の街）", "Add Town（黄砂の街）", "Add Town（黄砂の街）", "黄砂の町", "Desert Town", "黄沙之城"},
            {"53afbb50-bff7-40a7-8b5f-97f23d433c7c", "煙（白）", "Object 168", "物体 168", "煙（白）", "Smoke(White)", "烟(白)"},
            {"54f59c5d-4232-4f40-95ec-fc6bca0ac972", "Door（格子、苔）", "Object 87", "物体 87", "Door（格子、苔）", "Door(Lattice/Moss)", "Door(格子/苔)"},
            {"558e2e02-fee4-4124-854e-4de0fb585a2f", "Door2", "Object 40", "物体 40", "Door2", "Door2", "Door2"},
            {"56360614-4f61-4b9a-9101-076c76c71f10", "Crystal ball（青、暗）", "Object 38", "物体 38", "Crystal ball（青、暗）", "Crystal ball(Blue/Dark)", "Crystal ball(蓝/暗)"},
            {"56b28fe6-eec3-47c6-950e-83d5122dab4f", "Door（鉄格子、錆、左鍵）", "Object 81", "物体 81", "Door（鉄格子、錆、左鍵）", "Door(Iron Bars/Rust/Left Key)", "Door(铁栏杆/锈/左键)"},
            {"56d40424-2912-4914-9d97-96a20585ce01", "Door（鉄格子、錆）", "Object 79", "物体 79", "Door（鉄格子、錆）", "Door(Iron Bars/Rust)", "Door(铁栏杆/锈)"},
            {"5757ed88-ad76-4326-a07e-401b33b18551", "Weapon（剣、小、破壊）", "Object 228", "物体 228", "Weapon（剣、小、破壊）", "Weapon(Sword/Small/Broken)", "Weapon(剑/小/破坏)"},
            {"58421e97-328c-480a-a0cd-b568a5c2bd3c", "Add Bridge（橋）", "Add Bridge（橋）", "Add Bridge（橋）", "橋", "Bridge", "桥梁"},
            {"59030fd5-4f70-4822-acb2-81d85dbac551", "Sphere（青、明）", "Object 66", "物体 66", "Sphere（青、明）", "Sphere(Blue/Light)", "Sphere(蓝/明)"},
            {"5ad094c0-9053-472b-9e56-c8e3cdbbccbd", "Door（魔方陣、黄、破損）", "Object 96", "物体 96", "Door（魔方陣、黄、破損）", "Door(Magic Circle/Yellow/Damaged)", "Door(魔方阵/黄/破损)"},
            {"5b3481cf-6e39-4b9d-855c-851240e0894d", "足跡（小、上）", "Object 137", "物体 137", "足跡（小、上）", "Footprints(Small/Up)", "脚印(小/上)"},
            {"5b3bbfc3-79b4-4628-9281-9180b5861efc", "Door18", "Object 56", "物体 56", "Door18", "Door18", "Door18"},
            {"5bf2860e-766e-477b-83b3-42333db3058a", "Flame（蝋燭スタンド）2", "Object 105", "物体 105", "Flame（蝋燭スタンド）2", "Flame(Candle Stand)2", "Flame(蜡烛)2"},
            {"5c1b4812-84ae-4912-b209-7b596d08d0ee", "Switch（ボタン、壁、鉄、青）", "Object 217", "物体 217", "Switch（ボタン、壁、鉄、青）", "Switch(Button/Wall/Iron/Blue)", "Switch(按键/壁/铁/蓝)"},
            {"5e46fe1c-53fb-4cab-9456-29dc85e29126", "オーブ（赤、破損）", "Object 147", "物体 147", "オーブ（赤、破損）", "Orb(Red/Damaged)", "球(红/破损)"},
            {"5e52dc76-59c3-4594-a477-8b7ab88e6254", "Door（鉄格子、左鍵）", "Object 73", "物体 73", "Door（鉄格子、左鍵）", "Door(Iron Bars/Left Key)", "Door(铁栏杆/左键)"},
            {"5e6ce338-a18e-45d4-9851-f90154a42845", "Switch（ボタン、壁、ブロンズ、黄）", "Object 219", "物体 219", "Switch（ボタン、壁、ブロンズ、黄）", "Switch(Button/Wall/Bronze/Yellow)", "Switch(按键/壁/铜/黄)"},
            {"5f9314d7-8b5f-4fea-9564-72f1ac25e378", "波（入り口・中）", "Object 157", "物体 157", "波（入り口・中）", "Wave(Entry Medium)", "波(入口 中)"},
            {"615b5cd4-6fa3-40b2-b21f-cab96d9618e8", "Door3", "Object 41", "物体 41", "Door3", "Door3", "Door3"},
            {"625aa0d8-deb8-4e36-9742-d745fdc32134", "Gate4", "Object 04", "物体 04", "Gate4", "Gate4", "Gate4"},
            {"62dfb9fd-9414-47a3-85b3-2241ef5e3482", "Switch（レバー、ゴールド、緑）", "Object 188", "物体 188", "Switch（レバー、ゴールド、緑）", "Switch(Lever/Gold/Green)", "Switch(操作杆/金/绿)"},
            {"62f712d1-2bb3-4bcf-b3de-4e4a66fffd76", "篝火台（炎、青）", "Object 173", "物体 173", "篝火台（炎、青）", "Fire Pit(Fire/Blue)", "火炉(火/蓝)"},
            {"63a384ea-9dfb-4c9d-b196-90c1c0ea46c6", "オーブ（台座）", "Object 151", "物体 151", "オーブ（台座）", "Orb(Pedestal)", "球(座)"},
            {"6478b3da-0692-4028-852e-f6c7cc449c4f", "Flame（火の玉、赤）", "Object 111", "物体 111", "Flame（火の玉、赤）", "Flame(Fireball/Red)", "Flame(陨石/红)"},
            {"65098de5-0edb-46d1-a82e-521adae8a06b", "Sphere（赤、明）", "Object 63", "物体 63", "Sphere（赤、明）", "Sphere(Red/Light)", "Sphere(红/明)"},
            {"652d65b6-8e55-4cce-b4bb-f68750ea9e8e", "足跡（大、下）", "Object 127", "物体 127", "足跡（大、下）", "Footprints(Large/Down)", "脚印(大/下)"},
            {"654a8335-b47d-489d-9c6c-93175d878b65", "波（炎、小波）", "Object 162", "物体 162", "波（炎、小波）", "Wave(Fire/Wavelets)", "波(火/小波)"},
            {"654c54b7-8b84-473c-be8f-d1bcd5dd119f", "Weapon（剣、小、輝き）", "Object 226", "物体 226", "Weapon（剣、小、輝き）", "Weapon(Sword/Small/Sparkle)", "Weapon(剑/小/辉煌)"},
            {"674de871-5a58-494a-beac-565577716113", "Crystal（グレー、暗）", "Object 24", "物体 24", "Crystal（グレー、暗）", "Crystal(Gray/Dark)", "Crystal(灰色/暗)"},
            {"678caea5-f2d5-4456-9e72-ad37a9cb91b4", "Switch（ボタン、ブロンズ、青）", "Object 197", "物体 197", "Switch（ボタン、ブロンズ、青）", "Switch(Button/Bronze/Blue)", "Switch(按键/铜/蓝)"},
            {"67d7ae8a-7783-4b24-b43e-f17c2de8197f", "Door（格子、老朽、破損）", "Object 94", "物体 94", "Door（格子、老朽、破損）", "Door(Lattice/Old/Damaged)", "Door(格子/老朽/破损)"},
            {"6861e8d4-7d49-43d6-be4c-ed9ba0b94139", "Switch（ボタン、壁、ブロンズ、青）", "Object 221", "物体 221", "Switch（ボタン、壁、ブロンズ、青）", "Switch(Button/Wall/Bronze/Blue)", "Switch(按键/壁/铜/蓝)"},
            {"6a170bc7-3813-49ce-8585-b19321b66f3a", "Door（鉄格子、錆、右鍵）", "Object 80", "物体 80", "Door（鉄格子、錆、右鍵）", "Door(Iron Bars/Rust/Right Key)", "Door(铁栏杆/锈/右键)"},
            {"6b39b42c-4be3-4436-9aea-94ce8565d41d", "Door9", "Object 47", "物体 47", "Door9", "Door9", "Door9"},
            {"6b5b055f-2180-4ef9-acac-c32f8d27059f", "足跡（中、左）", "Object 134", "物体 134", "足跡（中、左）", "Footprints(Medium/Left)", "脚印(中/左)"},
            {"6c80973d-db08-44a0-b6fd-a2d1d8d8ce59", "足跡（大、左）", "Object 130", "物体 130", "足跡（大、左）", "Footprints(Large/Left)", "脚印(大/左)"},
            {"6c9ee38d-df43-4581-8530-831aee803b89", "Door14", "Object 52", "物体 52", "Door14", "Door14", "Door14"},
            {"6e10267d-8677-43b7-bf96-a79a79c6863a", "Door（格子、老朽、右鍵）", "Object 92", "物体 92", "Door（格子、老朽、右鍵）", "Door(Lattice/Old/Right Key)", "Door(格子/老朽/右键)"},
            {"6ff7d4df-d5e8-48cc-8c0f-d4a6d53e3601", "Door4", "Object 42", "物体 42", "Door4", "Door4", "Door4"},
            {"717f1129-5db2-4694-ae12-30759127938d", "Switch（レバー、壁、鉄、緑）", "Object 204", "物体 204", "Switch（レバー、壁、鉄、緑）", "Switch(Lever/Wall/Iron/Green)", "Switch(操作杆/壁/铁/绿)"},
            {"723929e4-3f17-4365-8b19-4c5705a482f4", "BigMonster（シャーク）", "Object 251", "物体 251", "BigMonster（シャーク）", "LargeMonster(Shark)", "BigMonster(鲨鱼)"},
            {"735f3498-d76d-4a9f-8fc6-d1d58900bb91", "Flame（蝋燭スタンド）1", "Object 102", "物体 102", "Flame（蝋燭スタンド）1", "Flame(Candle Stand)1", "Flame(蜡烛)1"},
            {"7373bb1b-ab7b-4a43-a194-f99a72440583", "波（炎、入り口・小）", "Object 160", "物体 160", "波（炎、入り口・小）", "Wave(Fire/Entry Small)", "波(火/入口 小)"},
            {"73ec0e3a-62c3-4c15-9f68-5a05b8fa6aef", "Flame（円、アニメ、緑）", "Object 118", "物体 118", "Flame（円、アニメ、緑）", "Flame(Round/Animation/Green)", "Flame(円/动画/绿)"},
            {"75bddd3b-4c07-41a0-94ea-ebe127ac9e41", "Switch（レバー、壁、鉄、青）", "Object 205", "物体 205", "Switch（レバー、壁、鉄、青）", "Switch(Lever/Wall/Iron/Blue)", "Switch(操作杆/壁/铁/蓝)"},
            {"75d075a7-83d1-42a1-90f6-eff15b38327c", "Door22", "Object 60", "物体 60", "Door22", "Door22", "Door22"},
            {"7788a9f0-b50c-4f4e-a5e9-e7f27fdf2821", "煙（金）", "Object 171", "物体 171", "煙（金）", "Smoke(Gold)", "烟(金)"},
            {"77b041a8-45c9-40c2-8628-18d1a6dc2960", "Door20", "Object 58", "物体 58", "Door20", "Door20", "Door20"},
            {"79691f42-4d98-4a68-ae99-53303eb8329c", "Flame（輪、アニメ、緑）", "Object 119", "物体 119", "Flame（輪、アニメ、緑）", "Flame(Circle/Animation/Green)", "Flame(圆形/动画/绿)"},
            {"7ac58e29-7af2-4596-90fb-2396f5bf7dab", "Crystal（赤、明）", "Object 19", "物体 19", "Crystal（赤、明）", "Crystal(Red/Light)", "Crystal(红/明)"},
            {"7b281a9e-3101-4218-a66a-1551e60c3475", "像（女神、金）", "Object 165", "物体 165", "像（女神、金）", "Statue(Goddess/Gold)", "雕像(女神/金)"},
            {"7baaf801-2b61-484f-8ec3-5459f59e8f31", "Waterref（水面3）", "Object 264", "物体 264", "Sparkle（水面3）", "Sparkle(Water Surface3)", "Sparkle(水面3)"},
            {"7bec4089-3d65-4d51-bdfc-566471d28784", "大玉（岩）", "Object 120", "物体 120", "大玉（岩）", "Large Ball(Rock)", "大球(岩)"},
            {"7d056f83-b4f7-480c-b8ea-4db3407f1d2c", "Switch（ボタン、ゴールド、黄）", "Object 199", "物体 199", "Switch（ボタン、ゴールド、黄）", "Switch(Button/Gold/Yellow)", "Switch(按键/金/黄)"},
            {"7e7889e0-c69e-45e0-b3e8-34b234ac21dd", "Vehicle（ユニコーン）", "Object 258", "物体 258", "Vehicle（ユニコーン）", "Vehicle(Unicorn)", "Vehicle(独角兽)"},
            {"7f0a0ce1-0d4a-4b1e-94cb-ca6441a730f6", "Chest6", "Object 12", "物体 12", "Chest6", "Chest6", "Chest6"},
            {"82a780f5-2a8e-4215-88f1-f9ac412b20ea", "篝火台（空）", "Object 175", "物体 175", "篝火台（空）", "Fire Pit(Empty)", "火炉(空)"},
            {"83364051-e62b-4269-9686-48cd8a4ae3be", "Door12", "Object 50", "物体 50", "Door12", "Door12", "Door12"},
            {"845e8034-79ed-4984-a09c-49450c9e05bf", "Weapon（剣、大、輝き）", "Object 229", "物体 229", "Weapon（剣、大、輝き）", "Weapon(Sword/Large/Sparkle)", "Weapon(剑/大/辉煌)"},
            {"8482818c-9f4c-4a3f-bb9b-0f8419bea710", "足跡（大、右）", "Object 128", "物体 128", "足跡（大、右）", "Footprints(Large/Right)", "脚印(大/右)"},
            {"85a43779-a010-4514-8edf-974a8b6734cb", "Door（鉄格子、右鍵）", "Object 72", "物体 72", "Door（鉄格子、右鍵）", "Door(Iron Bars/Right Key)", "Door(铁栏杆/右键)"},
            {"85c2e760-670a-4fec-a0aa-a97dbc2c73e1", "Weapon（レイピア、小、輝き）", "Object 232", "物体 232", "Weapon（レイピア、小、輝き）", "Weapon(Rapier/Small/Sparkle)", "Weapon(西洋剑/小/辉煌)"},
            {"85fdc4fa-079a-432a-98a8-6c093a51f6db", "Flame（ランプ、消）", "Object 107", "物体 107", "Flame（ランプ、消）", "Flame(Lamp/Off)", "Flame(灯/消)"},
            {"875bcfc4-eea6-4daa-ad46-068f05d28966", "Door（魔方陣、赤、破損）", "Object 95", "物体 95", "Door（魔方陣、赤、破損）", "Door(Magic Circle/Red/Damaged)", "Door(魔方阵/红/破损)"},
            {"8843ba25-c257-4834-9afb-2673d5ad00ae", "Door（格子、錆）", "Object 91", "物体 91", "Door（格子、錆）", "Door(Lattice/Rust)", "Door(格子/锈)"},
            {"899d37ab-bd34-4fde-9e8e-0936a3102922", "Crystal（グレー、明）", "Object 28", "物体 28", "Crystal（グレー、明）", "Crystal(Gray/Light)", "Crystal(灰色/明)"},
            {"89a29dc3-f4bc-4464-8ffe-5fbfea136db8", "Switch（レバー、壁、ゴールド、黄）", "Object 211", "物体 211", "Switch（レバー、壁、ゴールド、黄）", "Switch(Lever/Wall/Gold/Yellow)", "Switch(操作杆/壁/金/黄)"},
            {"8aa82ce2-ab63-44f0-9566-3b93bc8969f7", "Weapon（斧、小、輝き）", "Object 238", "物体 238", "Weapon（斧、小、輝き）", "Weapon(Axe/Small/Sparkle)", "Weapon(斧头/小/辉煌)"},
            {"8b59dd0c-8943-4334-8053-c61046ed9bb4", "Flame（焚き火）", "Object 109", "物体 109", "Flame（焚き火）", "Flame(Bonfire)", "Flame(明火)"},
            {"8b64718d-7ee5-4445-88f7-c5c0a25851b0", "Door（鉄格子、苔）", "Object 75", "物体 75", "Door（鉄格子、苔）", "Door(Iron Bars/Moss)", "Door(铁栏杆/苔)"},
            {"8c2b045a-3dd3-413b-91fd-c8c26204ca33", "Door19", "Object 57", "物体 57", "Door19", "Door19", "Door19"},
            {"8c82ec3f-34fe-429c-8195-7d457b7a9855", "Switch（ボタン、ブロンズ、緑）", "Object 196", "物体 196", "Switch（ボタン、ブロンズ、緑）", "Switch(Button/Bronze/Green)", "Switch(按键/铜/绿)"},
            {"8d8f513d-9b3b-47f3-b2e8-05b336bffbfa", "足跡（中、右）", "Object 132", "物体 132", "足跡（中、右）", "Footprints(Medium/Right)", "脚印(中/右)"},
            {"8daf6361-c86b-4294-a39a-43dad4a86bad", "オーブ（青、消）", "Object 146", "物体 146", "オーブ（青、消）", "Orb(Blue/Off)", "球(蓝/消灭)"},
            {"8dd49521-0a96-48f7-b5dc-e3b240654323", "Weapon（二刀、大、破壊）", "Object 237", "物体 237", "Weapon（二刀、大、破壊）", "Weapon(Dual Weapons/Large/Broken)", "Weapon(二刀/大/破坏)"},
            {"8ef3bd6a-0b28-40dd-8104-e13f482f87c4", "大玉（雪）", "Object 122", "物体 122", "大玉（雪）", "Large Ball(Snowball)", "大球(雪)"},
            {"8f5e2157-1d10-4d2f-b4a2-a19936637e01", "Add Barricade（バリケード縦）", "Add Barricade（バリケード縦）", "Add Barricade（バリケード縦）", "バリケード", "Barricade", "路障"},
            {"8fa3215d-f8ac-4dfa-9bd7-f52afa3359ff", "Flame（燭台、青）", "Object 100", "物体 100", "Flame（燭台、青）", "Flame(Sconce/Blue)", "Flame(蜡烛/蓝)"},
            {"90612f06-d277-4cb6-a219-0f832cedefb1", "Flame（蝋燭）", "Object 103", "物体 103", "Flame（蝋燭）", "Flame(Candle)", "Flame(烛)"},
            {"90e465f8-7ea7-47e8-974f-60bad29e59ad", "Sphere（青、暗）", "Object 70", "物体 70", "Sphere（青、暗）", "Sphere(Blue/Dark)", "Sphere(蓝/暗)"},
            {"919375df-a27e-4e0b-83de-5247ee708079", "BigMonster（トレント）", "Object 250", "物体 250", "BigMonster（トレント）", "LargeMonster(Treant)", "BigMonster(树精灵)"},
            {"92e3a04c-447e-4240-8332-2eb11ca7f772", "Switch（レバー、壁、ゴールド、緑）", "Object 212", "物体 212", "Switch（レバー、壁、ゴールド、緑）", "Switch(Lever/Wall/Gold/Green)", "Switch(操作杆/壁/金/绿)"},
            {"94aa0a99-504e-42bd-a1f2-ae94838ebd98", "Crystal ball（赤、暗）", "Object 35", "物体 35", "Crystal ball（赤、暗）", "Crystal ball(Red/Dark)", "Crystal ball(红/暗)"},
            {"97c69da6-c6f8-4e11-938c-d44638fbcac2", "Chest2", "Object 08", "物体 08", "Chest2", "Chest2", "Chest2"},
            {"98062f63-5068-4ff4-a17f-05527c686c1d", "Flame（灯）", "Object 101", "物体 101", "Flame（灯）", "Flame(Lantern)", "Flame(轻)"},
            {"98b93fda-cae0-4665-a959-ce0e54b2dc9d", "Weapon（弓、大、輝き）", "Object 241", "物体 241", "Weapon（弓、大、輝き）", "Weapon(Bow/Large/Sparkle)", "Weapon(弓箭/大/辉煌)"},
            {"995d132f-9469-45f0-adc5-f319162bfb9c", "Vehicle（ゴンドラ）", "Object 261", "物体 261", "Vehicle（ゴンドラ）", "Vehicle(Gondola)", "Vehicle(观览车)"},
            {"9a047b89-5ecd-49e4-8707-cd841bbe3cca", "Door（鉄格子、苔、破損）", "Object 78", "物体 78", "Door（鉄格子、苔、破損）", "Door(Iron Bars/Moss/Damaged)", "Door(铁栏杆/苔/破损)"},
            {"9b69dc9b-3364-40e4-8b6e-a2e890ec0034", "Weapon（ロッド、小、破壊）", "Object 249", "物体 249", "Weapon（ロッド、小、破壊）", "Weapon(Rod/Small/Broken)", "Weapon(魔杖/小/破坏)"},
            {"9cf29a5f-74ee-4ee0-b213-707e03f524db", "Switch（ボタン、ゴールド、赤）", "Object 198", "物体 198", "Switch（ボタン、ゴールド、赤）", "Switch(Button/Gold/Red)", "Switch(按键/金/红)"},
            {"9d1ed436-0d07-4222-8a90-f68319e84ee4", "BigMonster（ドラゴン）", "Object 252", "物体 252", "BigMonster（ドラゴン）", "LargeMonster(Dragon)", "BigMonster(龙)"},
            {"9e0e59fc-d98a-4621-a491-1c2630d144e4", "Gate6", "Object 06", "物体 06", "Gate6", "Gate6", "Gate6"},
            {"9efecf03-4603-4d37-b9a5-662c4036fe0b", "柱（溶岩）", "Object 167", "物体 167", "柱（溶岩）", "Pillar(Lava)", "柱(熔岩)"},
            {"9ff2d4a7-fe97-4dee-8c5e-0054b8239b9d", "Door（格子、老朽、左鍵）", "Object 93", "物体 93", "Door（格子、老朽、左鍵）", "Door(Lattice/Old/Left Key)", "Door(格子/老朽/左键)"},
            {"a1fd9835-bf8f-4d4f-bb2c-a3eb1d333cfe", "波（炎、入り口・中）", "Object 161", "物体 161", "波（炎、入り口・中）", "Wave(Fire/Entry Medium)", "波(火/入口 中)"},
            {"a2ebbf4d-01e8-4c89-97ae-4b728a86dedf", "Switch（ボタン、鉄、青）", "Object 193", "物体 193", "Switch（ボタン、鉄、青）", "Switch(Button/Iron/Blue)", "Switch(按键/铁/蓝)"},
            {"a494be80-93cc-46cc-95e9-46f04471d381", "Switch（レバー、壁、ゴールド、青）", "Object 213", "物体 213", "Switch（レバー、壁、ゴールド、青）", "Switch(Lever/Wall/Gold/Blue)", "Switch(操作杆/壁/金/蓝)"},
            {"a531bdea-4ed9-413c-890a-52ea869f4be3", "Weapon（二刀、大、輝き）", "Object 235", "物体 235", "Weapon（二刀、大、輝き）", "Weapon(Dual Weapons/Large/Sparkle)", "Weapon(二刀/大/辉煌)"},
            {"a5b628bc-e061-4c21-a514-eab44544b14d", "Crystal（黄、明）", "Object 20", "物体 20", "Crystal（黄、明）", "Crystal(Yellow/Light)", "Crystal(黄/明)"},
            {"a710c2db-6c72-4d84-9894-e2b57c97ee88", "Crystal（白、明）", "Object 30", "物体 30", "Crystal（白、明）", "Crystal(White/Light)", "Crystal(白/明)"},
            {"a82cfc4e-1542-4353-af6d-9dce05b2ec67", "Switch（レバー、鉄、緑）", "Object 180", "物体 180", "Switch（レバー、鉄、緑）", "Switch(Lever/Iron/Green)", "Switch(操作杆/铁/绿)"},
            {"a8e7fb43-4292-4a06-a62c-abcd6c122987", "足跡（中、下）", "Object 131", "物体 131", "足跡（中、下）", "Footprints(Medium/Down)", "脚印(中/下)"},
            {"a9766bcb-7dd5-4a8a-b5d6-631139cf5e98", "像（悪魔、女性）", "Object 166", "物体 166", "像（悪魔、女性）", "Statue(Demonic/Female)", "雕像(恶魔/女)"},
            {"a99df5e2-85bf-4130-9829-9e176971fa4e", "柱（火）", "Object 164", "物体 164", "柱（火）", "Pillar(Fire)", "柱(火)"},
            {"ab2d28cc-dbcf-439b-b85e-b82584def623", "Switch（レバー、ブロンズ、青）", "Object 185", "物体 185", "Switch（レバー、ブロンズ、青）", "Switch(Lever/Bronze/Blue)", "Switch(操作杆/铜/蓝)"},
            {"ac0145e4-9f24-4706-a910-cf30db711c7a", "Crystal（緑、明）", "Object 21", "物体 21", "Crystal（緑、明）", "Crystal(Green/Light)", "Crystal(绿/明)"},
            {"ac0e00b7-2863-42f1-845e-2ec3117fd241", "Switch（レバー、壁、ゴールド、赤）", "Object 210", "物体 210", "Switch（レバー、壁、ゴールド、赤）", "Switch(Lever/Wall/Gold/Red)", "Switch(操作杆/壁/金/红)"},
            {"ac1314f4-ba5d-4753-bbce-006e086a324c", "Switch（レバー、ブロンズ、黄）", "Object 183", "物体 183", "Switch（レバー、ブロンズ、黄）", "Switch(Lever/Bronze/Yellow)", "Switch(操作杆/铜/黄)"},
            {"ad69a4b1-6fa2-4b89-9b01-85aa67ca3719", "Switch（レバー、壁、ブロンズ、青）", "Object 209", "物体 209", "Switch（レバー、壁、ブロンズ、青）", "Switch(Lever/Wall/Bronze/Blue)", "Switch(操作杆/壁/铜/蓝)"},
            {"af0d7bc4-41eb-49a2-a8e4-94f159f9ce97", "Vehicle（亀）", "Object 257", "物体 257", "Vehicle（亀）", "Vehicle(Turtle)", "Vehicle(龟)"},
            {"af3260de-d219-4bf8-a4ed-ad816e6132de", "オーブ（赤、明滅）", "Object 139", "物体 139", "オーブ（赤、明滅）", "Orb(Red/Blink)", "球(红/闪烁)"},
            {"b037c930-1cf9-4884-aa66-f8e9d6a33226", "Crystal（赤、暗）", "Object 15", "物体 15", "Crystal（赤、暗）", "Crystal(Red/Dark)", "Crystal(红/暗)"},
            {"b108da64-b270-4690-9a26-20f9cd867724", "Flame（火の玉、紫）", "Object 112", "物体 112", "Flame（火の玉、紫）", "Flame(Fireball/Purple)", "Flame(陨石/紫)"},
            {"b225dbe6-2836-4887-8d33-83ccc6177d54", "柱（水）", "Object 163", "物体 163", "柱（水）", "Pillar(Water)", "柱(水)"},
            {"b2cadbf8-139f-40c5-b83f-dd71ff2405a1", "Waterref（水面2）", "Object 263", "物体 263", "Sparkle（水面2）", "Sparkle(Water Surface2)", "Sparkle(水面2)"},
            {"b3fdb044-8504-47d4-829b-c0c0e99c019f", "オーブ（毒）", "Object 154", "物体 154", "オーブ（毒）", "Orb(Poison)", "球(毒)"},
            {"b3fe9e09-0c85-4a76-9602-b3aa70cb16db", "Switch（レバー、鉄、黄）", "Object 179", "物体 179", "Switch（レバー、鉄、黄）", "Switch(Lever/Iron/Yellow)", "Switch(操作杆/铁/黄)"},
            {"b4022bca-6fc5-4a6a-bb8b-aafcc2fb68f4", "Door1", "Object 39", "物体 39", "Door1", "Door1", "Door1"},
            {"b46157b4-27f0-449f-8d11-650c87ecc51f", "Weapon（杖、小、七色）", "Object 245", "物体 245", "Weapon（杖、小、七色）", "Weapon(Staff/Small/Rainbow)", "Weapon(杖/小/七种颜色)"},
            {"b4936cd3-13b3-4d2d-8b24-99a719b8933e", "Sphere（黄、暗）", "Object 68", "物体 68", "Sphere（黄、暗）", "Sphere(Yellow/Dark)", "Sphere(黄/暗)"},
            {"b64bf4c3-711e-4f2c-869c-7d451dea8b46", "Weapon（杖、小、破壊）", "Object 246", "物体 246", "Weapon（杖、小、破壊）", "Weapon(Staff/Small/Broken)", "Weapon(杖/小/破坏)"},
            {"b6683eb9-26be-4a85-b9ec-24a078dfdd71", "Door（鉄格子、破損）", "Object 74", "物体 74", "Door（鉄格子、破損）", "Door(Iron Bars/Damaged)", "Door(铁栏杆/破损)"},
            {"b773d196-573a-449c-b827-ec57ab7642ad", "足跡（中、上）", "Object 133", "物体 133", "足跡（中、上）", "Footprints(Medium/Up)", "脚印(中/上)"},
            {"b7c79cbf-f05e-42bd-b606-63f78ba9b313", "波（小波）", "Object 158", "物体 158", "波（小波）", "Wave(Wavelets)", "波(小波)"},
            {"b7f93b35-19e3-40fa-a8bc-209a97cf3832", "オーブ（緑、破損）", "Object 149", "物体 149", "オーブ（緑、破損）", "Orb(Green/Damaged)", "球(绿/破损)"},
            {"b83bce9d-ae2f-4dda-8f6b-803e1dbeecc7", "Switch（ボタン、壁、鉄、緑）", "Object 216", "物体 216", "Switch（ボタン、壁、鉄、緑）", "Switch(Button/Wall/Iron/Green)", "Switch(按键/壁/铁/绿)"},
            {"ba36b4d7-477f-48f4-b657-5bff9a801c17", "Crystal（マリン、明）", "Object 29", "物体 29", "Crystal（マリン、明）", "Crystal(Marine/Light)", "Crystal(海洋/明)"},
            {"bba99e36-5ce6-495c-8345-e0fa3c4bbdf2", "Switch（レバー、鉄、青）", "Object 181", "物体 181", "Switch（レバー、鉄、青）", "Switch(Lever/Iron/Blue)", "Switch(操作杆/铁/蓝)"},
            {"be46d856-8853-4a52-9d80-1bb8fd204d4c", "Flame（十字、緑）", "Object 115", "物体 115", "Flame（十字、緑）", "Flame(Cross/Green)", "Flame(十字/绿)"},
            {"bf70556a-a42b-4e2a-9517-af8eab643cef", "オーブ（赤、消）", "Object 143", "物体 143", "オーブ（赤、消）", "Orb(Red/Off)", "球(红/消灭)"},
            {"c032f0e7-be6f-44f0-8b5e-6ccc16bf89f9", "篝火台（炭）", "Object 177", "物体 177", "篝火台（炭）", "Fire Pit(Coal)", "火炉(炭)"},
            {"c0db889c-ef6f-4b6b-9933-5e4e7348a03d", "Door21", "Object 59", "物体 59", "Door21", "Door21", "Door21"},
            {"c1a64b95-f994-4649-81ec-108ecd5466dc", "Switch（ボタン、壁、ゴールド、赤）", "Object 222", "物体 222", "Switch（ボタン、壁、ゴールド、赤）", "Switch(Button/Wall/Gold/Red)", "Switch(按键/壁/金/红)"},
            {"c1bae20d-ef2e-4929-8f0d-9763da21fcbd", "Vehicle（飛竜）", "Object 256", "物体 256", "Vehicle（飛竜）", "Vehicle(Dragon)", "Vehicle(飞龙)"},
            {"c3eecb71-5a78-46fa-a41b-08166a368860", "Crystal（紫、暗）", "Object 23", "物体 23", "Crystal（紫、暗）", "Crystal(Purple/Dark)", "Crystal(紫/暗)"},
            {"c48d6449-8c89-400f-9c1a-771f20839d0b", "Crystal（青、明）", "Object 22", "物体 22", "Crystal（青、明）", "Crystal(Blue/Light)", "Crystal(蓝/明)"},
            {"c53bf1b2-d9ae-4569-8436-3e7244476f78", "Weapon（二刀、大、七色）", "Object 236", "物体 236", "Weapon（二刀、大、七色）", "Weapon(Dual Weapons/Large/Rainbow)", "Weapon(二刀/大/七种颜色)"},
            {"c6700bb1-5482-48ec-807d-a734d7e844c3", "Weapon（斧、小、破壊）", "Object 240", "物体 240", "Weapon（斧、小、破壊）", "Weapon(Axe/Small/Broken)", "Weapon(斧头/小/破坏)"},
            {"c6bacd6a-fa17-4f75-a532-5f9bb47e32b9", "Door24", "Object 62", "物体 62", "Door24", "Door24", "Door24"},
            {"c7b93feb-c10c-4026-bf0e-9c2746b70cf6", "Flame（八芒星、アニメ、緑）", "Object 117", "物体 117", "Flame（八芒星、アニメ、緑）", "Flame(Octagram/Animation/Green)", "Flame(八角星/动画/绿)"},
            {"c89d943c-c5bc-422a-8b3a-fe7fb6607b5a", "Switch（レバー、ゴールド、青）", "Object 189", "物体 189", "Switch（レバー、ゴールド、青）", "Switch(Lever/Gold/Blue)", "Switch(操作杆/金/蓝)"},
            {"c89e392c-f9fd-4b42-a09a-33edef20bffb", "Door6", "Object 44", "物体 44", "Door6", "Door6", "Door6"},
            {"c984070e-f909-48b6-aceb-5160c4585492", "Crystal ball（黄、暗）", "Object 36", "物体 36", "Crystal ball（黄、暗）", "Crystal ball(Yellow/Dark)", "Crystal ball(黄/暗)"},
            {"cb0f09c5-8603-4c13-b3b2-1ea102fa5787", "Sphere（緑、明）", "Object 65", "物体 65", "Sphere（緑、明）", "Sphere(Green/Light)", "Sphere(绿/明)"},
            {"cb4d1596-60a3-44d6-94dd-b0b2f387a931", "Switch（ボタン、鉄、黄）", "Object 191", "物体 191", "Switch（ボタン、鉄、黄）", "Switch(Button/Iron/Yellow)", "Switch(按键/铁/黄)"},
            {"cb4f4d72-1409-489c-8c3b-766b3a970d79", "煙（青）", "Object 170", "物体 170", "煙（青）", "Smoke(Blue)", "烟(蓝)"},
            {"cc441c9a-9537-48d0-b44f-aa68f0e407a2", "Weapon（レイピア、小、七色）", "Object 233", "物体 233", "Weapon（レイピア、小、七色）", "Weapon(Rapier/Small/Rainbow)", "Weapon(西洋剑/小/七种颜色)"},
            {"cca3e664-2b34-4224-9dec-5f2ba2a89cd3", "オーブ（透明）", "Object 152", "物体 152", "オーブ（透明）", "Orb(Transparent)", "球(透明)"},
            {"cd251c40-f8f9-4130-8970-073a043afb83", "Door（格子、苔、破損）", "Object 90", "物体 90", "Door（格子、苔、破損）", "Door(Lattice/Moss/Damaged)", "Door(格子/苔/破损)"},
            {"cf1e1d4a-6fc1-4e0d-aa8c-e79d23d39193", "足跡（小、左）", "Object 138", "物体 138", "足跡（小、左）", "Footprints(Small/Left)", "脚印(小/左)"},
            {"cf6dd008-ab23-480a-9bbe-6cffdaff5e8c", "Switch（ボタン、壁、鉄、黄）", "Object 215", "物体 215", "Switch（ボタン、壁、鉄、黄）", "Switch(Button/Wall/Iron/Yellow)", "Switch(按键/壁/铁/黄)"},
            {"d160f6bb-97d8-43f2-a41b-2cf936cd22c6", "Door5", "Object 43", "物体 43", "Door5", "Door5", "Door5"},
            {"d4c11676-89a9-4c52-bf5e-43d869d8015c", "Crystal（青、暗）", "Object 18", "物体 18", "Crystal（青、暗）", "Crystal(Blue/Dark)", "Crystal(蓝/暗)"},
            {"d59b6460-ffcd-45d3-bfaa-20701cb4d5dd", "Switch（ボタン、ブロンズ、黄）", "Object 195", "物体 195", "Switch（ボタン、ブロンズ、黄）", "Switch(Button/Bronze/Yellow)", "Switch(按键/铜/黄)"},
            {"d8994985-87ef-4413-bdf0-bba7c2d3bfaf", "Switch（レバー、壁、ブロンズ、黄）", "Object 207", "物体 207", "Switch（レバー、壁、ブロンズ、黄）", "Switch(Lever/Wall/Bronze/Yellow)", "Switch(操作杆/壁/铜/黄)"},
            {"d9200020-672a-4f11-b1f9-ce327262470a", "Gate2", "Object 02", "物体 02", "Gate2", "Gate2", "Gate2"},
            {"d98eb7a8-b323-4670-88b4-4a677295b58d", "Crystal ball（黄、明）", "Object 32", "物体 32", "Crystal ball（黄、明）", "Crystal ball(Yellow/Light)", "Crystal ball(黄/明)"},
            {"daf8474e-74dd-4966-8a07-2a00055fbe83", "Switch（ボタン、壁、ゴールド、黄）", "Object 223", "物体 223", "Switch（ボタン、壁、ゴールド、黄）", "Switch(Button/Wall/Gold/Yellow)", "Switch(按键/壁/金/黄)"},
            {"db00da6c-37a9-48e4-91ec-cacdb67e0ad3", "オーブ（宙）", "Object 153", "物体 153", "オーブ（宙）", "Orb(Space)", "球(宇宙)"},
            {"dc8408c9-8cfb-414b-9efd-1183596bc046", "Crystal ball（緑、暗）", "Object 37", "物体 37", "Crystal ball（緑、暗）", "Crystal ball(Green/Dark)", "Crystal ball(绿/暗)"},
            {"e273e0a8-b393-4d11-af6c-ab9c32b33429", "煙（茶）", "Object 169", "物体 169", "煙（茶）", "Smoke(Brown)", "烟(茶)"},
            {"e47b489f-4d16-4cb2-a576-7b30f8e7c51d", "Chest3", "Object 09", "物体 09", "Chest3", "Chest3", "Chest3"},
            {"e83634b6-b233-4b2f-92f8-7200d1747444", "オーブ（黄、消）", "Object 144", "物体 144", "オーブ（黄、消）", "Orb(Yellow/Off)", "球(黄/消灭)"},
            {"e89fe762-a9a0-4430-87b0-df8c680fe9d8", "Crystal（マリン、暗）", "Object 25", "物体 25", "Crystal（マリン、暗）", "Crystal(Marine/Dark)", "Crystal(海洋/暗)"},
            {"e8db3cd4-7988-4342-9547-bf00f3abe6ea", "Chest7", "Object 13", "物体 13", "Chest7", "Chest7", "Chest7"},
            {"e9359134-1e12-4797-a9c0-e7226de584bc", "Switch（レバー、ブロンズ、緑）", "Object 184", "物体 184", "Switch（レバー、ブロンズ、緑）", "Switch(Lever/Bronze/Green)", "Switch(操作杆/铜/绿)"},
            {"e98773c9-6bcf-4687-880a-0589cc5c3d8c", "Vehicle（小船）", "Object 254", "物体 254", "Vehicle（小船）", "Vehicle(Boat)", "Vehicle(小船)"},
            {"eb374a11-bb66-4f6d-964b-21b94eccad21", "Switch（レバー、ゴールド、黄）", "Object 187", "物体 187", "Switch（レバー、ゴールド、黄）", "Switch(Lever/Gold/Yellow)", "Switch(操作杆/金/黄)"},
            {"ee16303b-be2a-4589-a5fc-6c3675213252", "Switch（ボタン、鉄、赤）", "Object 190", "物体 190", "Switch（ボタン、鉄、赤）", "Switch(Button/Iron/Red)", "Switch(按键/铁/红)"},
            {"f264e25d-4fd4-4c18-8f93-1c93b05e9596", "Add Wall（壁）", "Add Wall（壁）", "Add Wall（壁）", "壁", "Wall", "墙壁"},
            {"f298d47d-475c-41b8-a05d-7cd9e0709ca0", "Switch（レバー、壁、鉄、黄）", "Object 203", "物体 203", "Switch（レバー、壁、鉄、黄）", "Switch(Lever/Wall/Iron/Yellow)", "Switch(操作杆/壁/铁/黄)"},
            {"f2ea8e25-9e14-4865-9a0b-264cf600a518", "Switch（ボタン、ブロンズ、赤）", "Object 194", "物体 194", "Switch（ボタン、ブロンズ、赤）", "Switch(Button/Bronze/Red)", "Switch(按键/铜/红)"},
            {"f4ccad7b-7f55-4926-b7a5-386f089a3f91", "Flame（蝋燭、消）", "Object 106", "物体 106", "Flame（蝋燭、消）", "Flame(Candle/Off)", "Flame(烛/消)"},
            {"f5249cba-4362-4e2b-b994-f6a121104ece", "Door（格子、右鍵）", "Object 84", "物体 84", "Door（格子、右鍵）", "Door(Lattice/Right Key)", "Door(格子/右键)"},
            {"f589c3a1-f03e-4fe2-b7c7-95e5be145410", "Door（格子）", "Object 83", "物体 83", "Door（格子）", "Door(Lattice)", "Door(格子)"},
            {"f5c0c454-4dca-42fa-88fc-cf3b93ea79ec", "Flame（燭台）", "Object 99", "物体 99", "Flame（燭台）", "Flame(Sconce)", "Flame(蜡烛)"},
            {"f7693488-e743-4175-be39-ba173abe718f", "Flame（蝋燭スタンド、消）2", "Object 108", "物体 108", "Flame（蝋燭スタンド、消）2", "Flame(Candle Stand/Off)2", "Flame(蜡烛/消)2"},
            {"f7fe1d0c-b1a1-468d-a895-86fa62fc5968", "Door（格子、左鍵）", "Object 85", "物体 85", "Door（格子、左鍵）", "Door(Lattice/Left Key)", "Door(格子/左键)"},
            {"f8333c97-69c9-40aa-ba9f-9e25e8b3602f", "Sphere（赤、暗）", "Object 67", "物体 67", "Sphere（赤、暗）", "Sphere(Red/Dark)", "Sphere(红/暗)"},
            {"f8750b7d-e616-48a0-9085-1610e7b6050c", "Door7", "Object 45", "物体 45", "Door7", "Door7", "Door7"},
            {"fa5156d5-2fdd-4771-9ba9-1e121ceff6a5", "罠（トゲ、血塗り）", "Object 126", "物体 126", "罠（トゲ、血塗り）", "Trap(Spike/Bloody)", "诡计(刺/血痕)"},
            {"faee2aca-ff0a-430d-9574-9f4ab1648f9d", "Switch（ボタン、ゴールド、青）", "Object 201", "物体 201", "Switch（ボタン、ゴールド、青）", "Switch(Button/Gold/Blue)", "Switch(按键/金/蓝)"},
            {"fc8dcb16-be1c-4fcf-9d52-c8450290beed", "Weapon（ロッド、小、輝き）", "Object 247", "物体 247", "Weapon（ロッド、小、輝き）", "Weapon(Rod/Small/Sparkle)", "Weapon(魔杖/小/辉煌)"},
            {"fcd3593d-b2c1-4e61-9d67-c7d5b94a2abb", "Weapon（弓、大、破壊）", "Object 243", "物体 243", "Weapon（弓、大、破壊）", "Weapon(Bow/Large/Broken)", "Weapon(弓箭/大/破坏)"},
            {"fd09c3e0-21af-468e-b899-761cd7c2e6ef", "Door（鉄格子）", "Object 71", "物体 71", "Door（鉄格子）", "Door(Iron Bars)", "Door(铁栏杆)"},
            {"fe327ae3-13b0-4f3e-8d12-45c73063c607", "Switch（ボタン、鉄、緑）", "Object 192", "物体 192", "Switch（ボタン、鉄、緑）", "Switch(Button/Iron/Green)", "Switch(按键/铁/绿)"}
        };

        public string GetIdentifier()
        {
            return "Migration107Class";
        }

        public void Execute()
        {
            ObjectName_Convert();
        }

        public void Rollback()
        {
        }

        public bool IsStorageUpdate()
        {
            return false;
        }

        public List<string> ListStorageCopy()
        {
            return null;
        }

        public List<string> ListStorageDelete()
        {
            return null;
        }

        /// <summary>
        /// 素材管理 - オブジェクトの名称変更
        /// ユーザーが編集済みのオブジェクトは対象としない
        /// </summary>
        private void ObjectName_Convert() {
#if UNITY_EDITOR
            // Objectデータの取得
            DatabaseManagementService databaseManagementService = new DatabaseManagementService();
            List<AssetManageDataModel> objectAssets = databaseManagementService.LoadObjectAssets();

            // 現在の言語設定
            var assembly2 = typeof(EditorWindow).Assembly;
            var localizationDatabaseType2 = assembly2.GetType("UnityEditor.LocalizationDatabase");
            var currentEditorLanguageProperty2 = localizationDatabaseType2.GetProperty("currentEditorLanguage");
            var lang2 = (SystemLanguage) currentEditorLanguageProperty2.GetValue(null);

            // 中国語の場合
            if (lang2 == SystemLanguage.Chinese || lang2 == SystemLanguage.ChineseSimplified || lang2 == SystemLanguage.ChineseTraditional)
            {
                for (int i = 0; i < objectAssets.Count; i++)
                {
                    for (int j = 0; j < dataCount; j++)
                    {
                        if (objectAssets[i].id == objectNames[j, 0])
                        {
                            if (objectAssets[i].name == objectNames[j, 3])
                            {
                                objectAssets[i].name = objectNames[j, 6];
                                databaseManagementService.SaveAssetManage(objectAssets[i]);
                            }
                            break;
                        }
                    }
                }
            }
            // 中国語でも日本語でもない場合（英語）
            else if (lang2 != SystemLanguage.Japanese)
            {
                for (int i = 0; i < objectAssets.Count; i++)
                {
                    for (int j = 0; j < dataCount; j++)
                    {
                        if (objectAssets[i].id == objectNames[j, 0])
                        {
                            if (objectAssets[i].name == objectNames[j, 2])
                            {
                                objectAssets[i].name = objectNames[j, 5];
                                databaseManagementService.SaveAssetManage(objectAssets[i]);
                            }
                            break;
                        }
                    }
                }
            }
            // 日本語の場合
            else
            {
                for (int i = 0; i < objectAssets.Count; i++)
                {
                    for (int j = 0; j < dataCount; j++)
                    {
                        if (objectAssets[i].id == objectNames[j, 0])
                        {
                            if (objectAssets[i].name == objectNames[j, 1])
                            {
                                objectAssets[i].name = objectNames[j, 4];
                                databaseManagementService.SaveAssetManage(objectAssets[i]);
                            }
                            break;
                        }
                    }
                }
            }
#endif
        }
    }
}