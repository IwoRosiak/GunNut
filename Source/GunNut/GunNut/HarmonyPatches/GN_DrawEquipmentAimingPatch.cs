﻿using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace GunNut
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming", new Type[] { typeof(Thing), typeof(Vector3), typeof(float) })]
    public static class GN_DrawEquipmentAimingPatch
    {
        [HarmonyPostfix]
        private static void PawnRendererPatch(Thing eq, Vector3 drawLoc, float aimAngle, PawnRenderer __instance)
        {
            if (eq.TryGetComp<GN_AttachmentComp>() != null)
            {
                var weapon = eq.TryGetComp<GN_AttachmentComp>();
                foreach (var attachment in weapon.AttachmentsOnWeapon)
                {
                    float num = aimAngle - 90f;
                    Mesh mesh;
                    if (aimAngle > 20f && aimAngle < 160f)
                    {
                        mesh = MeshPool.plane10;
                        num += eq.def.equippedAngleOffset;
                    }
                    else if (aimAngle > 200f && aimAngle < 340f)
                    {
                        mesh = MeshPool.plane10Flip;
                        num -= 180f;
                        num -= eq.def.equippedAngleOffset;
                    }
                    else
                    {
                        mesh = MeshPool.plane10;
                        num += eq.def.equippedAngleOffset;
                    }
                    num %= 360f;
                    CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
                    if (compEquippable != null)
                    {
                        Vector3 b;
                        float num2;
                        EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out b, out num2, aimAngle);
                        drawLoc += b;
                        num += num2;
                    }
                    Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
                    Material matSingle;
                    if (graphic_StackCount != null)
                    {
                        matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
                    }
                    else
                    {
                        matSingle = eq.Graphic.MatSingle;
                    }
                    drawLoc.y += 1;
                    Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), attachment.onWeaponGraphic.Graphic.MatSingle, 0);
                }
            }
        }
    }
}