using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.ViewModel;

namespace VoyageForge.ObservationToolkit.Sample.AutoWeave
{
    /// <summary>
    /// 自动织入示例面板。
    /// 该脚本演示如何使用 <see cref="ObservationAttribute"/> 让 IL Weaver 自动为属性 setter 注入通知逻辑，
    /// 从而在业务代码中不再手写 <c>SetField</c>。
    /// </summary>
    public class AutoWeaveSample : MonoBehaviour
    {
        /// <summary>
        /// 使用类级 <see cref="ObservationAttribute"/> 的普通模型示例。
        /// 类级标记会让 Weaver 默认处理该类型中的可处理属性。
        /// </summary>
        public AutoWeavePlayerModel playerModel = new();

        /// <summary>
        /// 使用 <see cref="ViewModel{TData}"/> 自动代理织入的 ViewModel 示例。
        /// Weaver 会把同名属性代理到 Data 字段，并注入属性变化通知。
        /// </summary>
        public AutoWeaveSettingsViewModel settingsModel = new();

        /// <summary>
        /// 显示分数的文本控件。
        /// </summary>
        public TMP_Text scoreText;

        /// <summary>
        /// 显示等级的文本控件。
        /// </summary>
        public TMP_Text levelText;

        /// <summary>
        /// 显示音量的文本控件。
        /// </summary>
        public TMP_Text volumeText;

        /// <summary>
        /// 显示困难模式状态的文本控件。
        /// </summary>
        public TMP_Text hardModeText;

        /// <summary>
        /// 执行加分操作的按钮。
        /// </summary>
        public Button addScoreButton;

        /// <summary>
        /// 执行提高音量操作的按钮。
        /// </summary>
        public Button increaseVolumeButton;

        /// <summary>
        /// 执行切换困难模式操作的按钮。
        /// </summary>
        public Button toggleHardModeButton;

        /// <summary>
        /// Unity 生命周期入口。
        /// 在示例启动时建立绑定和按钮事件，并主动刷新一次当前状态。
        /// </summary>
        private void Start()
        {
            BindModelTexts();
            BindButtons();
            RefreshAll();
        }

        /// <summary>
        /// 建立模型属性到文本控件的绑定。
        /// 这些属性的 setter 不手写 SetField，通知由 IL Weaver 在编译后注入。
        /// </summary>
        private void BindModelTexts()
        {
            if (scoreText != null)
            {
                playerModel.For(m => m.Score)
                    .To<string>(text => scoreText.text = text)
                    .OneWay(value => $"Score: {value}")
                    .AddTo(this);
            }

            if (levelText != null)
            {
                playerModel.For(m => m.Level)
                    .To<string>(text => levelText.text = text)
                    .OneWay(value => $"Level: {value}")
                    .AddTo(this);
            }

            if (volumeText != null)
            {
                settingsModel.For(m => m.Volume)
                    .To<string>(text => volumeText.text = text)
                    .OneWay(value => $"Volume: {value:0.0}")
                    .AddTo(this);
            }

            if (hardModeText != null)
            {
                settingsModel.For(m => m.HardMode)
                    .To<string>(text => hardModeText.text = text)
                    .OneWay(value => $"Hard Mode: {value}")
                    .AddTo(this);
            }
        }

        /// <summary>
        /// 建立示例按钮事件。
        /// 按钮点击只修改属性，UI 刷新由自动织入的属性通知驱动。
        /// </summary>
        private void BindButtons()
        {
            if (addScoreButton != null)
            {
                addScoreButton.onClick.AddListener(AddScore);
            }

            if (increaseVolumeButton != null)
            {
                increaseVolumeButton.onClick.AddListener(IncreaseVolume);
            }

            if (toggleHardModeButton != null)
            {
                toggleHardModeButton.onClick.AddListener(ToggleHardMode);
            }
        }

        /// <summary>
        /// 增加玩家分数。
        /// 该方法只对自动属性赋值，不直接触发绑定通知。
        /// </summary>
        private void AddScore()
        {
            playerModel.Score += 10;
            playerModel.Level = playerModel.Score / 50 + 1;
            playerModel.RuntimeOnlyChangeCount++;
        }

