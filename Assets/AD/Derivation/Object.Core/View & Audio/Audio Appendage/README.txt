对于小音频，推荐使用Decompress On Load+取消Preload Audio Data+PCM
对于大音频，推荐使用Streaming

对于不需要立刻打开，而是允许一定延迟播放的音频，推荐使用Streaming+取消Preload Audio Data

对于本身音质需求不高的较大音频，推荐使用ADPCM