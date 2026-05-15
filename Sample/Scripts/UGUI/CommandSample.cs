using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Command;
using VoyageForge.ObservationToolkit.Runtime.UGUI;
using VoyageForge.ObservationToolkit.Runtime.ViewModel;

namespace VoyageForge.ObservationToolkit.Sample.UGUI
{
    /// <summary>
    /// Command 绑定示例面板。
    /// 这个脚本演示 View 层只负责绑定按钮，真正的点击逻辑和可执行条件都放在 ViewModel 的 ICommand 中。
    /// </summary>
    public class CommandSample : MonoBehaviour
    {
        /// <summary>
        /// 示例 ViewModel。实际项目里通常由页面、窗口或依赖注入流程创建。
        /// </summary>
        public CommandSampleViewModel model = new();

        /// <summary>
        /// 显示当前分数的文本控件。
        /// </summary>
        [Header("Text")]
        public TMP_Text scoreText;

        /// <summary>
        /// 显示最近一次命令执行结果或页面状态的文本控件。
        /// </summary>
        public TMP_Text statusText;

        /// <summary>
        /// 执行加分命令的按钮。
        /// </summary>
        [Header("Command Buttons")]
        public Button addScoreButton;

        /// <summary>
        /// 执行扣除 5 分命令的按钮。
        /// </summary>
        public Button spendFiveButton;

        /// <summary>
        /// 执行扣除 20 分命令的按钮。
        /// </summary>
        public Button spendTwentyButton;

        /// <summary>
        /// 执行分数重置命令的按钮。
        /// </summary>
        public Button resetButton;

        /// <summary>
        /// 执行切换到 Casual 模式命令的按钮。
        /// </summary>
        public Button casualModeButton;

        /// <summary>
        /// 执行切换到 Hard 模式命令的按钮。
        /// </summary>
        public Button hardModeButton;

        /// <summary>
        /// Unity 生命周期入口。
        /// 在示例启动时完成文本绑定、命令绑定，并主动刷新一次 ViewModel 状态。
        /// </summary>
        private void Start()
        {
            BindTexts();
            BindCommands();

            // OneWay 绑定只在属性变化时推送；示例启动时主动刷新一次，让 UI 立即显示当前 ViewModel 状态。
            model.RefreshAll();
        }

        /// <summary>
        /// 绑定文本显示。
        /// </summary>
        private void BindTexts()
        {
            if (scoreText != null)
            {
                model.For(m => m.Score)
                    .To<string>(text => scoreText.text = text)
                    .OneWay(value => $"Score: {value}")
                    .AddTo(this);
            }

            if (statusText != null)
            {
                model.For(m => m.Status)
                    .To<string>(text => statusText.text = text)
                    .OneWay()
                    .AddTo(this);
            }
        }

        /// <summary>
        /// 绑定按钮命令。
        /// 同一个 ICommand 可以绑定到多个 Button，并通过不同参数表达不同操作。
        /// </summary>
        private void BindCommands()
        {
            if (addScoreButton != null)
            {
                model.For(m => m.AddScoreCommand)
                    .To(addScoreButton)
                    .OneWay()
                    .AddTo(this);
            }

            if (spendFiveButton != null)
            {
                model.For(m => m.SpendScoreCommand)
                    .To(spendFiveButton, 5)
                    .OneWay()
                    .AddTo(this);
            }

            if (spendTwentyButton != null)
            {
                model.For(m => m.SpendScoreCommand)
                    .To(spendTwentyButton, 20)
                    .OneWay()
                    .AddTo(this);
            }

            if (resetButton != null)
            {
                resetButton.BindCommand(model, m => m.ResetCommand);
            }

            if (casualModeButton != null)
            {
                casualModeButton.BindCommand(model, m => m.SetModeCommand, "Casual");
            }

            if (hardModeButton != null)
            {
                hardModeButton.BindCommand(model, m => m.SetModeCommand, "Hard");
            }
        }
    }

    /// <summary>
    /// Command 示例数据。
    /// 数据类只保存状态，不放按钮点击逻辑。
    /// </summary>
    [Serializable]
    public class CommandSampleData
    {
        /// <summary>
        /// 当前示例分数。
        /// </summary>
        public int score;

