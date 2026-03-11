using System.Linq;
using Voyage.ObservationToolkit.Runtime;
using Mono.Cecil;

namespace Voyage.ObservationToolkit.Editor
{
    /// <summary>
    /// 编织器辅助工具
    /// </summary>
    public static class WeaverHelper
    {
        /// <summary>
        /// 判断是否应该忽略属性编织
        /// </summary>
        /// <param name="prop">属性定义</param>
        /// <param name="isClassObservable">类是否被标记为 Observation</param>
        /// <returns>是否忽略</returns>
        public static bool ShouldIgnoreObservation(PropertyDefinition prop, bool isClassObservable)
        {
            // 忽略 BindingHandler 属性
            if (prop.PropertyType.FullName == typeof(BindingHandler).FullName)
                return true;

            // 1. 如果属性显式标记了 IgnoreObservation，则忽略
            if (prop.CustomAttributes.Any(a => a.AttributeType.Name == nameof(IgnoreObservationAttribute)))
                return true;

            // 2. 如果属性显式标记了 Observation，则必须处理
            if (prop.CustomAttributes.Any(a => a.AttributeType.Name == nameof(ObservationAttribute)))
                return false;

            // 3. 如果类标记了 Observation，则默认处理所有属性（除非被忽略）
            if (isClassObservable)
                return false;

            // 4. 如果都不满足，则忽略（即：类没标记，属性也没标记）
            return true;
        }
    }
}
