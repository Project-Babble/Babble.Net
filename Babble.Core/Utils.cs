﻿using Babble.Core.Enums;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Babble.Core;

internal static class Utils
{
    internal static readonly Dictionary<ARKitExpression, List<UnifiedExpression>> ExpressionMapping = new()
    {
        { ARKitExpression.CheekPuffLeft, new List<UnifiedExpression>() { UnifiedExpression.CheekPuffLeft } },
        { ARKitExpression.CheekPuffRight, new List<UnifiedExpression>() { UnifiedExpression.CheekPuffRight } },
        { ARKitExpression.CheekSuckLeft, new List<UnifiedExpression>() { UnifiedExpression.CheekSuckLeft } },
        { ARKitExpression.CheekSuckRight, new List<UnifiedExpression>() { UnifiedExpression.CheekSuckRight } },
        { ARKitExpression.JawOpen, new List<UnifiedExpression>() { UnifiedExpression.JawOpen } },
        { ARKitExpression.JawForward, new List<UnifiedExpression>() { UnifiedExpression.JawForward } },
        { ARKitExpression.JawLeft, new List<UnifiedExpression>() { UnifiedExpression.JawLeft } },
        { ARKitExpression.JawRight, new List<UnifiedExpression>() { UnifiedExpression.JawRight } },
        { ARKitExpression.NoseSneerLeft, new List<UnifiedExpression>() { UnifiedExpression.NoseSneerLeft } },
        { ARKitExpression.NoseSneerRight, new List<UnifiedExpression>() { UnifiedExpression.NoseSneerRight } },
        { ARKitExpression.MouthFunnel, new List<UnifiedExpression>() 
        { 
            UnifiedExpression.LipFunnelLowerLeft,
            UnifiedExpression.LipFunnelLowerRight,
            UnifiedExpression.LipFunnelUpperLeft,
            UnifiedExpression.LipFunnelUpperRight
        }},
        { ARKitExpression.MouthPucker, new List<UnifiedExpression>() 
        { 
            UnifiedExpression.LipPuckerLowerLeft,
            UnifiedExpression.LipPuckerLowerRight,
            UnifiedExpression.LipPuckerUpperLeft,
            UnifiedExpression.LipPuckerUpperRight,
        }},
        { ARKitExpression.MouthLeft, new List<UnifiedExpression>() 
        { 
            UnifiedExpression.MouthUpperLeft,
            UnifiedExpression.MouthLowerLeft
        }},
        { ARKitExpression.MouthRight, new List<UnifiedExpression>() 
        {
            UnifiedExpression.MouthUpperRight,
            UnifiedExpression.MouthLowerRight
        }},
        { ARKitExpression.MouthRollUpper, new List<UnifiedExpression>() 
        { 
            UnifiedExpression.LipSuckUpperLeft,
            UnifiedExpression.LipSuckUpperRight,
        }},
        { ARKitExpression.MouthRollLower, new List<UnifiedExpression>() 
        { 
            UnifiedExpression.LipSuckLowerLeft,
            UnifiedExpression.LipSuckLowerRight,
        }},
        { ARKitExpression.MouthShrugUpper, new List<UnifiedExpression>() { UnifiedExpression.MouthRaiserUpper } },
        { ARKitExpression.MouthShrugLower, new List<UnifiedExpression>() { UnifiedExpression.MouthRaiserLower } },
        { ARKitExpression.MouthClose, new List<UnifiedExpression>() { UnifiedExpression.MouthClosed } },
        { ARKitExpression.MouthSmileLeft, new List<UnifiedExpression>() { UnifiedExpression.MouthCornerPullLeft } },
        { ARKitExpression.MouthSmileRight, new List<UnifiedExpression>() { UnifiedExpression.MouthCornerPullRight } },
        { ARKitExpression.MouthFrownLeft, new List<UnifiedExpression>() { UnifiedExpression.MouthFrownLeft } },
        { ARKitExpression.MouthFrownRight, new List<UnifiedExpression>() { UnifiedExpression.MouthFrownRight } },
        { ARKitExpression.MouthDimpleLeft, new List<UnifiedExpression>() { UnifiedExpression.MouthDimpleLeft } },
        { ARKitExpression.MouthDimpleRight, new List<UnifiedExpression>() { UnifiedExpression.MouthDimpleRight } },
        { ARKitExpression.MouthUpperUpLeft, new List<UnifiedExpression>() { UnifiedExpression.MouthUpperUpLeft } },
        { ARKitExpression.MouthUpperUpRight, new List<UnifiedExpression>() { UnifiedExpression.MouthUpperUpRight } },
        { ARKitExpression.MouthLowerDownLeft, new List<UnifiedExpression>() { UnifiedExpression.MouthLowerDownLeft } },
        { ARKitExpression.MouthLowerDownRight, new List<UnifiedExpression>() { UnifiedExpression.MouthLowerDownRight } },
        { ARKitExpression.MouthPressLeft, new List<UnifiedExpression>() { UnifiedExpression.MouthPressLeft } },
        { ARKitExpression.MouthPressRight, new List<UnifiedExpression>() { UnifiedExpression.MouthPressRight } },
        { ARKitExpression.MouthStretchLeft, new List<UnifiedExpression>() { UnifiedExpression.MouthStretchLeft } },
        { ARKitExpression.MouthStretchRight, new List<UnifiedExpression>() { UnifiedExpression.MouthStretchRight } },
        { ARKitExpression.TongueOut, new List<UnifiedExpression>() { UnifiedExpression.TongueOut } },
        { ARKitExpression.TongueUp, new List<UnifiedExpression>() { UnifiedExpression.TongueUp } },
        { ARKitExpression.TongueDown, new List<UnifiedExpression>() { UnifiedExpression.TongueDown } },
        { ARKitExpression.TongueLeft, new List<UnifiedExpression>() { UnifiedExpression.TongueLeft } },
        { ARKitExpression.TongueRight, new List<UnifiedExpression>() { UnifiedExpression.TongueRight } },
        { ARKitExpression.TongueRoll, new List<UnifiedExpression>() { UnifiedExpression.TongueRoll } },
        { ARKitExpression.TongueBendDown, new List<UnifiedExpression>() { UnifiedExpression.TongueBendDown } },
        { ARKitExpression.TongueCurlUp, new List<UnifiedExpression>() { UnifiedExpression.TongueCurlUp } },
        { ARKitExpression.TongueSquish, new List<UnifiedExpression>() { UnifiedExpression.TongueSquish } },
        { ARKitExpression.TongueFlat, new List<UnifiedExpression>() { UnifiedExpression.TongueFlat } },
        { ARKitExpression.TongueTwistLeft, new List<UnifiedExpression>() { UnifiedExpression.TongueTwistLeft } },
        { ARKitExpression.TongueTwistRight, new List<UnifiedExpression>() { UnifiedExpression.TongueTwistRight } }
    };

