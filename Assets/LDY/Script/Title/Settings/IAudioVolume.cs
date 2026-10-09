namespace LDY.Script
{
    public enum VolumeChannel
    {
        Master,
        Bgm,
        Sfx
    }

    // 설정창이 음량을 읽고 쓰는 유일한 창구. 값은 0~1(선형)이며 실제 오디오 연결은 구현체(어댑터)가 맡는다.
    public interface IAudioVolume
    {
        float Get(VolumeChannel channel);
        void Set(VolumeChannel channel, float value);
    }
}
