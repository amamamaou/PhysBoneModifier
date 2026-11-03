using System;
using UnityEngine;
using VRC.SDKBase;

namespace Amama.PhysBoneModifier
{
    [Serializable]
    public class InteractionSetting
    {
        public enum OverrideType
        {
            [InspectorName("継承／変更なし")]
            Unchanged,
            True,
            False,
            Other
        }

        public OverrideType Type = OverrideType.Unchanged;

        public bool AllowSelf = true;

        public bool AllowOthers = true;
    }

    [AddComponentMenu("Amama/PhysBone Modifier")]
    public class PhysBoneModifier : MonoBehaviour, IEditorOnly
    {
        public InteractionSetting AllowCollision = new();

        public InteractionSetting AllowGrabbing = new();

        public InteractionSetting AllowPosing = new();

        public GameObject[] ExcludeObjects;
    }
}
