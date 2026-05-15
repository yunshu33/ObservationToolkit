using NUnit.Framework;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Sample.AutoWeave;

namespace VoyageForge.ObservationToolkit.Tests.EditMode
{
    /// <summary>
    /// 自动织入系统的 EditMode 测试。
    /// 这些测试用于确认 Unity IL Post Processor 已经实际运行，而不只是程序集成功编译。
    /// </summary>
    public class AutoWeaveTests
    {
        /// <summary>
        /// 验证标记了 <see cref="ObservationAttribute"/> 的自动属性会在赋值时触发绑定通知。
        /// 如果 IL Post Processor 没有运行，Score 的自动属性 setter 不会调用 SetField，本测试会失败。
        /// </summary>
        [Test]
        public void ObservationAutoPropertyRaisesBindingNotification()
        {
            var model = new AutoWeavePlayerModel();
            var observedValue = -1;

            model.For(m => m.Score)
                .To(value => observedValue = value)
                .OneWay();

            model.Score = 42;

            Assert.AreEqual(42, observedValue);
        }
    }
}