        /// <summary>
        /// 提高设置音量。
        /// 该属性由 ViewModel Weaver 代理到 Data.volume 并自动注入通知。
        /// </summary>
        private void IncreaseVolume()
        {
            settingsModel.Volume = Mathf.Clamp01(settingsModel.Volume + 0.1f);
        }

        /// <summary>
        /// 切换困难模式。
        /// 该属性由 ViewModel Weaver 代理到 Data.hardMode 并自动注入通知。
        /// </summary>
        private void ToggleHardMode()
        {
            settingsModel.HardMode = !settingsModel.HardMode;
        }

        /// <summary>
        /// 主动刷新所有示例绑定。
        /// OneWay 绑定只在属性变化后推送，示例启动时调用一次以显示初始值。
        /// </summary>
        private void RefreshAll()
        {
            ((IObservable)playerModel).OnPropertyChanged(playerModel.Score, nameof(AutoWeavePlayerModel.Score));
            ((IObservable)playerModel).OnPropertyChanged(playerModel.Level, nameof(AutoWeavePlayerModel.Level));
            ((IObservable)settingsModel).OnPropertyChanged(settingsModel.Volume, nameof(AutoWeaveSettingsViewModel.Volume));
            ((IObservable)settingsModel).OnPropertyChanged(settingsModel.HardMode, nameof(AutoWeaveSettingsViewModel.HardMode));
        }
    }

    /// <summary>
    /// 自动织入普通模型示例。
    /// 类级 <see cref="ObservationAttribute"/> 表示该类中的属性默认都参与织入，除非被 <see cref="IgnoreObservationAttribute"/> 排除。
    /// </summary>
    [Serializable]
    [Observation]
    public class AutoWeavePlayerModel : IObservable
    {
        /// <summary>
        /// 绑定处理器。
        /// 该属性由 <see cref="IObservable"/> 使用，并已通过接口上的 <see cref="IgnoreObservationAttribute"/> 避免递归织入。
        /// </summary>
        public BindingHandler BindingHandler { get; set; }

        /// <summary>
        /// 玩家分数。
        /// 这是自动属性，IL Weaver 会把 setter 改写为带通知的写入逻辑。
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 玩家等级。
        /// 该属性会因为类级 <see cref="ObservationAttribute"/> 自动参与织入。
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// 运行时变化计数。
        /// 该属性被显式忽略，变化时不会触发绑定通知。
        /// </summary>
        [IgnoreObservation]
        public int RuntimeOnlyChangeCount { get; set; }
    }

    /// <summary>
    /// 自动织入 ViewModel 数据示例。
    /// 字段名用于展示 ViewModel Weaver 的同名字段代理能力。
    /// </summary>
    [Serializable]
    public class AutoWeaveSettingsData
    {
        /// <summary>
        /// 音量数据字段。
        /// ViewModel 上的 Volume 属性会代理到该字段。
        /// </summary>
        public float volume = 0.5f;

        /// <summary>
        /// 困难模式数据字段。
        /// ViewModel 上的 HardMode 属性会代理到该字段。
        /// </summary>
        public bool hardMode;
    }

    /// <summary>
    /// 自动织入 ViewModel 示例。
    /// 类级 <see cref="ObservationAttribute"/> 会让 Weaver 为同名 Data 字段生成代理属性逻辑和通知。
    /// </summary>
    [Serializable]
    [Observation]
    public class AutoWeaveSettingsViewModel : ViewModel<AutoWeaveSettingsData>
    {
        /// <summary>
        /// 音量属性。
        /// Weaver 会代理到 <see cref="AutoWeaveSettingsData.volume"/> 并注入通知。
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// 困难模式属性。
        /// Weaver 会代理到 <see cref="AutoWeaveSettingsData.hardMode"/> 并注入通知。
        /// </summary>
        public bool HardMode { get; set; }
    }
}
