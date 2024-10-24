using Babble.Core.Scripts.EmguCV;
using Emgu.CV;
using Emgu.CV.Cuda;

namespace Babble.Core.Scripts;

internal class RuntimeDetector
{
    /// <summary>
    /// Gets supported non-CPU runtimes for this platform
    /// </summary>
    /// <returns></returns>
    public static HashSet<Runtime> GetRuntimes()
    {
        var runtimes = new HashSet<Runtime>();

        if (CudaInvoke.HasCuda)
            runtimes.Add(Runtime.CUDA);

        if (CvInvoke.HaveOpenCL)
            runtimes.Add(Runtime.OPEN_CL);

        if (CvInvoke.HaveOpenCLCompatibleGpuDevice)
            runtimes.Add(Runtime.OPEN_CL_GPU);

        if (CvInvoke.HaveOpenVX)
            runtimes.Add(Runtime.OPEN_VX);

        return runtimes;
    }

    public static bool IsRuntimeSupported(Runtime runtime)
    {
        if (runtime == Runtime.CPU) 
            return true;

        return GetRuntimes().Contains(runtime);
    }
}
