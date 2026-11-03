using System;
using System.Collections.Generic;
using System.Linq;
using Amama.PhysBoneModifier.Editor;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using static VRC.Dynamics.VRCPhysBoneBase;

[assembly: ExportsPlugin(typeof(PhysBoneModifierPlugin))]

namespace Amama.PhysBoneModifier.Editor
{
    public class PhysBoneModifierPlugin : Plugin<PhysBoneModifierPlugin>
    {
        public override string QualifiedName => "me.sachino-toto.physbone-modifier";
        public override string DisplayName => "PhysBone Modifier";

        protected override void Configure()
        {
            // 他ツールの Generating フェーズで生成される PhysBone は変更しないようにする
            InPhase(BuildPhase.Resolving).Run("Apply PhysBone Settings", context =>
            {
                GameObject root = context.AvatarRootObject;

                // 親から子の順に適用し、子が明示した設定がある場合はそれを優先する
                PhysBoneModifier[] modifiers = root.GetComponentsInChildren<PhysBoneModifier>(true)
                    .OrderBy(modifier => GetHierarchyDepth(modifier.transform))
                    .ToArray();

                foreach (PhysBoneModifier modifier in modifiers)
                {
                    if (!modifier.enabled)
                    {
                        continue;
                    }

                    // null な要素を取り除く
                    GameObject[] excludeObjects = (modifier.ExcludeObjects ?? Array.Empty<GameObject>())
                        .Where(excludedObject => excludedObject != null)
                        .ToArray();

                    // 対象の PhysBone オブジェクト
                    IEnumerable<VRCPhysBone> physBones = modifier.GetComponentsInChildren<VRCPhysBone>(true)
                        .Where(physBone =>
                        {
                            // 対象外のオブジェクトかどうか確認
                            bool isExcludedByObject = excludeObjects.Any(excludedObject =>
                                physBone.gameObject == excludedObject || physBone.transform.IsChildOf(excludedObject.transform));
                            return !isExcludedByObject;
                        });

                    foreach (VRCPhysBone physBone in physBones)
                    {
                        // Allow Collision
                        ApplyInteraction(modifier.AllowCollision, ref physBone.allowCollision,
                            (self, others) =>
                            {
                                physBone.collisionFilter.allowSelf = self;
                                physBone.collisionFilter.allowOthers = others;
                            });

                        // Allow Grabbing
                        ApplyInteraction(modifier.AllowGrabbing, ref physBone.allowGrabbing,
                            (self, others) =>
                            {
                                physBone.grabFilter.allowSelf = self;
                                physBone.grabFilter.allowOthers = others;
                            });

                        // Allow Posing
                        ApplyInteraction(modifier.AllowPosing, ref physBone.allowPosing,
                            (self, others) =>
                            {
                                physBone.poseFilter.allowSelf = self;
                                physBone.poseFilter.allowOthers = others;
                            });
                    }
                }

                // AAO の最適化前に削除する
                foreach (PhysBoneModifier modifier in modifiers)
                {
                    UnityEngine.Object.DestroyImmediate(modifier);
                }
            });
        }

        private static int GetHierarchyDepth(Transform transform)
        {
            int depth = 0;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                depth++;
            }

            return depth;
        }

        // PhysBone の各項目に値を設定する
        private static void ApplyInteraction(
            InteractionSetting setting,
            ref AdvancedBool target,
            Action<bool, bool> applyFilter)
        {
            if (setting == null)
            {
                return;
            }

            switch (setting.Type)
            {
                case InteractionSetting.OverrideType.True:
                    target = AdvancedBool.True;
                    break;
                case InteractionSetting.OverrideType.False:
                    target = AdvancedBool.False;
                    break;
                case InteractionSetting.OverrideType.Other:
                    target = AdvancedBool.Other;
                    applyFilter?.Invoke(setting.AllowSelf, setting.AllowOthers);
                    break;
            }
        }
    }
}
