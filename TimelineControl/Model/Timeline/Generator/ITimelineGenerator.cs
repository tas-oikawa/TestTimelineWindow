using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TimelineControl.Model.Timeline.Generator
{
    /// <summary>
    /// タイムライン生成器のインターフェース
    /// </summary>
    public interface ITimelineGenerator
    {
        /// <summary>
        /// 枠を作る
        /// </summary>
        /// <param name="canvas">描画するためのCanvas</param>
        void GenerateBorders(Canvas canvas);

        /// <summary>
        /// 横軸を作る
        /// </summary>
        /// <param name="canvas">描画するためのCanvas</param>
        void GenerateScale(Canvas canvas);

        /// <summary>
        /// イベントを作る
        /// </summary>
        /// <param name="canvas">描画するためのCanvas</param>
        /// <param name="eventManager">描画するイベントのマネージャー</param>
        void GenerateEvents(Canvas canvas, EventModelManager eventManager);
    }
}