        /// <summary>
        /// 最近一次命令执行结果或页面状态文案。
        /// </summary>
        public string status = "Ready";

        /// <summary>
        /// 当前示例模式名称。
        /// </summary>
        public string mode = "Casual";
    }

    /// <summary>
    /// Command 示例 ViewModel。
    /// ViewModel 暴露可绑定状态和命令，按钮能否点击由命令的 CanExecute 决定。
    /// </summary>
    [Serializable]
    public class CommandSampleViewModel : ViewModel<CommandSampleData>
    {
        /// <summary>
        /// 当前分数。分数变化会影响 Add 和 Spend 命令的可执行状态。
        /// </summary>
        public int Score
        {
            get => Data.score;
            set => this.SetField(ref Data.score, value);
        }

        /// <summary>
        /// 页面状态文案。
        /// </summary>
        public string Status
        {
            get => Data.status;
            set => this.SetField(ref Data.status, value);
        }

        /// <summary>
        /// 当前模式。
        /// </summary>
        public string Mode
        {
            get => Data.mode;
            set => this.SetField(ref Data.mode, value);
        }

        /// <summary>
        /// 加分命令。Score 达到 100 后按钮会自动不可点击。
        /// </summary>
        private ICommand _addScoreCommand;

        /// <summary>
        /// 加分命令实例。
        /// 该命令依赖 <see cref="Score"/>，分数变化后会通过 Observes 自动刷新按钮可点击状态。
        /// </summary>
        public ICommand AddScoreCommand => _addScoreCommand ??= new RelayCommand(
            execute: () =>
            {
                Score += 10;
                Status = $"Added 10 points. Mode={Mode}";
            },
            canExecute: () => Score < 100)
            .Observes(this, m => m.Score);

        /// <summary>
        /// 扣分命令。通过参数决定扣多少分，同一条命令可以复用给多个按钮。
        /// </summary>
        private ICommand _spendScoreCommand;

        /// <summary>
        /// 扣分命令实例。
        /// 该命令的参数表示扣除分数，并依赖 <see cref="Score"/> 自动刷新可执行状态。
        /// </summary>
        public ICommand SpendScoreCommand => _spendScoreCommand ??= new RelayCommand<int>(
            execute: amount =>
            {
                Score -= amount;
                Status = $"Spent {amount} points. Mode={Mode}";
            },
            canExecute: amount => amount > 0 && Score >= amount)
            .Observes(this, m => m.Score);

        /// <summary>
        /// 重置命令。
        /// </summary>
        private ICommand _resetCommand;

        /// <summary>
        /// 重置分数的命令实例。
        /// 该命令始终可执行，因此不需要登记 Observes 依赖。
        /// </summary>
        public ICommand ResetCommand => _resetCommand ??= new RelayCommand(
            execute: () =>
            {
                Score = 0;
                Status = "Reset score.";
            });

        /// <summary>
        /// 设置模式命令。参数来自按钮绑定时传入的字符串。
        /// </summary>
        private ICommand _setModeCommand;

        /// <summary>
        /// 设置模式的命令实例。
        /// 该命令只依赖按钮传入的模式参数，不依赖 ViewModel 属性变化。
        /// </summary>
        public ICommand SetModeCommand => _setModeCommand ??= new RelayCommand<string>(
            execute: mode =>
            {
                Mode = mode;
                Status = $"Mode changed to {Mode}.";
            },
            canExecute: mode => !string.IsNullOrWhiteSpace(mode));

        /// <summary>
        /// 主动刷新所有可绑定状态。
        /// 示例启动时调用一次，真实项目也可以在加载存档或重置页面时调用。
        /// </summary>
        public void RefreshAll()
        {
            ((IObservable)this).OnPropertyChanged(Score, nameof(Score));
            ((IObservable)this).OnPropertyChanged(Status, nameof(Status));
            ((IObservable)this).OnPropertyChanged(Mode, nameof(Mode));
        }
    }
}