    internal static readonly Dictionary<ARKitExpression, float> ARKitExpressions = new()
    {
        { ARKitExpression.CheekPuffLeft, 0f },
        { ARKitExpression.CheekPuffRight, 0f },
        { ARKitExpression.CheekSuckLeft, 0f },
        { ARKitExpression.CheekSuckRight, 0f },
        { ARKitExpression.JawOpen, 0f },
        { ARKitExpression.JawForward, 0f },
        { ARKitExpression.JawLeft, 0f },
        { ARKitExpression.JawRight, 0f },
        { ARKitExpression.NoseSneerLeft, 0f },
        { ARKitExpression.NoseSneerRight, 0f },
        { ARKitExpression.MouthFunnel, 0f },
        { ARKitExpression.MouthPucker, 0f },
        { ARKitExpression.MouthLeft, 0f },
        { ARKitExpression.MouthRight, 0f },
        { ARKitExpression.MouthRollUpper, 0f },
        { ARKitExpression.MouthRollLower, 0f },
        { ARKitExpression.MouthShrugUpper, 0f },
        { ARKitExpression.MouthShrugLower, 0f },
        { ARKitExpression.MouthClose, 0f },
        { ARKitExpression.MouthSmileLeft, 0f },
        { ARKitExpression.MouthSmileRight, 0f },
        { ARKitExpression.MouthFrownLeft, 0f },
        { ARKitExpression.MouthFrownRight, 0f },
        { ARKitExpression.MouthDimpleLeft, 0f },
        { ARKitExpression.MouthDimpleRight, 0f },
        { ARKitExpression.MouthUpperUpLeft, 0f },
        { ARKitExpression.MouthUpperUpRight, 0f },
        { ARKitExpression.MouthLowerDownLeft, 0f },
        { ARKitExpression.MouthLowerDownRight, 0f },
        { ARKitExpression.MouthPressLeft, 0f },
        { ARKitExpression.MouthPressRight, 0f },
        { ARKitExpression.MouthStretchLeft, 0f },
        { ARKitExpression.MouthStretchRight, 0f },
        { ARKitExpression.TongueOut, 0f },
        { ARKitExpression.TongueUp, 0f },
        { ARKitExpression.TongueDown, 0f },
        { ARKitExpression.TongueLeft, 0f },
        { ARKitExpression.TongueRight, 0f },
        { ARKitExpression.TongueRoll, 0f },
        { ARKitExpression.TongueBendDown, 0f },
        { ARKitExpression.TongueCurlUp, 0f },
        { ARKitExpression.TongueSquish, 0f },
        { ARKitExpression.TongueFlat, 0f },
        { ARKitExpression.TongueTwistLeft, 0f },
        { ARKitExpression.TongueTwistRight, 0f }
    };

    /// <summary>
    /// Represents the expected size of the input image,
    /// grayscale at 256x256px, 1 float per pixel, normalized to 0f-1f.
    /// </summary>
    private const int EXPECTED_SIZE = 256 * 256; 

    /// <summary>
    /// Converts a float[256 * 256] array to a (dense) Tensor.
    /// An exception is raised if the array is the incorrect size.
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    internal static Tensor<float> PreprocessFrame(float[] frame)
    {
        if (frame.Length != EXPECTED_SIZE)
            throw new InvalidDataException();

        var input = new DenseTensor<float>([1, 1, 256, 256]);

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                // Bit shift this 8 instead??
                input[0, 0, y, x] = frame[y * 256 + x];
            }
        }

        return input;
    }
}
