﻿using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GunNut
{
    public class CompProperties_GunNut : CompProperties
    {
        public CompProperties_GunNut()
        {
            this.compClass = typeof(GN_ThingComp);
        }

        public List<Slot> Slots
        {
            get
            {
                return this.slots;
            }
        }

        public List<Slot> slots;

        //public List<string> attachments;

        public JobDef jobDef;



    }
    public class GN_ThingComp : ThingComp
    {
        public CompProperties_GunNut Props
        {
            get
            {

                return (CompProperties_GunNut)this.props;
            }

        }

        public List<Slot> SlotsProps
        {
            get
            {
                return Props.Slots;
            }
        }
        public List<Slot> Slots = new List<Slot>();

        public override void CompTick()
        {
            base.CompTick();
            if (Slots == null || Slots.Count == 0)
            {
                Slots = this.SlotsProps;
            }
        }

        public override void PostExposeData()

        {

            Scribe_Collections.Look<Slot>(ref Slots, "SlotsAttachment" + this.parent.ThingID, LookMode.Deep);

            /*foreach (var slot in Slots)
            {
                slot.ExposeData();
            }*/
        }


        private bool TryGiveWeaponRepairJobToPawn(Pawn pawn, Thing attachment)
        {
            if (attachment != null)
            {
                Job job = new Job(this.Props.jobDef, parent, attachment);
                job.count = 1;
                return pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

            }
            return false;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption fmo in base.CompFloatMenuOptions(selPawn))
            {
                yield return fmo;
            }

            foreach (var slot in this.Slots)
            {
                if (slot.attachment != null)
                {
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Slot for " + slot.weaponPart.ToString() + " is already taken.", null, MenuOptionPriority.Default, null, null, 0f, null, null), selPawn, this.parent, "ReservedBy");
                    continue;
                }
                List<Thing> AccessableAttachments = GetAvailableAttachment(selPawn, slot.weaponPart);
                if (AccessableAttachments.NullOrEmpty() == true)
                {
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Haven't found any attachments for " + slot.weaponPart.ToString() + ".", null, MenuOptionPriority.Default, null, null, 0f, null, null), selPawn, this.parent, "ReservedBy");
                    continue;
                }

                foreach (var attachment in AccessableAttachments)
                {
                    if (!selPawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Cannot reach.", null, MenuOptionPriority.Default, null, null, 0f, null, null), selPawn, this.parent, "ReservedBy");
                        continue;
                    }

                    Action hoverAction = delegate ()
                    {
                        //Thing availableTwinThing = this.GetAvailableTwinThing(selPawn);
                        //MoteMaker.MakeStaticMote(availableTwinThing.Position, this.parent.Map, ThingDefOf.Mote_FeedbackGoto, 1f);
                    };
                    Action giveAttachJob = delegate ()
                    {
                        this.TryGiveWeaponRepairJobToPawn(selPawn, attachment);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Install " + attachment.def.label, giveAttachJob, MenuOptionPriority.Default, hoverAction, null, 0f, null, null), selPawn, this.parent, "ReservedBy");
                }
            }
            yield break;
        }

        private List<Thing> GetAvailableAttachment(Pawn pawn, GN_ThingDefOf.WeaponPart part)
        {
            List<Thing> results = new List<Thing>();

            if (pawn == null || !pawn.Spawned || pawn.Downed || pawn.Map == null)
            {
                results = null;
            }
            else
            {
                foreach (var attachment in GN_AttachmentList.allAttachments)
                {
                    if (attachment.weaponPart == part)
                    {
                        foreach (Thing thing in this.parent.Map.listerThings.ThingsOfDef(attachment))
                        {
                            results.Add(thing);
                        }
                    }

                }
            }
            return results;
        }


    }
}
