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
                    if (roll > 0)
                    {
                        switch ((int)mobType)
                        {
                            case 0:
                            case 3:
                            case 256:
                            case 260:
                                AddVermin(mobType, roll/4);
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
                    
                }
            }
        }

        private static void AddVermin(MobileTypes mobType, int roll)
        {
            Debug.Log("[Horrible Hordes]AddVermin roll = " + roll.ToString());
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
            Debug.Log("[Horrible Hordes]AddOrcs roll = " + roll.ToString());
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
            Debug.Log("[Horrible Hordes] AddSkeletons roll = " + roll.ToString());
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
    }
}
