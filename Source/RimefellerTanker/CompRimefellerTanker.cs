using System;
using System.Collections.Generic;
using System.Text;
using Multiplayer.API;
using Rimefeller;
using UnityEngine;
using Verse;

namespace RimefellerTanker
{
    public class CompRimefellerTanker : ThingComp
    {
        private CompPipe compPipe;
        private MapComponent_Rimefeller mapComp;
        public bool drawOverlay = false;

        // Gizmos
        private Command_Action gizmoDebugFill;
        private Command_Action gizmoDebugEmpty;
        private Command_Toggle gizmoToggleDrain;
        private Command_Toggle gizmoToggleFill;

        // Exposed fields
        private double storedAmount = 0;
        private bool isDraining = false;
        private bool isFilling = false;

        public double CapPercent => (storedAmount / Props.storageCap);
        private CompPipe CompPipe => compPipe ??= parent.GetComp<CompPipe>();
        private MapComponent_Rimefeller MapComp => mapComp ??= Find.CurrentMap.GetComponent<MapComponent_Rimefeller>();
        private CompProperties_RimefellerTanker Props => (CompProperties_RimefellerTanker)props;
        private Command_Action GizmoDebugFill => gizmoDebugFill ??= new Command_Action
        {
            action = DebugFill,
            defaultLabel = "Dev: Fill",
        };
        private Command_Action GizmoDebugEmpty => gizmoDebugEmpty ??= new Command_Action
        {
            action = DebugEmpty,
            defaultLabel = "Dev: Empty",
        };
        private Command_Toggle GizmoToggleDrain => gizmoToggleDrain ??= new Command_Toggle
        {
            isActive = () => isDraining,
            toggleAction = ToggleDrain,
            defaultLabel = "RimefellerTankerToggleDrain".Translate(),
            defaultDesc = "RimefellerTankerToggleDrainDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("RimefellerTanker/UI/Drain"),
        };
        private Command_Toggle GizmoToggleFill => gizmoToggleFill ??= new Command_Toggle
        {
            isActive = () => isFilling,
            toggleAction = ToggleFill,
            defaultLabel = "RimefellerTankerToggleFill".Translate(),
            defaultDesc = "RimefellerTankerToggleFillDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("RimefellerTanker/UI/Fill"),
        };

        [SyncMethod]
        private void ToggleFill()
        {
            isDraining = false;
            isFilling = !isFilling;
        }

        [SyncMethod]
        private void ToggleDrain()
        {
            isFilling = false;
            isDraining = !isDraining;
        }

        [SyncMethod(debugOnly = true)]
        private void DebugFill() => storedAmount = Props.storageCap;

        [SyncMethod(debugOnly = true)]
        private void DebugEmpty() => storedAmount = 0;

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            MapComp.MarkTowersForDraw = true;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            compPipe = null;
            mapComp = null;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (CompPipe == null) return;

            if (isDraining)
            {
                if (storedAmount <= 0)
                {
                    isDraining = false;
                    return;
                }

                var num = Math.Min(storedAmount, Props.drainAmount);
                if (num > 0)
                {
                    storedAmount -= num;
                    storedAmount += Props.contents switch
                    {
                        TankClass.Fuel => CompPipe.pipeNet.PushFuel((float)num),
                        TankClass.Oil => CompPipe.pipeNet.PushCrude(num),
                        _ => num,
                    };
                }
            }
            else if (isFilling)
            {
                if (storedAmount >= Props.storageCap)
                {
                    isFilling = false;
                    return;
                }

                var num = Math.Min(Props.storageCap - storedAmount, Props.fillAmount);
                num = Math.Max(num, 0);

                var success = Props.contents switch
                {
                    TankClass.Fuel => CompPipe.pipeNet.PullFuel(num),
                    TankClass.Oil => CompPipe.pipeNet.PullOil(num),
                    _ => false,
                };

                if (success)
                    storedAmount += num;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            yield return GizmoToggleDrain;
            yield return GizmoToggleFill;

            if (Prefs.DevMode)
            {
                yield return GizmoDebugEmpty;
                yield return GizmoDebugFill;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref storedAmount, nameof(storedAmount), 0);
            Scribe_Values.Look(ref isDraining, nameof(isDraining), false);
            Scribe_Values.Look(ref isFilling, nameof(isFilling), false);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!drawOverlay) return;

            var r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + Vector3.up * 0.1f;
            r.size = CompStorageTank.BarSize;
            r.fillPercent = (float)CapPercent;
            r.filledMat = CompStorageTank.WaterBarFilledMat;
            r.unfilledMat = CompStorageTank.BarUnfilledMat;
            r.margin = 0.15f;
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);

            drawOverlay = false;
        }

        public override string CompInspectStringExtra()
        {
            if (!parent.Spawned) return string.Empty;

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(Props.contents == TankClass.Fuel
                ? "FuelStorage".Translate(storedAmount.ToString("0.0"), Props.storageCap)
                : "OilStorage".Translate(storedAmount.ToString("0.0"), Props.storageCap));

            if (isFilling)
                stringBuilder.AppendLine("RimefellerTankerFillingInspect".Translate());
            else if (isDraining)
                stringBuilder.AppendLine("RimefellerTankerDrainingInspect".Translate());

            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
