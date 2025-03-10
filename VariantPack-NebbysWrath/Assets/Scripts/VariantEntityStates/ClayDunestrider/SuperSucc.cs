﻿using EntityStates;
using EntityStates.ClayBoss;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace NebbysWrath.VariantEntityStates.ClayDunestrider
{
    public class SuperSucc : BaseState
    {
        private enum SubState
        {
            Entry,
            Tethers
        }

        public static float duration = 30f;

        public static float maxTetherDistance = 80f;

        public static float tetherMulchDistance = 5f;

        public static float tetherMulchDamageScale = 2f;

        public static float tetherMulchTickIntervalScale = 0.5f;

        public static float damagePerSecond = 2f;

        public static float damageTickFrequency = 3f;

        public static float entryDuration = 1f;

        public static GameObject mulchEffectPrefab;

        public static string enterSoundString;

        public static string beginMulchSoundString;

        public static string stopMulchSoundString;

        private GameObject mulchEffect;

        private Transform muzzleTransform;

        private List<TarTetherController> tetherControllers;

        private float stopwatch;

        private uint soundID;

        private SubState subState;

        public override void OnEnter()
        {
            duration = Recover.duration * 2;
            maxTetherDistance = Recover.maxTetherDistance * 2;
            tetherMulchDistance = Recover.tetherMulchDistance;
            tetherMulchDamageScale = Recover.tetherMulchDamageScale;
            tetherMulchTickIntervalScale = Recover.tetherMulchTickIntervalScale;
            damagePerSecond = Recover.damagePerSecond;
            damageTickFrequency = Recover.damageTickFrequency;
            entryDuration = Recover.entryDuration;
            mulchEffectPrefab = Recover.mulchEffectPrefab;
            enterSoundString = Recover.enterSoundString;
            beginMulchSoundString = Recover.beginMulchSoundString;
            stopMulchSoundString = Recover.stopMulchSoundString;
            base.OnEnter();
            stopwatch = 0f;
            if ((bool)base.modelLocator)
            {
                ChildLocator component = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    muzzleTransform = component.FindChild("MuzzleMulch");
                }
            }
            subState = SubState.Entry;
            PlayCrossfade("Body", "PrepSiphon", "PrepSiphon.playbackRate", entryDuration, 0.1f);
            soundID = Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
        }

        private void FireTethers()
        {
            Vector3 position = muzzleTransform.position;
            float breakDistanceSqr = maxTetherDistance * maxTetherDistance;
            List<GameObject> list = new List<GameObject>();
            tetherControllers = new List<TarTetherController>();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = position;
            bullseyeSearch.maxDistanceFilter = maxTetherDistance;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.filterByLoS = true;
            bullseyeSearch.searchDirection = Vector3.up;
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(base.gameObject);
            List<HurtBox> list2 = bullseyeSearch.GetResults().ToList();
            Debug.Log(list2);
            for (int i = 0; i < list2.Count; i++)
            {
                GameObject gameObject = list2[i].healthComponent.gameObject;
                if ((bool)gameObject)
                {
                    list.Add(gameObject);
                }
            }
            float tickInterval = 1f / damageTickFrequency;
            float damageCoefficientPerTick = damagePerSecond / damageTickFrequency;
            float mulchDistanceSqr = tetherMulchDistance * tetherMulchDistance;
            GameObject original = Resources.Load<GameObject>("Prefabs/NetworkedObjects/TarTether");
            for (int j = 0; j < list.Count; j++)
            {
                GameObject obj = Object.Instantiate(original, position, Quaternion.identity);
                TarTetherController component = obj.GetComponent<TarTetherController>();
                component.NetworkownerRoot = base.gameObject;
                component.NetworktargetRoot = list[j];
                component.breakDistanceSqr = breakDistanceSqr;
                component.damageCoefficientPerTick = damageCoefficientPerTick;
                component.tickInterval = tickInterval;
                component.tickTimer = (float)j * 0.1f;
                component.mulchDistanceSqr = mulchDistanceSqr;
                component.mulchDamageScale = tetherMulchDamageScale;
                component.mulchTickIntervalScale = tetherMulchTickIntervalScale;
                tetherControllers.Add(component);
                NetworkServer.Spawn(obj);
            }
        }

        private void DestroyTethers()
        {
            if (tetherControllers == null)
            {
                return;
            }
            for (int num = tetherControllers.Count - 1; num >= 0; num--)
            {
                if ((bool)tetherControllers[num])
                {
                    EntityState.Destroy(tetherControllers[num].gameObject);
                }
            }
        }

        public override void OnExit()
        {
            DestroyTethers();
            if ((bool)mulchEffect)
            {
                EntityState.Destroy(mulchEffect);
            }
            Util.PlaySound(stopMulchSoundString, base.gameObject);
            if (NetworkServer.active && (bool)base.characterBody)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            }
            base.OnExit();
        }

        private static void RemoveDeadTethersFromList(List<TarTetherController> tethersList)
        {
            for (int num = tethersList.Count - 1; num >= 0; num--)
            {
                if (!tethersList[num])
                {
                    tethersList.RemoveAt(num);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (subState == SubState.Entry)
            {
                if (!(stopwatch >= entryDuration))
                {
                    return;
                }
                subState = SubState.Tethers;
                stopwatch = 0f;
                PlayAnimation("Body", "ChannelSiphon");
                Util.PlaySound(beginMulchSoundString, base.gameObject);
                if (!NetworkServer.active)
                {
                    return;
                }
                FireTethers();
                mulchEffect = Object.Instantiate(mulchEffectPrefab, muzzleTransform.position, Quaternion.identity);
                ChildLocator component = mulchEffect.gameObject.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform transform = component.FindChild("AreaIndicator");
                    if ((bool)transform)
                    {
                        transform.localScale = new Vector3(maxTetherDistance * 2f, maxTetherDistance * 2f, maxTetherDistance * 2f);
                    }
                }
                mulchEffect.transform.parent = muzzleTransform;
            }
            else if (subState == SubState.Tethers && NetworkServer.active)
            {
                RemoveDeadTethersFromList(tetherControllers);
                if ((stopwatch >= duration || tetherControllers.Count == 0) && base.isAuthority)
                {
                    outer.SetNextState(new RecoverExit());
                }
            }
        }
    }
}
