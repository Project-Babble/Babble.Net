using System.Reflection;
using VRCFaceTracking;
using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.DataTypes;
using Object = System.Object;
using Vector2 = VRCFaceTracking.Core.Types.Vector2;

namespace Hypernex.ExtendedTracking;

public static partial class VRCFTParameters
{
    public static bool UseBinary { get; set; } = true;
    
    public static List<VRCFTProgrammableExpression> GetParameters()
    {
        Parameter[] parametersToPullFrom = UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray();
        return GetParameters(parametersToPullFrom);
    }
    
    private static List<VRCFTProgrammableExpression> GetParameters(Parameter[] parametersToPullFrom)
    {
        List<VRCFTProgrammableExpression> expressions = new();
        foreach (Parameter vrcftParameter in parametersToPullFrom)
        {
            Type parameterType = GetRootTypeNoAbstractParameter(vrcftParameter.GetType());
            if (parameterType == typeof(BaseParam<float>))
            {
                BaseParam<float> paramLiteral = (BaseParam<float>) vrcftParameter;
                (Func<string>, Func<UnifiedTrackingData, float>) paramValue = GetBaseParamValue(paramLiteral);
                expressions.Add(new VRCFTProgrammableExpression(paramValue.Item1, paramValue.Item2));
            }
            else if (parameterType == typeof(BaseParam<bool>))
            {
                BaseParam<bool> paramLiteral = (BaseParam<bool>) vrcftParameter;
                (Func<string>, Func<UnifiedTrackingData, bool>) paramValue = GetBaseParamValue(paramLiteral);
                expressions.Add(new VRCFTProgrammableExpression(paramValue.Item1,
                    data => paramValue.Item2.Invoke(data) ? 1.0f : 0.0f));
            }
            else if (parameterType == typeof(BaseParam<Vector2>))
            {
                BaseParam<Vector2> paramLiteral = (BaseParam<Vector2>) vrcftParameter;
                (Func<string>, Func<UnifiedTrackingData, Vector2>) paramValue = GetBaseParamValue(paramLiteral);
                expressions.Add(new VRCFTProgrammableExpression(() => paramValue.Item1.Invoke() + "X",
                    data => paramValue.Item2.Invoke(data).x));
                expressions.Add(new VRCFTProgrammableExpression(() => paramValue.Item1.Invoke() + "Y",
                    data => paramValue.Item2.Invoke(data).y));
            }
            else if (UseBinary && parameterType == typeof(BinaryBaseParameter))
            {
                BinaryBaseParameter paramLiteral = (BinaryBaseParameter) vrcftParameter;
                foreach ((Func<string>, Func<UnifiedTrackingData, bool>) valueTuple in
                         GetBinaryBaseParamValue(paramLiteral))
                    expressions.Add(new VRCFTProgrammableExpression(valueTuple.Item1,
                        data => valueTuple.Item2.Invoke(data) ? 1.0f : 0.0f));
            }
        }
        foreach (Parameter vrcftParameter in parametersToPullFrom)
        {
            Type parameterType = GetRootTypeNoAbstractParameter(vrcftParameter.GetType());
            if (parameterType != typeof(EParam)) continue;
            EParam paramLiteral = (EParam) vrcftParameter;
            bool exists = false;
            foreach ((string, Parameter) parameter in paramLiteral.GetParamNames())
            {
                if (expressions.Select(x => x.Name).Contains(parameter.Item1))
                {
                    exists = true;
                    break;
                }
                foreach ((string, Parameter) valueTuple in parameter.Item2.GetParamNames())
                {
                    if (expressions.Select(x => x.Name).Contains(valueTuple.Item1))
                    {
                        exists = true;
                        break;
                    }
                    if (expressions.Select(x => x.Name).Contains(valueTuple.Item2.GetParamNames()[0].paramName))
                    {
                        exists = true;
                        break;
                    }
                }
            }
            if(exists) continue;
            foreach ((Func<string>, Func<UnifiedTrackingData, float>) valueTuple in GetEParamValue(paramLiteral))
                expressions.Add(new VRCFTProgrammableExpression(valueTuple.Item1, valueTuple.Item2));
        }
        return expressions;
    }

    private static Type GetRootTypeNoAbstractParameter(Type derivedType)
    {
        Type baseType = derivedType;
        Type? lastType = derivedType;
        while (lastType != null && lastType != typeof(Parameter) && lastType != typeof(Object))
        {
            baseType = lastType;
            lastType = baseType.BaseType;
        }
        return baseType;
    }

    private static (Func<string>, Func<UnifiedTrackingData, T>) GetBaseParamValue<T>(BaseParam<T> baseParam) where T : struct
    {
        Type paramType = GetRootTypeNoAbstractParameter(baseParam.GetType());
        Func<UnifiedTrackingData, T> getValueFunc =
            (Func<UnifiedTrackingData, T>) paramType.GetField("_getValueFunc",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(baseParam);
        return (() => baseParam.GetParamNames()[0].Item1, getValueFunc);
    }
    
    private static (Func<string>, Func<UnifiedTrackingData, bool>)[] GetBinaryBaseParamValue(BinaryBaseParameter baseParam)
    {
        List<(Func<string>, Func<UnifiedTrackingData, bool>)> binaryParameters = new();
        (string, Parameter)[] paramNames = baseParam.GetParamNames();
        foreach ((string, Parameter) valueTuple in paramNames)
        {
            BaseParam<bool> paramLiteral = (BaseParam<bool>) valueTuple.Item2;
            binaryParameters.Add((() => paramLiteral.GetParamNames()[0].Item1,
                (Func<UnifiedTrackingData, bool>) paramLiteral.GetType()
                    .GetField("_getValueFunc", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(paramLiteral)));
        }
        return binaryParameters.ToArray();
    }
    
    private static (Func<string>, Func<UnifiedTrackingData, float>)[] GetEParamValue(EParam eParam)
    {
        List<(Func<string>, Func<UnifiedTrackingData, float>)> eParameters = new();
        (string, Parameter)[] paramNames = eParam.GetParamNames();
        foreach ((string, Parameter) valueTuple in paramNames)
        {
            Type paramLiteralType = GetRootTypeNoAbstractParameter(valueTuple.Item2.GetType());
            if (paramLiteralType == typeof(BaseParam<float>))
            {
                (Func<string>, Func<UnifiedTrackingData, float>) baseParamFunc =
                    GetBaseParamValue((BaseParam<float>) valueTuple.Item2);
                eParameters.Add(baseParamFunc);
            }
            else if (UseBinary && paramLiteralType == typeof(BinaryBaseParameter))
            {
                BinaryBaseParameter binaryBaseParameter = (BinaryBaseParameter) valueTuple.Item2;
                foreach ((Func<string>, Func<UnifiedTrackingData, bool>) binaryTuple in GetBinaryBaseParamValue(
                             binaryBaseParameter))
                    eParameters.Add((binaryTuple.Item1, data => binaryTuple.Item2.Invoke(data) ? 1.0f : 0.0f));
            }
        }
        return eParameters.ToArray();
    }
}