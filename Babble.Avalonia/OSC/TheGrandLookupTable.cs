using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;

namespace Babble.Avalonia.OSC;

internal static class TheGrandLookupTable
{
    internal static readonly Dictionary<string, Func<float>> Table = new()
    {
        {"CheekPuffLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight },
        {"CheekPuffLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight },
        {"CheekPuffRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight },
        {"CheekPuffRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight },
        {"CheekSquintLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight },
        {"CheekSquintLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight },
        {"CheekSquintRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight },
        {"CheekSquintRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight },
        {"CheekSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight +
                             UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2 },
        {"CheekSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight +
                                      UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2 },
        {"CheeksSquint", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight +
                               UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight) / 2 },
        {"CheeksSquintNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight +
                                        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight) / 2 },
        {"JawForward", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawForward].Weight },
        {"JawForwardNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawForward].Weight },
        {"JawLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawLeft].Weight },
        {"JawLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawLeft].Weight },
        {"JawOpen", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight },
        {"JawOpenApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight * 1.5f },
        {"JawOpenApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight * 1.5f },
        {"JawOpenForward", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                 UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawForward].Weight) / 2 },
        {"JawOpenForwardNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                          UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawForward].Weight) / 2 },
        {"JawOpenNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight },
        {"JawOpenOverlay", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight },
        {"JawOpenOverlayNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight },
        {"JawOpenPuff", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight +
                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 3 },
        {"JawOpenPuffLeft", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                  UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"JawOpenPuffLeftNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                           UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"JawOpenPuffNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight +
                                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 3 },
        {"JawOpenPuffRight", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"JawOpenPuffRightNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"JawOpenSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight +
                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 3 },
        {"JawOpenSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight +
                                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight +
                                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 3 },
        {"JawRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawRight].Weight },
        {"JawRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawRight].Weight },
        {"JawX", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawLeft].Weight -
                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawRight].Weight },
        {"JawXNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawLeft].Weight -
                                UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawRight].Weight) },
        {"LipTrackingActive", () => 1.0f },
        {"Max", () => 1.0f },
        {"MaxNegative", () => -1.0f },
        {"MouthApeShape", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight * 1.5f },
        {"MouthApeShapeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight * 1.5f },
        {"MouthDimple", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight +
                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight) / 2 },
        {"MouthDimpleLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight },
        {"MouthDimpleLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight },
        {"MouthDimpleNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight +
                                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight) / 2 },
        {"MouthDimpleRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight },
        {"MouthDimpleRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight },
        {"MouthLower", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                             UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 },
        {"MouthLowerDownApe", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                    UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 * 1.5f },
        {"MouthLowerDownApeNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                             UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 * 1.5f) },
        {"MouthLowerDownInside", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight +
                                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight) / 2 },
        {"MouthLowerDownInsideNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight +
                                               UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight) / 2 },
        {"MouthLowerDownLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight },
        {"MouthLowerDownLeftApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight * 1.5f },
        {"MouthLowerDownLeftApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight * 1.5f },
        {"MouthLowerDownLeftLowerInside", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight },
        {"MouthLowerDownLeftLowerInsideNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight },
        {"MouthLowerDownLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight },
        {"MouthLowerDownLeftOverlay", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight },
        {"MouthLowerDownLeftOverlayNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight },
        {"MouthLowerDownLeftPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                         UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 2 },
        {"MouthLowerDownLeftPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                                  UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 2 },
        {"MouthLowerDownLeftPuffLeft", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                             UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"MouthLowerDownLeftPuffLeftNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                                      UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"MouthLowerDownLeftSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                         UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2 },
        {"MouthLowerDownLeftSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                                  UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2 },
        {"MouthLowerDownLowerInside", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight) / 2 },
        {"MouthLowerDownLowerInsideNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight +
                                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight) / 2 },
        {"MouthLowerDownOverlay", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 },
        {"MouthLowerDownOverlayNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                                 UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 },
        {"MouthLowerDownPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 4 },
        {"MouthLowerDownPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 4 },
        {"MouthLowerDownPuff", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 4 },
        {"MouthLowerDownPuffLeft", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                         UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"MouthLowerDownPuffLeftNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                                  UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"MouthLowerDownPuffNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 4 },
        {"MouthLowerDownPuffRight", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                      UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthLowerDownPuffRightNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthLowerDownRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight },
        {"MouthLowerDownRightApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight * 1.5f },
        {"MouthLowerDownRightApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight * 1.5f },
        {"MouthLowerDownRightLowerInside", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight },
        {"MouthLowerDownRightLowerInsideNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight },
        {"MouthLowerDownRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight },
        {"MouthLowerDownRightOverlay", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight },
        {"MouthLowerDownRightOverlayNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight },
        {"MouthLowerDownRightPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                          UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 2 },
        {"MouthLowerDownRightPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 2 },
        {"MouthLowerDownRightPuffRight", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                               UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthLowerDownRightPuffRightNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                                        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthLowerDownRightSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                          UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"MouthLowerDownRightSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"MouthLowerDownSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 4 },
        {"MouthLowerDownSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 4 },
        {"MouthLowerInside", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight +
                               UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight) / 2 },
        {"MouthLowerInsideNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight) / 2 },
        {"MouthLowerInsideOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight +
                                           UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight) / 2 },
        {"MouthLowerInsideOverturnNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight +
                                                    UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight) / 2 },
        {"MouthLowerLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight },
        {"MouthLowerLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight },
        {"MouthLowerNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                      UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 },
        {"MouthLowerOverlay", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                    UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 },
        {"MouthLowerOverlayNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight +
                                             UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 },
        {"MouthLowerOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight) / 2 },
        {"MouthLowerOverturnNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight) / 2 },
        {"MouthLowerRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight },
        {"MouthLowerRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight },
        {"MouthPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight +
                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight +
                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight +
                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 4 },
        {"MouthPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 4 },
        {"MouthPress", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight +
                         UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight) / 2 },
        {"MouthPressLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight },
        {"MouthPressLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight },
        {"MouthPressNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight +
                                      UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight) / 2 },
        {"MouthPressRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight },
        {"MouthPressRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight },
        {"MouthRaiserLower", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthRaiserLower].Weight },
        {"MouthRaiserLowerNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthRaiserLower].Weight },
        {"MouthRaiserUpper", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthRaiserUpper].Weight },
        {"MouthRaiserUpperNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthRaiserUpper].Weight },
        {"MouthSadLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight },
        {"MouthSadLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight },
        {"MouthSadRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight },
        {"MouthSadRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight },
        {"MouthSmileLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight },
        {"MouthSmileLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight },
        {"MouthSmileRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight },
        {"MouthSmileRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight },
        {"MouthStretch", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight +
                               UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight) / 2 },
        {"MouthStretchLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight },
        {"MouthStretchLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight },
        {"MouthStretchNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight +
                                        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight) / 2 },
        {"MouthStretchRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight },
        {"MouthStretchRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight },
        {"MouthTightener", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight +
                                 UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight) / 2 },
        {"MouthTightenerLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight },
        {"MouthTightenerLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight },
        {"MouthTightenerNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight +
                                          UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight) / 2 },
        {"MouthTightenerRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight },
        {"MouthTightenerRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight },
        {"MouthUpper", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                             UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight) / 2 },
        {"MouthUpperInside", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight +
                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight) / 2 },
        {"MouthUpperInsideNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight) / 2 },
        {"MouthUpperInsideOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight +
                                           UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight) / 2 },
        {"MouthUpperInsideOverturnNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight +
                                                    UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight) / 2 },
        {"MouthUpperLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight },
        {"MouthUpperLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight },
        {"MouthUpperNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                      UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight) / 2 },
        {"MouthUpperOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight) / 2 },
        {"MouthUpperOverturnNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight) / 2 },
        {"MouthUpperRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight },
        {"MouthUpperRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight },
        {"MouthUpperUpApe", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                  UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight) / 2 * 1.5f },
        {"MouthUpperUpApeNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight) / 2 * 1.5f) },
        {"MouthUpperUpInside", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight +
                                     UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight) / 2 },
        {"MouthUpperUpInsideNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight +
                                              UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight) / 2 },
        {"MouthUpperUpLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight },
        {"MouthUpperUpLeftApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight * 1.5f },
        {"MouthUpperUpLeftApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight * 1.5f },
        {"MouthUpperUpLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight },
        {"MouthUpperUpLeftOverlay", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight },
        {"MouthUpperUpLeftOverlayNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight },
        {"MouthUpperUpLeftPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight) / 2 },
        {"MouthUpperUpLeftPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                                UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight) / 2 },
        {"MouthUpperUpLeftPuffLeft", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                           UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"MouthUpperUpLeftPuffLeftNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                                    UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2 },
        {"MouthUpperUpLeftSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                       UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2 },
        {"MouthUpperUpLeftSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                                UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2 },
        {"MouthUpperUpLeftUpperInside", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight },
        {"MouthUpperUpLeftUpperInsideNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight },
        {"MouthUpperUpOverlay", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                      UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight) / 2 },
        {"MouthUpperUpOverlayNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                               UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight) / 2 },
        {"MouthUpperUpPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight +
                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight +
                                   UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight) / 4 },
        {"MouthUpperUpPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight +
                                            UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight) / 4 },
        {"MouthUpperUpPuff", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthUpperUpPuffLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight },
        {"MouthUpperUpPuffLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight },
        {"MouthUpperUpPuffNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthUpperUpPuffRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight },
        {"MouthUpperUpPuffRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight },
        {"MouthUpperUpRight", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) / 2 },
        {"MouthUpperUpRightApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight * 1.5f },
        {"MouthUpperUpRightApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight * 1.5f },
        {"MouthUpperUpRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight },
        {"MouthUpperUpRightOverlay", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight },
        {"MouthUpperUpRightOverlayNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight },
        {"MouthUpperUpRightPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight) / 2 },
        {"MouthUpperUpRightPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight) / 2 },
        {"MouthUpperUpRightPuffRight", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthUpperUpRightPuffRightNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 },
        {"MouthUpperUpRightSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"MouthUpperUpRightSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"MouthUpperUpRightUpperInside", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight },
        {"MouthUpperUpRightUpperInsideNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight },
        {"MouthUpperUpSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"MouthUpperUpSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"MouthUpperUpUpperInside", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight) / 2 },
        {"MouthUpperUpUpperInsideNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight) / 2 },
        {"MouthX", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight - UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight },
        {"MouthXNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight - UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight) },
        {"NoseSneer", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight) / 2 },
        {"NoseSneerLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight },
        {"NoseSneerLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight },
        {"NoseSneerNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight) / 2 },
        {"NoseSneerRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight },
        {"NoseSneerRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight },
        {"PuffLeftLowerOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight * 1.2f },
        {"PuffLeftLowerOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight * 1.2f },
        {"PuffLeftOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight * 1.5f },
        {"PuffLeftOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight * 1.5f },
        {"PuffLeftUpperOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight * 1.8f },
        {"PuffLeftUpperOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight * 1.8f },
        {"PuffLowerOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 * 1.2f },
        {"PuffLowerOverturnNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 * 1.2f) },
        {"PuffOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 * 1.5f },
        {"PuffOverturnNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 * 1.5f) },
        {"PuffRightLowerOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight * 1.2f },
        {"PuffRightLowerOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight * 1.2f },
        {"PuffRightOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight * 1.5f },
        {"PuffRightOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight * 1.5f },
        {"PuffRightUpperOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight * 1.8f },
        {"PuffRightUpperOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight * 1.8f },
        {"PuffSuck", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"PuffSuckLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight },
        {"PuffSuckLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight },
        {"PuffSuckNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2 },
        {"PuffSuckRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight },
        {"PuffSuckRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight },
        {"PuffUpperOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 * 1.8f },
        {"PuffUpperOverturnNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight) / 2 * 1.8f) },
        {"SmileApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.5f },
        {"SmileApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.5f },
        {"SmileLeftApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.5f },
        {"SmileLeftApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.5f },
        {"SmileLeftLowerOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.2f },
        {"SmileLeftLowerOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.2f },
        {"SmileLeftOverlay", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight },
        {"SmileLeftOverlayNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight },
        {"SmileLeftOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.5f },
        {"SmileLeftOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.5f },
        {"SmileLeftPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 2 },
        {"SmileLeftPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 2 },
        {"SmileLeftUpperOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.8f },
        {"SmileLeftUpperOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * 1.8f },
        {"SmileLowerOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2 * 1.2f },
        {"SmileLowerOverturnNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2 * 1.2f) },
        {"SmileOverlay", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2 },
        {"SmileOverlayNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2) },
        {"SmileOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2 * 1.5f },
        {"SmileOverturnNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2 * 1.5f) },
        {"SmilePout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 4 },
        {"SmilePoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 4 },
        {"SmileRightApe", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.5f },
        {"SmileRightApeNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.5f },
        {"SmileRightLowerOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.2f },
        {"SmileRightLowerOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.2f },
        {"SmileRightOverlay", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight },
        {"SmileRightOverlayNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight },
        {"SmileRightOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.5f },
        {"SmileRightOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.5f },
        {"SmileRightPout", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 2 },
        {"SmileRightPoutNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 2 },
        {"SmileRightUpperOverturn", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.8f },
        {"SmileRightUpperOverturnNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * 1.8f },
        {"SmileSad", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight) / 2 },
        {"SmileSadLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight },
        {"SmileSadLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight },
        {"SmileSadNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight) / 2 },
        {"SmileSadRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight },
        {"SmileSadRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight },
        {"SmileUpperOverturn", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2 * 1.8f },
        {"SmileUpperOverturnNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight) / 2 * 1.8f) },
        {"TongueDown", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueDown].Weight },
        {"TongueDownLeftMorph", () => (UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueDown].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueLeft].Weight) / 2 },
        {"TongueDownLeftMorphNegative", () => -(UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueDown].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueLeft].Weight) / 2 },
        {"TongueDownNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueDown].Weight },
        {"TongueDownRightMorph", () => (UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueDown].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueRight].Weight) / 2 },
        {"TongueDownRightMorphNegative", () => -(UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueDown].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueRight].Weight) / 2 },
        {"TongueLeft", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueLeft].Weight },
        {"TongueLeftNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueLeft].Weight },
        {"TongueLongStep1", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight },
        {"TongueLongStep1Negative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight },
        {"TongueLongStep2", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight },
        {"TongueLongStep2Negative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight },
        {"TongueRight", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueRight].Weight },
        {"TongueRightNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueRight].Weight },
        {"TongueRoll", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueRoll].Weight },
        {"TongueRollNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueRoll].Weight },
        {"TongueSteps", () => (UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight) / 2 },
        {"TongueStepsNegative", () => -((UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight + UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight) / 2) },
        {"TongueUp", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueUp].Weight },
        {"TongueUpLeftMorph", () => (UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueUp].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueLeft].Weight) / 2 },
        {"TongueUpLeftMorphNegative", () => -(UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueUp].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueLeft].Weight) / 2 },
        {"TongueUpNegative", () => -UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueUp].Weight },
        {"TongueUpRightMorph", () => (UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueUp].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueRight].Weight) / 2 },
        {"TongueUpRightMorphNegative", () => -(UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueUp].Weight + UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.TongueRight].Weight) / 2 },
        {"TongueX", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueLeft].Weight - UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueRight].Weight },
        {"TongueXNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueLeft].Weight - UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueRight].Weight) },
        {"TongueY", () => UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueDown].Weight - UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueUp].Weight },
        {"TongueYNegative", () => -(UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueDown].Weight - UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueUp].Weight) }
    };
}