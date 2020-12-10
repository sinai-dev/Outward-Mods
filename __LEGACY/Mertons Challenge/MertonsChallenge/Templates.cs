using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MertonsChallenge
{
    public static class Templates
    {
        // scene template dictionary
        public static Dictionary<string, SceneTemplate> Scenes = new Dictionary<string, SceneTemplate>
        {
            {
                "Emercar",
                new SceneTemplate()
                {
                    ArenaName = "Enmerkar Forest",
                    InteractorPos = new Vector3(572.8f, -10.3f, 1175.2f),
                    PlayerSpawnPos = new Vector3(572.8f, -10.3f, 1175.2f),
                    ToDStart = -2f, // 10pm
                    EnemySpawns = new List<Vector3>
                    {
                        new Vector3(579.1f, -11.8f, 1122.3f),
                        new Vector3(532.5f, -12.9f, 1129.1f),
                        new Vector3(515.1f, -13.1f, 1164.7f),
                        new Vector3(533.2f, -12.6f, 1206.0f),
                        new Vector3(552.8f, -7.6f, 1218.3f),
                        new Vector3(597.3f, -14.9f, 1222.9f),
                        new Vector3(623.9f, -14.5f, 1192.8f),
                    },
                    BossTemplate = new BossTemplate
                    {
                        Name = "Merton",
                        Health = 600,
                        Equipment = new List<int>
                        {
                            3200030, // mertons set
                            3200031,
                            3200032,
                            2020070 // firepoker
                        },
                        weaponDamages = new DamageList(new DamageType[]
                        {
                            new DamageType(DamageType.Types.Fire, 25),
                            new DamageType(DamageType.Types.Ethereal, 25),
                        }),
                        WeaponType = Weapon.WeaponType.Sword_2H,
                        TwoHandType = Equipment.TwoHandedType.TwoHandedRight,
                        weaponImpact = 35,
                        impactRes = 50,
                        Resistances = new float[9]
                        {
                            25, 75, -25, 25, 25, 75, 0, 0, 0
                        },
                        DropTable = new DropTableTemplate
                        {
                            MinDrops = 4,
                            MaxDrops = 6,
                            DroppedItems = new List<int>
                            {
                                6300030, // gold ingot
                                4000090, // azure shrimp
                                6600210, // gold-lich mechanism
                                6000010, // firefly powder
                                6600110, // occult remains
                            },
                            ItemChances = new List<int>
                            {
                                30,
                                10,
                                10,
                                10,
                                20
                            },
                            ItemMinQtys = new List<int>
                            {
                                1,
                                1,
                                1,
                                2,
                                3
                            },
                            ItemMaxQtys = new List<int>
                            {
                                2,
                                5,
                                5,
                                4,
                                5
                            }
                        }
                    }
                }
            },
        };
    }

    public class SceneTemplate
    {
        public string ArenaName;                // just my name for internal usage
        public Vector3 InteractorPos;           // position for 'begin' interactor
        public Vector3 PlayerSpawnPos;           // player start pos
        public float ToDStart;                  // Time of Day start
        public List<Vector3> EnemySpawns;       // random enemy spawn positions
        public BossTemplate BossTemplate;
    }

    public class BossTemplate
    {
        public string Name;
        public float Health;
        public List<int> Equipment;
        public float[] Resistances;
        public float impactRes;
        public DamageList weaponDamages;
        public Weapon.WeaponType WeaponType;
        public Weapon.TwoHandedType TwoHandType;
        public float weaponImpact;
        public DropTableTemplate DropTable;
    }

    public class DropTableTemplate
    {
        public int MinDrops;
        public int MaxDrops;

        public List<int> DroppedItems;
        public List<int> ItemChances;
        public List<int> ItemMinQtys;
        public List<int> ItemMaxQtys;

    }
}
