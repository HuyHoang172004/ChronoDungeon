public interface ITimeLoopResettable
{
    void CaptureInitialState();
    void ResetToInitialState();
}
