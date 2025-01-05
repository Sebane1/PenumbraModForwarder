using PenumbraModForwarder.Common.Enums;

namespace PenumbraModForwarder.Common.Interfaces;

public interface ISoundManagerService
{
    Task PlaySoundAsync(SoundType soundType, float volume = 1.0f);
}