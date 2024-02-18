// Project:         Horrible Hordes mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using System.Collections.Generic;

namespace HorribleHordesMod
{
    public class HorribleHordes : MonoBehaviour
    {
        private static Mod mod;

        private static bool dexDetected;
        private static int hordeAdjustSetting = 0;
        private static int hordeAdjust = 0;
        private static DaggerfallEntityBehaviour entityBehaviour;
        private static EnemyEntity enemyEntity;
        private static EnemySenses enemySenses;
        private static EnemyMotor enemyMotor;
        private static List<Vector3> npcBillboardPos;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<HorribleHordes>();

            PlayerEnterExit.OnTransitionDungeonInterior += CheckOnDungeonEntrance;

            ModSettings settings = mod.GetSettings();

            hordeAdjustSetting = settings.GetInt("Difficulty", "HordeAdjustment");
            Debug.Log("[Horrible Hordes] hordeAdjustSetting = " + hordeAdjustSetting.ToString());

            Mod dex = ModManager.Instance.GetModFromGUID("76557441-7025-402e-a145-e3e1a28a093d");
            if (dex != null)
            {
                Debug.Log("[Horrible Hordes] DEX is active");
                dexDetected = true;
            }
            else
                Debug.Log("[Horrible Hordes] DEX is not detected");


            mod.IsReady = true;
        }

        private void Start()
        {
            
        }

        private static void CheckOnDungeonEntrance(PlayerEnterExit.TransitionEventArgs args)
        {
            switch(hordeAdjustSetting)
            {
                case 0:
                    hordeAdjust = 10;
                    break;
                case 1:
                    hordeAdjust = 0;
                    break;
                case 2:
                    hordeAdjust = Mathf.Max(0, 12 - GameManager.Instance.PlayerEntity.Level);
                    break;
                case 3:
                    hordeAdjust = - 10;
                    break;
            }
             
            npcBillboardPos = new List<Vector3>();

            Billboard[] billboards = (Billboard[])FindObjectsOfType(typeof(Billboard));
            foreach (Billboard billb in billboards)
            {
                if (NPCbillCheck(billb.name))
                {
                    Debug.Log(billb.name + "added to npcBillboardPos");
                    npcBillboardPos.Add(billb.transform.position);
                }
            }

            DaggerfallEntityBehaviour[] entityBehaviours = FindObjectsOfType<DaggerfallEntityBehaviour>();
            for (int i = 0; i < entityBehaviours.Length; i++)
            {
                entityBehaviour = entityBehaviours[i];
                if (entityBehaviour.EntityType == EntityTypes.EnemyMonster)
                {
                    enemyEntity = entityBehaviour.Entity as EnemyEntity;
                    enemySenses = entityBehaviour.GetComponent<EnemySenses>();
                    enemyMotor = entityBehaviour.GetComponent<EnemyMotor>();
                    MobileTypes mobType = (MobileTypes)enemyEntity.MobileEnemy.ID;
                    int roll = UnityEngine.Random.Range(0, 20 - hordeAdjust);
                    if (roll > 0 && !npcBillboardClose(entityBehaviour.transform.position))
                    {
                        switch ((int)mobType)
                        {
                            case 0:
                            case 3:
                            case 256:
                            case 260:
                                AddVermin(mobType, roll / 4);
                                break;
                            case 12:
                            case 21:
                            case 24:
                                AddOrcs(mobType, roll);
                                break;
                            case 32:
                            case 33:
                                AddSkeletons(mobType, roll);
                                break;
                        }
                    }
                    else if (npcBillboardClose(entityBehaviour.transform.position))
                        Debug.Log("npc close for " + enemyEntity.Name);
                    
                }
            }
        }

        private static void AddVermin(MobileTypes mobType, int roll)
        {
            roll -= (GameManager.Instance.PlayerEntity.Stats.LiveLuck / 10) - 5;
            roll = Mathf.Max(0, roll);
            Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * 0.1f;
            GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, (MobileTypes)enemyEntity.MobileEnemy.ID, roll);
            roll = mobiles.Length;
            while (roll > 0)
            {
                roll -= 1;
                mobiles[roll].SetActive(true);
            }
        }

        private static void AddOrcs(MobileTypes mobType, int roll)
        {
            roll -= (GameManager.Instance.PlayerEntity.Stats.LiveLuck / 10) - 5;
            roll = Mathf.Max(0, roll);
            if (mobType == MobileTypes.OrcWarlord)
                roll += 5;
            else if (mobType == MobileTypes.OrcSergeant)
                roll -= 5;
            if (roll >= 20)
            {
                roll -= 8;
                Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * -0.1f;
                GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, MobileTypes.OrcShaman, 1);
                mobiles[0].SetActive(true);
            }
            if (roll >= 13)
            {
                roll -= 5;
                Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * 0.2f;
                GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, MobileTypes.OrcSergeant, 1);
                mobiles[0].SetActive(true);
            }
            if (roll <= 2 && dexDetected)
            {
                roll -= 1;
                Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * -0.2f;
                GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, (MobileTypes)256, 1);
                mobiles[0].SetActive(true);
            }
            if (roll > 0)
            {
                roll = Mathf.Min(8, roll);
                Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * 0.1f;
                GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, MobileTypes.Orc, roll);
                roll = mobiles.Length;
                while (roll > 0)
                {
                    roll -= 1;
                    mobiles[roll].SetActive(true);
                }
            }
        }

        private static void AddSkeletons(MobileTypes mobType, int roll)
        {
            if (mobType == MobileTypes.AncientLich)
                roll += UnityEngine.Random.Range(1, 5);
            roll -= (GameManager.Instance.PlayerEntity.Stats.LiveLuck / 10) - 5;
            roll = Mathf.Max(0, roll);
            if (roll > 20 && dexDetected)
            {
                roll -= 10;
                Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * -0.1f;
                GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, (MobileTypes)388, 1);
                mobiles[0].SetActive(true);
            }
            if (roll >= 4 && dexDetected)
            {
                roll = Mathf.Min(8, roll);
                int enemyNumber = UnityEngine.Random.Range(0, roll);
                Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * 0.2f;
                GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, (MobileTypes)266, enemyNumber);
                while (enemyNumber > 0)
                {
                    enemyNumber -= 1;
                    mobiles[enemyNumber].SetActive(true);
                }
            }
            if (roll > 0)
            {
                roll = Mathf.Min(8, roll);
                Vector3 newEnemyPos = enemyMotor.transform.position + enemyMotor.transform.forward * 0.1f;
                GameObject[] mobiles = GameObjectHelper.CreateFoeGameObjects(newEnemyPos, MobileTypes.SkeletalWarrior, roll);
                roll = mobiles.Length;
                while (roll > 0)
                {
                    roll -= 1;
                    mobiles[roll].SetActive(true);
                }
            }
        }

        private static bool NPCbillCheck(string billName)
        {
            if (billName.StartsWith("DaggerfallBillboard [TEXTURE.175,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.176,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.177,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.178,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.179,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.180,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.181,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.182,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.183,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.184,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.185,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.186,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.195,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.197,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.334,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.346,") ||
                billName.StartsWith("DaggerfallBillboard [TEXTURE.357,"))
            {
                Debug.Log(billName + "= true");
                return true;
            }
            else
            {
                return false;
            }

        }

        private static bool npcBillboardClose(Vector3 enemyPos)
        {
            bool close = false;
            foreach (Vector3 billPos in npcBillboardPos)
            {
                if (Vector3.Distance(billPos, enemyPos) < 10)
                    close = true;
            }
            
            return close;
        }
    }
}
