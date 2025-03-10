﻿using EntityStates;
using EntityStates.Wisp1Monster;
using EntityStates.GreaterWispMonster;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NebbysWrath.VariantEntityStates.GreaterWisp
{
    public class ChargeGreaterEmbers : BaseState
    {
        public static float baseDuration;
        public static GameObject chargeEffectPrefab;
        private GameObject chargeEffectInstanceLeft;
        private GameObject chargeEffectInstanceRight;

        public static GameObject laserEffectPrefab;
        private GameObject laserEffectInstanceLeft;
        private LineRenderer laserEffectInstanceLineRendererLeft;
        private GameObject laserEffectInstanceRight;
        private LineRenderer laserEffectInstanceLineRendererRight;

        public static string attackString;
        private float duration;
        private float stopwatch;
        private uint soundID;

        public override void OnEnter()
        {
            ChargeCannons cg = new ChargeCannons();
            baseDuration = cg.baseDuration;
            attackString = ChargeEmbers.attackString;
            chargeEffectPrefab = ChargeEmbers.chargeEffectPrefab;
            laserEffectPrefab = ChargeEmbers.laserEffectPrefab;

            base.OnEnter();
            stopwatch = 0f;
            duration = baseDuration / attackSpeedStat;
            soundID = Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
            PlayAnimation("Gesture", "ChargeCannons", "ChargeCannons.playbackRate", duration);
            Transform modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform transform1 = component.FindChild("MuzzleLeft");
                    Transform transform2 = component.FindChild("MuzzleRight");
                    if ((bool)transform1)
                    {
                        if ((bool)chargeEffectPrefab)
                        {
                            chargeEffectInstanceLeft = Object.Instantiate(chargeEffectPrefab, transform.position, transform.rotation);
                            chargeEffectInstanceLeft.transform.parent = transform;
                            ScaleParticleSystemDuration component2 = chargeEffectInstanceLeft.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserEffectPrefab)
                        {
                            laserEffectInstanceLeft = Object.Instantiate(laserEffectPrefab, transform.position, transform.rotation);
                            laserEffectInstanceLeft.transform.parent = transform;
                            laserEffectInstanceLineRendererLeft = laserEffectInstanceLeft.GetComponent<LineRenderer>();
                        }
                    }
                    if ((bool)transform2)
                    {
                        if ((bool)chargeEffectPrefab)
                        {
                            chargeEffectInstanceRight = Object.Instantiate(chargeEffectPrefab, transform.position, transform.rotation);
                            chargeEffectInstanceRight.transform.parent = transform;
                            ScaleParticleSystemDuration component2 = chargeEffectInstanceRight.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserEffectPrefab)
                        {
                            laserEffectInstanceRight = Object.Instantiate(laserEffectPrefab, transform.position, transform.rotation);
                            laserEffectInstanceRight.transform.parent = transform;
                            laserEffectInstanceLineRendererRight = laserEffectInstanceRight.GetComponent<LineRenderer>();
                        }
                    }
                }
            }
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(duration);
            }
        }

        public override void OnExit()
        {
            PlayAnimation("Gesture", "Empty");
            base.OnExit();
            if ((bool)chargeEffectInstanceLeft )
            {
                EntityState.Destroy(chargeEffectInstanceLeft);
            }
            if ((bool)chargeEffectInstanceRight)
            {
                EntityState.Destroy(chargeEffectInstanceRight);
            }
            if ((bool)laserEffectInstanceLeft)
            {
                EntityState.Destroy(laserEffectInstanceLeft);
            }
            if ((bool)laserEffectInstanceRight)
            {
                EntityState.Destroy(laserEffectInstanceRight);
            }
        }

        public override void Update()
        {
            base.Update();
            float distance = 100f;
            Color startColor = new Color(1f, 1f, 1f, stopwatch / duration);
            Color clear = Color.clear;

            Ray leftAimRay = GetAimRay();
            Vector3 leftOrigin = leftAimRay.origin;
            Vector3 leftPoint = leftAimRay.GetPoint(distance);
            laserEffectInstanceLineRendererLeft.SetPosition(0, leftOrigin);
            laserEffectInstanceLineRendererLeft.SetPosition(1, leftPoint);
            laserEffectInstanceLineRendererLeft.startColor = startColor;
            laserEffectInstanceLineRendererLeft.endColor = clear;

            Ray rightAimRay = GetAimRay();
            Vector3 rightOrigin = rightAimRay.origin;
            Vector3 rightPoint = rightAimRay.GetPoint(distance);
            laserEffectInstanceLineRendererRight.SetPosition(0, rightOrigin);
            laserEffectInstanceLineRendererRight.SetPosition(1, rightPoint);
            laserEffectInstanceLineRendererRight.startColor = startColor;
            laserEffectInstanceLineRendererRight.endColor = clear;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= duration && base.isAuthority)
            {
                outer.SetNextState(new FireGreaterEmbers());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
