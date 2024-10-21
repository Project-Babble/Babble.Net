namespace Babble.Maui.Scripts.Platforms;

internal interface IPlatformConnector
{
    public abstract bool Initialize(string camera);
    public abstract bool GetCameraData(out float[] data);
    public abstract bool Terminate();
}
