namespace SlotGame
{
    /// <summary>Provides random values so game outcomes can be reproduced in verification.</summary>
    public interface IRandomSource
    {
        int Next(int count);
    }
}
