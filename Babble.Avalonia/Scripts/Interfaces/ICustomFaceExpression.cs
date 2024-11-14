using VRCFaceTracking.Core.Params.Data;

namespace Hypernex.ExtendedTracking
{
    public interface ICustomFaceExpression
    {
        public string Name { get; }
        public float GetWeight(UnifiedTrackingData unifiedTrackingData);
        bool IsMatch(string parameterName);
    }
}