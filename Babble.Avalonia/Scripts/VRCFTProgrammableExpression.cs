using System.Text.RegularExpressions;
using VRCFaceTracking.Core.Params.Data;

namespace Hypernex.ExtendedTracking;

public static partial class VRCFTParameters
{
    public class VRCFTProgrammableExpression : ICustomFaceExpression
    {
        private string? lastRegexName;
        private Regex? lastRegex;

        private Regex Regex
        {
            get
            {
                if (lastRegexName == null) lastRegexName = Name;
                if (lastRegex == null || lastRegexName != Name)
                {
                    lastRegexName = Name;
                    lastRegex = new Regex(@"(?<!(v\d+))(/" + lastRegexName + ")$|^(" + lastRegexName + ")$");
                    matches.Clear();
                }
                return lastRegex;
            }
        }
        
        private Dictionary<string, bool> matches = new();

        private Func<string> getNameFunc;
        private Func<UnifiedTrackingData, float> getWeightFunc;

        public VRCFTProgrammableExpression(Func<string> getNameFunc, Func<UnifiedTrackingData, float> getWeightFunc)
        {
            this.getNameFunc = getNameFunc;
            this.getWeightFunc = getWeightFunc;
        }

        public string Name => getNameFunc.Invoke();
        public float GetWeight(UnifiedTrackingData unifiedTrackingData) => getWeightFunc.Invoke(unifiedTrackingData);

        public bool IsMatch(string parameterName)
        {
            bool match;
            if (!matches.TryGetValue(parameterName, out match))
            {
                match = Regex.IsMatch(parameterName);
                matches.Add(parameterName, match);
            }
            return match;
        }
    }
}