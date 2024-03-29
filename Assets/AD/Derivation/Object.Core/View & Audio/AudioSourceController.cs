using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AD.Utility;
using AD.Utility.Object;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace AD.UI
{
    public interface ICanDrawLine
    {
        void DrawLine(LineRenderer renderer, AudioSourceController source);
    }

    [Serializable]
    public class SourcePair
    {
        public AudioClip Clip = null;
        public SourcePair ChangeClip(AudioClip newClip)
        {
            Clip = newClip;
            return this;
        }
        public string Name = "New Pair";
        public string CilpName = "New Clip";

        public ICanDrawLine LineDrawer = null;

        public bool IsLoaded
        {
            get
            {
                if (Clip != null)
                    return Clip.loadState == AudioDataLoadState.Loaded;
                else return false;
            }
        }
    }

    [Serializable]
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("UI/AD/AudioSourceController", 100)]
    public sealed class AudioSourceController : MonoBehaviour, ICanPrepareToOtherScene
    {
        #region Attribute

        public bool IsNeedContext => false;
        public BehaviourContext Context { get => null; }

        public string ElementName { get; set; } = "null";
        public int SerialNumber { get; set; } = 0;

        public AudioSource Source { get; private set; }
        public List<SourcePair> SourcePairs = new List<SourcePair>();
        private int CurrentPairIndex = 0;
        private float CurrentClock = 0;
        private float delay = 0;
        private bool IsDelayToStart = false;
        private bool IsNowPlaying = false;

        public SourcePair CurrentSourcePair
        {
            get
            {
                if (SourcePairs.Count > 0) return SourcePairs[CurrentPairIndex];
                else return null;
            }
            set
            {
                SourcePairs[CurrentPairIndex] = value;
            }
        }
        public AudioClip CurrentClip
        {
            get
            {
                if (SourcePairs.Count > 0) return SourcePairs[CurrentPairIndex].Clip;
                else return null;
            }
            set
            {
                if (SourcePairs.Count > 0) SourcePairs[CurrentPairIndex].Clip = value;
                else Debug.LogWarning("this AudioSource's list of Source is empty,but you try to change it");
            }
        }
        public int CurrentIndex
        {
            get { return CurrentPairIndex; }
        }
        public bool IsPlay
        {
            get { return (Source.isPlaying || IsDelayToStart) && !IsPause; }
            set
            {
                if (SourcePairs.Count > 0)
                {
                    if (value) Play();
                    else Pause();
                }
            }
        }
        public bool IsPause { get; private set; } = false;
        public float CurrentTime
        {
            get { return CurrentClock; }
            set
            {
                CurrentClock = value;
                Source.time = Mathf.Clamp(value, 0, (CurrentClip == null) ? 0 : CurrentClip.length);
                delay = Mathf.Clamp(-value, 0, Mathf.Infinity);
                if (value < 0 && IsPlay)
                {
                    Stop();
                    Play();
                }
            }
        }
        public float CurrentDelay => (IsDelayToStart) ? delay : 0;

        [SerializeField] private AudioPostMixer _Mixer = null;
        public AudioPostMixer Mixer
        {
            get { return _Mixer; }
        }
        public bool IsHavePostMixer { get { return Mixer != null; } }
        public bool IsHaveMixer { get { return Mixer.Package.audioMixer != null; } }

        public bool LoopAtAll = true;
        public bool Sampling = false;

        [SerializeField] private LineRenderer _m_LineRenderer = null;
        [SerializeField] private LineRenderer LineRendererPrefab = null;

        #endregion

        #region Function

        private void Awake()
        {
            Source = GetComponent<AudioSource>();
            OnValidate();
            GetSampleCount();
        }

        private void Start()
        {
            AD.SceneSingleAssets.audioSourceControllers.Add(this);
            AD.SceneSingleAssets.translist.Add(this);
        }

        private void OnDestroy()
        {
            AD.SceneSingleAssets.audioSourceControllers.Remove(this);
            AD.SceneSingleAssets.audioSourceControllers.Remove(this);
        }

        private void OnValidate()
        {
            if (samples.Length != spectrumLength) samples = new float[spectrumLength];
            if (bands.Length != BandCount) bands = new float[BandCount];
            if (freqBands.Length != BandCount) freqBands = new float[BandCount];
            if (bandBuffers.Length != BandCount) bandBuffers = new float[BandCount];
            if (bufferDecrease.Length != BandCount) bufferDecrease = new float[BandCount];
            if (bandHighest.Length != BandCount) bandHighest = new float[BandCount];
            if (normalizedBands.Length != BandCount) normalizedBands = new float[BandCount];
            if (normalizedBandBuffers.Length != BandCount) normalizedBandBuffers = new float[BandCount];
            if (sampleCount.Length != BandCount) sampleCount = new int[BandCount];
        }

        private void Update()
        {
            while (CurrentClip == null && SourcePairs.Count > 0)
            {
                SourcePairs.Remove(CurrentSourcePair);
                CurrentPairIndex = 0;
                return;
            }
            if (SourcePairs.Count == 0)
                return;
            if (Source.clip == null && CurrentClip != null)
                Source.clip = CurrentClip;
            if (Sampling)
                WhenSampling();
            if (_m_LineRenderer != null && (!Sampling || !DrawingLine))
                _m_LineRenderer.gameObject.SetActive(false);
            if (IsNowPlaying)
                WhenPlaying();
            if (IsNowPlaying && IsDelayToStart)
                WhenDelayCounting();

            void WhenSampling()
            {
                GetSpectrums();
                GetFrequencyBands();
                GetNormalizedBands();
                GetBandBuffers(increasingType, decreasingType);
                BandNegativeCheck();
                if (DrawingLine)
                {
                    if (_m_LineRenderer == null)
                    {
                        _m_LineRenderer = GameObject.Instantiate(LineRendererPrefab, transform).gameObject.GetComponent<LineRenderer>();
                        _m_LineRenderer.name = name + " LineRenderer(AudioiSourceController)";
                    }
                    else _m_LineRenderer.gameObject.SetActive(true);
                    if (CurrentSourcePair.LineDrawer == null)
                    {
                        DrawLineOnDefault();
                    }
                    else CurrentSourcePair.LineDrawer.DrawLine(_m_LineRenderer, this);
                }
                else if (_m_LineRenderer != null) _m_LineRenderer.gameObject.SetActive(false);
            }

            void WhenPlaying()
            {
                WhenNeedUpdataCurrentClock();
                if (CurrentClock > CurrentClip.length + 0.2f)
                {
                    Source.Stop();
                    CurrentTime = 0;
                    if (LoopAtAll)
                    {
                        if (CurrentPairIndex < SourcePairs.Count - 1) CurrentPairIndex++;
                        else CurrentPairIndex = 0;
                        Source.clip = CurrentClip;
                        Source.Play();
                    }
                }

            }

            void WhenNeedUpdataCurrentClock()
            {
                if (!IsPause)
                {
                    //CurrentClock += UnityEngine.Time.deltaTime;
                    CurrentClock = (float)Source.timeSamples / (float)Source.clip.frequency;
                    /*
                    if (!IsDelayToStart)
                    {
                        float cat = Mathf.Abs(CurrentClock - Source.time);
                        if (cat > 0.025f)
                        {
                            CurrentClock = Source.time;
                            Debug.LogWarning("音频计时出现误差(" + cat.ToString() + " S)，已尝试同步");
                        }
                    }
                    */
                }
            }

            void WhenNeedUpdataDelay()
            {
                if (!IsPause) delay -= Time.deltaTime;
            }

            void WhenDelayCounting()
            {
                WhenNeedUpdataDelay();
                if (delay <= 0)
                {
                    IsDelayToStart = false;
                    Play();
                }
            }
        }

        public void DrawLineOnDefault()
        {
            int vextcount = normalizedBands.Length;
            Keyframe[] keyframes = new Keyframe[vextcount * 2];
            Vector3[] vexts = new Vector3[vextcount];
            vextcount = (int)BandCount;
            for (int i = 0; i < vextcount; i++)
                vexts[i] = transform.position +
                    10 * new Vector3(Mathf.Cos(i / (float)vextcount * 2 * Mathf.PI), Mathf.Sin(i / (float)vextcount * 2 * Mathf.PI));
            _m_LineRenderer.positionCount = vextcount;
            _m_LineRenderer.SetPositions(vexts);
            AnimationCurve m_curve = AnimationCurve.Linear(0, 1, 1, 1);
            for (int i = 0; i < vextcount; i++)
            {
                keyframes[i].time = i / (float)(vextcount * 2);
                keyframes[i].value = Mathf.Clamp(bands[i], 0.01f, 100);
                keyframes[^(i + 1)].time = 1 - i / (float)(vextcount * 2);
                keyframes[^(i + 1)].value = Mathf.Clamp(bands[i], 0.01f, 100);
            }
            m_curve.keys = keyframes;
            _m_LineRenderer.widthCurve = m_curve;
        }

        public static AudioSourceController Generate(string name = "New AudioSource", Transform parent = null, params System.Type[] components)
        {
            AudioSourceController source = new GameObject(name, components).AddComponent<AudioSourceController>();
            source.transform.parent = parent.transform;

            return source;
        }

        public void NextPair()
        {
            if (SourcePairs.Count == 0) return;
            if (CurrentPairIndex < SourcePairs.Count - 1) CurrentPairIndex++;
            else CurrentPairIndex = 0;
            bool curPlaying = Source.isPlaying;
            Stop();
            Refresh();
            if (curPlaying) Play();
        }
        public void PreviousPair()
        {
            if (SourcePairs.Count == 0) return;
            if (CurrentPairIndex > 0) CurrentPairIndex--;
            else CurrentPairIndex = SourcePairs.Count - 1;
            bool curPlaying = Source.isPlaying;
            Stop();
            Refresh();
            if (curPlaying) Play();
        }
        public void RandomPair()
        {
            if (SourcePairs.Count == 0) return;
            CurrentPairIndex = UnityEngine.Random.Range(0, SourcePairs.Count);
            bool curPlaying = Source.isPlaying;
            Stop();
            Refresh();
            if (curPlaying) Play();
        }
        public void SetPair(int index)
        {
            if (SourcePairs.Count == 0) return;
            CurrentPairIndex = Mathf.Clamp(index, 0, SourcePairs.Count - 1);
            bool curPlaying = Source.isPlaying;
            Stop();
            Refresh();
            if (curPlaying) Play();
        }
        public void SetPair(string name)
        {
            for (int i = 0, e = SourcePairs.Count; i < e; i++)
            {
                SourcePair item = SourcePairs[i];
                if (item.Name == name)
                {
                    Stop();
                    SetPair(i);
                    return;
                }
            }
        }

        public void Play()
        {
            IsPause = false;
            IsNowPlaying = true;
            if (delay > 0)
            {
                IsDelayToStart = true;
                return;
            }
            if (SourcePairs.Count == 0) return;
            Source.Play();
        }
        public void Stop()
        {
            IsPause = false;
            IsNowPlaying = false;
            Source.Stop();
            delay = 0;
            IsDelayToStart = false;
            CurrentClock = 0;
        }
        public void Pause()
        {
            IsPause = true;
            IsNowPlaying = false;
            Source.Pause();
            return;
        }
        public void PlayOrPause()
        {
            IsPlay = !IsPlay;
        }

        public void Play(string key)
        {
            SetPair(key);
            Play();
        }

        public void Refresh()
        {
            Source.clip = CurrentClip;
        }

        public void IgnoreListenerPause()
        {
            Source.ignoreListenerPause = true;
        }
        public void SubscribeListenerPause()
        {
            Source.ignoreListenerPause = false;
        }

        public void IgnoreListenerVolume()
        {
            Source.ignoreListenerVolume = true;
        }
        public void SubscribeListenerVolume()
        {
            Source.ignoreListenerVolume = false;
        }

        public void SetLoop()
        {
            Source.loop = true;
        }
        public void UnLoop()
        {
            Source.loop = false;
        }

        public void SetLoopAtAll()
        {
            LoopAtAll = true;
        }
        public void UnLoopAtAll()
        {
            LoopAtAll = false;
        }

        public void SetMute()
        {
            Source.mute = true;
        }
        public void CancelMute()
        {
            Source.mute = false;
        }

        public void SetPitch(float pitch)
        {
            Source.pitch = pitch;
        }

        public void SetSpeed(float speed)
        {
            if (_Mixer != null) _Mixer.SetSpeed(speed);
            else
            {
                Debug.LogWarning("you try to change an AudioSource's speed without AudioMixer, which will cause it to change its pitch");
                SetPitch(speed);
            }
        }
        public void AddSpeed(float value)
        {
            if (_Mixer != null) _Mixer.AddSpeed(value);
            else
            {
                Debug.LogWarning("you try to change an AudioSource's speed without AudioMixer, which will cause it to change its pitch");
                SetPitch(Source.pitch + value);
            }
        }

        public void SetVolume(float volume)
        {
            Source.volume = volume;
        }

        public void SetPriority(int priority)
        {
            Source.priority = priority;
        }

        public void RandomPairs()
        {
            SourcePairs.Sort((T, P) => { if (UnityEngine.Random.Range(-1, 1) > 0) return 1; else return -1; });
        }

        public void PrepareToOtherScene()
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(ClockOnJump());
        }

        private IEnumerator ClockOnJump()
        {
            for (float now = 0; now < 1; now += UnityEngine.Time.deltaTime)
            {
                this.SetVolume(1 - now);
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
        }

        #endregion

        #region Inspector
        [Header("MusicSampler")]
        public bool DrawingLine = false;
        /// <summary>
        /// 这个参数用于设置进行采样的精度
        /// </summary>
        [Tooltip("采样精度")] public SpectrumLength SpectrumCount = SpectrumLength.Spectrum256;
        private int spectrumLength => (int)Mathf.Pow(2, ((int)SpectrumCount + 6));
        /// <summary>
        /// 这个属性返回采样得到的原始数据
        /// </summary>
        [Tooltip("原始数据")] public float[] samples = new float[64];
        private int[] sampleCount = new int[8];
        /// <summary>
        /// 这个参数用于设置将采样的结果分为几组进行讨论
        /// </summary>
        [Tooltip("拆分组数")] public uint BandCount = 8;
        /// <summary>
        /// 这个参数用于设置组别采样数值减小时使用的平滑策略
        /// </summary>
        [Tooltip("平衡采样值滑落的平滑策略")] public BufferDecreasingType decreasingType = BufferDecreasingType.Jump;
        /// <summary>
        /// 这个参数用于设置在Slide和Falling设置下，组别采样数值减小时每帧下降的大小。
        /// </summary>
        [Tooltip("Slide/Falling:采样值的滑落幅度")] public float decreasing = 0.003f;
        /// <summary>
        /// 这个参数用于设置在Falling设置下，组别采样数值减小时每帧下降时加速度的大小。
        /// </summary>
        [Tooltip("Falling:采样值滑落的加速度")] public float DecreaseAcceleration = 0.2f;
        /// <summary>
        /// 这个参数用于设置组别采样数值增大时使用的平滑策略
        /// </summary>
        [Tooltip("平衡采样值提升的平滑策略")] public BufferIncreasingType increasingType = BufferIncreasingType.Jump;
        /// <summary>
        /// 这个参数用于设置在Slide设置下，组别采样数值增大时每帧增加的大小。
        /// </summary>
        [Tooltip("Slide:采样值的提升幅度")] public float increasing = 0.003f;
        /// <summary>
        /// 这个属性返回经过平滑和平均的几组数据
        /// </summary>
        [Tooltip("经处理后的数据")] public float[] bands = new float[8];
        private float[] freqBands = new float[8];
        private float[] bandBuffers = new float[8];
        private float[] bufferDecrease = new float[8];
        /// <summary>
        /// 这个属性返回总平均采样结果
        /// </summary>
        public float average
        {
            get
            {
                float average = 0;
                for (int i = 0; i < BandCount; i++)
                {
                    average += normalizedBands[i];
                }
                average /= BandCount;
                return average;
            }
        }

        private float[] bandHighest = new float[8];
        /// <summary>
        /// 这个属性返回经过平滑、平均和归一化的几组数据
        /// </summary>
        [Tooltip("经过平滑、平均和归一化的几组数据")] public float[] normalizedBands = new float[8];
        private float[] normalizedBandBuffers = new float[8];

        #endregion  

        #region Programs

        private void GetSampleCount()
        {
            float acc = (((float)((int)SpectrumCount + 6)) / BandCount);
            int sum = 0;
            int last = 0;
            for (int i = 0; i < BandCount - 1; i++)
            {
                int pow = (int)Mathf.Pow(2, acc * (i));
                sampleCount[i] = pow - sum;
                if (sampleCount[i] < last) sampleCount[i] = last;
                sum += sampleCount[i];
                last = sampleCount[i];
            }
            sampleCount[BandCount - 1] = samples.Length - sum;
        }

        private void GetSpectrums()
        {
            Source.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        }

        private void GetFrequencyBands()
        {
            int counter = 0;
            for (int i = 0; i < BandCount; i++)
            {
                float average = 0;
                for (int j = 0; j < sampleCount[i]; j++)
                {
                    average += samples[counter] * (counter + 1);
                    counter++;
                }
                average /= sampleCount[i];
                freqBands[i] = average * 10;
            }
        }

        private void GetNormalizedBands()
        {
            for (int i = 0; i < BandCount; i++)
            {
                if (freqBands[i] > bandHighest[i])
                {
                    bandHighest[i] = freqBands[i];
                }
            }
        }

        private void GetBandBuffers(BufferIncreasingType increasingType, BufferDecreasingType decreasingType)
        {
            for (int i = 0; i < BandCount; i++)
            {
                if (freqBands[i] > bandBuffers[i])
                {
                    switch (increasingType)
                    {
                        case BufferIncreasingType.Jump:
                            bandBuffers[i] = freqBands[i];
                            bufferDecrease[i] = decreasing;
                            break;
                        case BufferIncreasingType.Slide:
                            bufferDecrease[i] = decreasing;
                            bandBuffers[i] += increasing;
                            break;
                    }
                    if (freqBands[i] < bandBuffers[i]) bandBuffers[i] = freqBands[i];
                }
                if (freqBands[i] < bandBuffers[i])
                {
                    switch (decreasingType)
                    {
                        case BufferDecreasingType.Jump:
                            bandBuffers[i] = freqBands[i];
                            break;
                        case BufferDecreasingType.Falling:
                            bandBuffers[i] -= decreasing;
                            break;
                        case BufferDecreasingType.Slide:
                            bandBuffers[i] -= bufferDecrease[i];
                            bufferDecrease[i] *= 1 + DecreaseAcceleration;
                            break;
                    }
                    if (freqBands[i] > bandBuffers[i]) bandBuffers[i] = freqBands[i]; ;
                }
                bands[i] = bandBuffers[i];
                if (bandHighest[i] == 0) continue;
                normalizedBands[i] = (freqBands[i] / bandHighest[i]);
                normalizedBandBuffers[i] = (bandBuffers[i] / bandHighest[i]);
                if (normalizedBands[i] > normalizedBandBuffers[i])
                {
                    switch (increasingType)
                    {
                        case BufferIncreasingType.Jump:
                            normalizedBandBuffers[i] = normalizedBands[i];
                            bufferDecrease[i] = decreasing;
                            break;
                        case BufferIncreasingType.Slide:
                            bufferDecrease[i] = decreasing;
                            normalizedBandBuffers[i] += increasing;
                            break;
                    }
                    if (normalizedBands[i] < normalizedBandBuffers[i]) normalizedBandBuffers[i] = normalizedBands[i];
                }
                if (normalizedBands[i] < normalizedBandBuffers[i])
                {
                    switch (decreasingType)
                    {
                        case BufferDecreasingType.Jump:
                            normalizedBandBuffers[i] = normalizedBands[i];
                            break;
                        case BufferDecreasingType.Falling:
                            normalizedBandBuffers[i] -= decreasing;
                            break;
                        case BufferDecreasingType.Slide:
                            normalizedBandBuffers[i] -= bufferDecrease[i];
                            bufferDecrease[i] *= 1 + DecreaseAcceleration;
                            break;
                    }
                    if (normalizedBands[i] > normalizedBandBuffers[i]) normalizedBandBuffers[i] = normalizedBands[i];
                }
                normalizedBands[i] = normalizedBandBuffers[i];
            }
        }

        private void BandNegativeCheck()
        {
            for (int i = 0; i < BandCount; i++)
            {
                if (bands[i] < 0)
                {
                    bands[i] = 0;
                }
                if (normalizedBands[i] < 0)
                {
                    normalizedBands[i] = 0;
                }
            }
        }

        #endregion

        #region Resource

        public void LoadOnResource(string source, AudioType audioType, bool isCurrent = true)
        {
            string finalPath = Application.dataPath + "/Resources/" + source;
            StartCoroutine(LoadAudio(finalPath, audioType, isCurrent));
        }

        public void LoadOnUrl(string url, AudioType audioType, bool isCurrent = true)
        {
            StartCoroutine(LoadAudio(url, audioType, isCurrent));
        }

        public IEnumerator LoadAudio(string path, AudioType audioType, bool isCurrent)
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                if (isCurrent && SourcePairs.Count > 0)
                    CurrentClip = audioClip;
                else
                    SourcePairs.Add(new SourcePair() { CilpName = path, Name = path, Clip = audioClip });
                Refresh();
            }
            else
                Debug.LogError("Failed To Load " + request.result.ToString());
        }

        #endregion

        /// <summary>
        /// 通过这个函数来生成一个AudioSource,并初始化其播放的片段为audioClip
        /// </summary>
        /// <param name="audioClip">播放的片段</param>
        /// <returns></returns>
        public static AudioSource CreateSampler(AudioClip audioClip)
        {
            GameObject go = new GameObject("New AudioSource");
            AudioSource asr = go.AddComponent<AudioSource>();
            asr.clip = audioClip;
            asr.loop = false;
            asr.Play();
            return asr;
        }

        /// <summary>
        /// 传入一个AudioClip 会将AudioClip上挂载的音频文件生成频谱到一张Texture2D上
        /// </summary>
        /// <param name="_clip"></param>
        /// <param name="resolution">这个值可以控制频谱的密度</param>
        /// <param name="width">这个是最后生成的Texture2D图片的宽度</param>
        /// <param name="height">这个是最后生成的Texture2D图片的高度</param>
        /// <returns></returns>
        public static Texture2D BakeAudioWaveform(AudioClip _clip, int resolution = 60, int width = 1920, int height = 200)
        {
            resolution = _clip.frequency / resolution;

            float[] samples = new float[_clip.samples * _clip.channels];
            _clip.GetData(samples, 0);

            float[] waveForm = new float[(samples.Length / resolution)];

            float min = 0;
            float max = 0;
            bool inited = false;

            for (int i = 0; i < waveForm.Length; i++)
            {
                waveForm[i] = 0;

                for (int j = 0; j < resolution; j++)
                {
                    waveForm[i] += Mathf.Abs(samples[(i * resolution) + j]);
                }

                if (!inited)
                {
                    min = waveForm[i];
                    max = waveForm[i];
                    inited = true;
                }
                else
                {
                    if (waveForm[i] < min)
                    {
                        min = waveForm[i];
                    }

                    if (waveForm[i] > max)
                    {
                        max = waveForm[i];
                    }
                }
                //waveForm[i] /= resolution;
            }


            Color backgroundColor = Color.black;
            Color waveformColor = Color.green;
            Color[] blank = new Color[width * height];
            Texture2D texture = new Texture2D(width, height);

            for (int i = 0; i < blank.Length; ++i)
            {
                blank[i] = backgroundColor;
            }

            texture.SetPixels(blank, 0);

            float xScale = (float)width / (float)waveForm.Length;

            int tMid = (int)(height / 2.0f);
            float yScale = 1;

            if (max > tMid)
            {
                yScale = tMid / max;
            }

            for (int i = 0; i < waveForm.Length; ++i)
            {
                int x = (int)(i * xScale);
                int yOffset = (int)(waveForm[i] * yScale);
                int startY = tMid - yOffset;
                int endY = tMid + yOffset;

                for (int y = startY; y <= endY; ++y)
                {
                    texture.SetPixel(x, y, waveformColor);
                }
            }

            texture.Apply();
            return texture;
        }

        public IADUI Obtain(int serialNumber)
        {
            return ADUI.Items.Find((P) => P.SerialNumber == serialNumber);
        }

        public IADUI Obtain(string elementName)
        {
            return ADUI.Items.Find((P) => P.ElementName == elementName);
        }

        public void InitializeContext()
        {
            throw new NotImplementedException();
        }

        public static AudioType GetAudioType(string path)
        {
            return Path.GetExtension(path) switch
            {
                "wav" => AudioType.WAV,
                "mp3" => AudioType.MPEG,
                "ogg" => AudioType.OGGVORBIS,
                _ => AudioType.UNKNOWN
            };
        }
    }

    public enum SpectrumLength
    {
        Spectrum64, Spectrum128, Spectrum256, Spectrum512, Spectrum1024, Spectrum2048, Spectrum4096, Spectrum8192
    }

    public enum BufferDecreasingType
    {
        Jump, Slide, Falling
    }

    public enum BufferIncreasingType
    {
        Jump, Slide
    }

}
