using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal static class ConstData {
    public const int BlockLayer = 6;
    public const int BlockLayerMask = 1 << BlockLayer;
    public const int InteractableLayerMask = 1 << 7;
    public const int EnemyLayer = 8;
}
