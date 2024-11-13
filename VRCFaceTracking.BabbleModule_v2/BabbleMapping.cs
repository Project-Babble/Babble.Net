using VRCFaceTracking.Core.Params.Expressions;
using BabbleExpression = Babble.Core.Enums.UnifiedExpression;

namespace VRCFaceTracking.BabbleModule_v2;

internal static class BabbleMapping
{
    /// <summary>
    /// 
    /// </summary>
    public static Dictionary<BabbleExpression, UnifiedExpressions> Mapping = new()
    {
        { BabbleExpression.CheekPuffLeft, UnifiedExpressions.CheekPuffLeft },
        { BabbleExpression.CheekPuffRight, UnifiedExpressions.CheekPuffRight },
        { BabbleExpression.CheekSuckLeft, UnifiedExpressions.CheekSuckLeft },
        { BabbleExpression.CheekSuckRight, UnifiedExpressions.CheekSuckRight },
        { BabbleExpression.JawOpen, UnifiedExpressions.JawOpen },
        { BabbleExpression.JawForward, UnifiedExpressions.JawForward },
        { BabbleExpression.JawLeft, UnifiedExpressions.JawLeft },
        { BabbleExpression.JawRight, UnifiedExpressions.JawRight },
        { BabbleExpression.NoseSneerLeft, UnifiedExpressions.NoseSneerLeft },
        { BabbleExpression.NoseSneerRight, UnifiedExpressions.NoseSneerRight },
        { BabbleExpression.LipFunnelLowerLeft, UnifiedExpressions.LipFunnelLowerLeft },
        { BabbleExpression.LipFunnelLowerRight, UnifiedExpressions.LipFunnelLowerRight },
        { BabbleExpression.LipFunnelUpperLeft, UnifiedExpressions.LipFunnelUpperLeft },
        { BabbleExpression.LipFunnelUpperRight, UnifiedExpressions.LipFunnelUpperRight },
        { BabbleExpression.LipPuckerLowerLeft, UnifiedExpressions.LipPuckerLowerLeft },
        { BabbleExpression.LipPuckerLowerRight, UnifiedExpressions.LipPuckerLowerRight },
        { BabbleExpression.LipPuckerUpperLeft, UnifiedExpressions.LipPuckerUpperLeft },
        { BabbleExpression.LipPuckerUpperRight, UnifiedExpressions.LipPuckerUpperRight },
        { BabbleExpression.MouthUpperLeft, UnifiedExpressions.MouthUpperLeft },
        { BabbleExpression.MouthLowerLeft, UnifiedExpressions.MouthLowerLeft },
        { BabbleExpression.MouthUpperRight, UnifiedExpressions.MouthUpperRight },
        { BabbleExpression.MouthLowerRight, UnifiedExpressions.MouthLowerRight },
        { BabbleExpression.LipSuckUpperLeft, UnifiedExpressions.LipSuckUpperLeft },
        { BabbleExpression.LipSuckUpperRight, UnifiedExpressions.LipSuckUpperRight },
        { BabbleExpression.LipSuckLowerLeft, UnifiedExpressions.LipSuckLowerLeft },
        { BabbleExpression.LipSuckLowerRight, UnifiedExpressions.LipSuckLowerRight },
        { BabbleExpression.MouthRaiserUpper, UnifiedExpressions.MouthRaiserUpper },
        { BabbleExpression.MouthRaiserLower, UnifiedExpressions.MouthRaiserLower },
        { BabbleExpression.MouthClosed, UnifiedExpressions.MouthClosed },
        { BabbleExpression.MouthCornerPullLeft, UnifiedExpressions.MouthCornerPullLeft },
        { BabbleExpression.MouthCornerPullRight, UnifiedExpressions.MouthCornerPullRight },
        { BabbleExpression.MouthFrownLeft, UnifiedExpressions.MouthFrownLeft },
        { BabbleExpression.MouthFrownRight, UnifiedExpressions.MouthFrownRight },
        { BabbleExpression.MouthDimpleLeft, UnifiedExpressions.MouthDimpleLeft },
        { BabbleExpression.MouthDimpleRight, UnifiedExpressions.MouthDimpleRight },
        { BabbleExpression.MouthUpperUpLeft, UnifiedExpressions.MouthUpperUpLeft },
        { BabbleExpression.MouthUpperUpRight, UnifiedExpressions.MouthUpperUpRight },
        { BabbleExpression.MouthLowerDownLeft, UnifiedExpressions.MouthLowerDownLeft },
        { BabbleExpression.MouthLowerDownRight, UnifiedExpressions.MouthLowerDownRight },
        { BabbleExpression.MouthPressLeft, UnifiedExpressions.MouthPressLeft },
        { BabbleExpression.MouthPressRight, UnifiedExpressions.MouthPressRight },
        { BabbleExpression.MouthStretchLeft, UnifiedExpressions.MouthStretchLeft },
        { BabbleExpression.MouthStretchRight, UnifiedExpressions.MouthStretchRight },
        { BabbleExpression.TongueOut, UnifiedExpressions.TongueOut },
        { BabbleExpression.TongueUp, UnifiedExpressions.TongueUp },
        { BabbleExpression.TongueDown, UnifiedExpressions.TongueDown },
        { BabbleExpression.TongueLeft, UnifiedExpressions.TongueLeft },
        { BabbleExpression.TongueRight, UnifiedExpressions.TongueRight },
        { BabbleExpression.TongueRoll, UnifiedExpressions.TongueRoll },
        { BabbleExpression.TongueBendDown, UnifiedExpressions.TongueBendDown },
        { BabbleExpression.TongueCurlUp, UnifiedExpressions.TongueCurlUp },
        { BabbleExpression.TongueSquish, UnifiedExpressions.TongueSquish },
        { BabbleExpression.TongueFlat, UnifiedExpressions.TongueFlat },
        { BabbleExpression.TongueTwistLeft, UnifiedExpressions.TongueTwistLeft },
        { BabbleExpression.TongueTwistRight, UnifiedExpressions.TongueTwistRight  },
    };
}
