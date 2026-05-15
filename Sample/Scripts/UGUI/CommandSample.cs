using System;
using UnityEngine;
using UnityEngine.UI;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Command;
using Voyage.ObservationToolkit.Runtime.UGUI;
using Voyage.ObservationToolkit.Runtime.ViewModel;

namespace Voyage.ObservationToolkit.Sample.UGUI
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

        [Header("Text")]
        public Text scoreText;
        public Text statusText;

        [Header("Command Buttons")]
        public Button addScoreButton;
        public Button spendFiveButton;
        public Button spendTwentyButton;
        public Button resetButton;
        public Button casualModeButton;
        public Button hardModeButton;

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
                    .To(scoreText)
                    .OneWay(value => $"Score: {value}")
                    .AddTo(this);
            }

            if (statusText != null)
            {
                model.For(m => m.Status)
                    .To(statusText)
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
        public int score;
        public string status = "Ready";
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
            set
            {
                if (this.SetField(ref Data.score, value))
                {
                    RaiseCommandStates();
                }
            }
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

        public ICommand AddScoreCommand => _addScoreCommand ??= new RelayCommand(
            execute: () =>
            {
                Score += 10;
                Status = $"Added 10 points. Mode={Mode}";
            },
            canExecute: () => Score < 100);

        /// <summary>
        /// 扣分命令。通过参数决定扣多少分，同一条命令可以复用给多个按钮。
        /// </summary>
        private ICommand _spendScoreCommand;

        public ICommand SpendScoreCommand => _spendScoreCommand ??= new RelayCommand<int>(
            execute: amount =>
            {
                Score -= amount;
                Status = $"Spent {amount} points. Mode={Mode}";
            },
            canExecute: amount => amount > 0 && Score >= amount);

        /// <summary>
        /// 重置命令。
        /// </summary>
        private ICommand _resetCommand;

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
            RaiseCommandStates();
        }

        /// <summary>
        /// 通知所有命令重新计算 CanExecute。
        /// 当影响按钮可点击状态的数据变化时，需要调用这个方法。
        /// </summary>
        private void RaiseCommandStates()
        {
            _addScoreCommand?.RaiseCanExecuteChanged();
            _spendScoreCommand?.RaiseCanExecuteChanged();
            _resetCommand?.RaiseCanExecuteChanged();
            _setModeCommand?.RaiseCanExecuteChanged();
        }
    }
}
