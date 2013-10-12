using RectanglePlacer.Biz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace TimelineControl.Model.Timeline.Generator
{
    /// <summary>
    /// タイムライン生成器の抽象基底クラス
    /// </summary>
    abstract public class TimelineGeneratorBase : ITimelineGenerator
    {
        /// <summary>
        /// 最上部
        /// </summary>
        protected double _minPos;

        /// <summary>
        /// 最下部
        /// </summary>
        protected double _maxPos;

        /// <summary>
        /// 横幅
        /// </summary>
        protected double _scaleWidth = 160;

        /// <summary>
        /// 縦軸のコレクション
        /// </summary>
        protected ICollection<TimelineAxis> _axisDataCollection;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="axis">縦軸のコレクション</param>
        /// <param name="scaleWidth">横幅</param>
        /// <param name="minPos">最上部</param>
        /// <param name="maxPos">最下部</param>
        public TimelineGeneratorBase(ICollection<TimelineAxis> axis, double scaleWidth, double minPos, double maxPos)
        {
            _axisDataCollection = axis;
            _scaleWidth = scaleWidth;
            _minPos = minPos;
            _maxPos = maxPos;
        }

        abstract public void GenerateBorders(System.Windows.Controls.Canvas canvas);

        abstract public void GenerateScale(System.Windows.Controls.Canvas canvas);
        abstract public void GenerateEvents(System.Windows.Controls.Canvas canvas, EventModelManager eventManager);


        #region イベントを登録する

        public abstract String GetOverView(EventBorderViewModel evt);

        public virtual Border GenerateGeneralEventBorder(Canvas canvas, Rect rect, EventBorderViewModel evt)
        {
            var brd = VacantBorder(1,1);
            brd.CornerRadius = new CornerRadius(2, 2, 2, 2);
            brd.DataContext = evt;
            brd.ToolTip = GetOverView(evt);

            brd.Width = rect.Width;
            brd.Height = rect.Height;

            Canvas.SetTop(brd, rect.Top);
            Canvas.SetLeft(brd, rect.Left);

            return brd;
        }

        #endregion

        #region Controlを作る
        protected TextBlock GetVacantTextBlock()
        {
            return new TextBlock()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                TextAlignment = System.Windows.TextAlignment.Left,
            };
        }

        protected TextBlock GetLargestTextBlock(string putText)
        {
            var block = GetVacantTextBlock();
            block.FontSize = 16;
            block.Foreground = Brushes.White;
            block.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            block.Text = putText;

            return block;
        }

        protected TextBlock GetMiddleTextBlock(string putText)
        {
            var block = GetVacantTextBlock();
            block.FontSize = 14;
            block.Foreground = Brushes.White;
            block.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            block.Text = putText;

            return block;
        }

        protected TextBlock GetSmallTextBlock(string putText)
        {
            var block = GetVacantTextBlock();
            block.FontSize = 12;
            block.Foreground = Brushes.White;
            block.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            block.Text = putText;

            return block;
        }

        protected Border VacantBorder(double width, double height)
        {
            return new Border()
            {
                Width = width,
                Height = height,
            };
        }



        public Border GenerateShadowEventBorder(Canvas canvas, Rect rect, EventBorderViewModel evt)
        {
            var brd = GenerateGeneralEventBorder(canvas, rect, evt);

            brd.Effect = new DropShadowEffect
            {
                Color = new Color { A = 255, R = 219, G = 219, B = 219 },
                Direction = 295,
                ShadowDepth = 5,
                Opacity = 5
            };

            return brd;
        }

        public Border AddEventBlock(Canvas canvas, Rect rect, EventBorderViewModel evt, bool isUnbound)
        {
            canvas.Children.Add(GenerateShadowEventBorder(canvas, rect, evt));

            var brd = GenerateGeneralEventBorder(canvas, rect, evt);

            canvas.Children.Add(brd);
            var grid = new Grid();

            brd.Child = grid;

            var blck = GetVacantTextBlock();

            blck.VerticalAlignment = VerticalAlignment.Top;
            blck.HorizontalAlignment = HorizontalAlignment.Center;
            blck.Text = evt.Title;
            grid.Children.Add(blck);

            if (isUnbound)
            {
                brd.Style = canvas.FindResource("UnboundEventItemBorder") as Style;
            }
            else
            {
                TextBlock deletableTextBlock = new TextBlock();
                deletableTextBlock.Style = canvas.FindResource("DeletableTextBlock") as Style;
                brd.Style = canvas.FindResource("EventItemBorder") as Style;
                grid.Children.Add(deletableTextBlock);
            }

            return brd;
        }

        public void GenerateEventBorders(Canvas canvas, List<PlacableRect> placableList, double minLeft, double width, bool isUnbounded)
        {
            // ソートしておく
            placableList.Sort((x, y) =>
            {
                if (x.rect.Y == y.rect.Y)
                {
                    return (int)(y.rect.Height - x.rect.Height);
                }
                return (int)(x.rect.Y - y.rect.Y);
            });

            // EventBorderのプラスマイナスはMargin分
            RectPlacer rectPlacer = new RectPlacer(minLeft + 2, minLeft + width - 10);

            rectPlacer.Place(placableList);

            foreach (var place in placableList)
            {
                AddEventBlock(canvas, place.rect, place.source as EventBorderViewModel, isUnbounded);
            }
        }

        #endregion

        #region 縦軸関係のデータを取得する
        protected TimelineAxis GetAxis(int id)
        {
            return _axisDataCollection.Where(item => item.Id == id).First();
        }

        protected double GetAxisLeft(int id)
        {
            double currentLeft = 0.0;
            foreach (var axis in _axisDataCollection)
            {
                if (axis.IsDisplayed == false)
                {
                    continue;
                }
                if (axis.Id == id)
                {
                    return currentLeft;
                }

                currentLeft += axis.Width;
            }

            return 0.0;
        }

        #endregion
    }
}
