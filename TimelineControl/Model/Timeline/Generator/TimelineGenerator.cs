using RectanglePlacer.Biz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TimelineControl.Model.Timeline.Generator;

namespace TimelineControl.Model
{
    public class TimelineGenerator : TimelineGeneratorBase
    {
        private double _borderHeight;

        private TimeRangeCollection TimeRangeCollection;
        private DateTimeAndPosConverter _timePosConverter;

        public TimelineGenerator(ICollection<TimelineAxis> axis, TimeRangeCollection allies, double scaleWidth, double minPos, double maxPos) :
            base(axis, scaleWidth, minPos, maxPos)
        {
            TimeRangeCollection = allies;

            _timePosConverter = new DateTimeAndPosConverter(minPos, maxPos, new TimeRange()
            {
                StartDateTime = TimeRangeCollection.First().StartDateTime,
                EndDateTime = TimeRangeCollection.Last().EndDateTime
            });
        }


        private Border GenerateBorder(Canvas canvas, TimeBorderViewModel model, double x, double y, double width, bool isUnbound)
        {
            var border = VacantBorder(width, _borderHeight);
            border.DataContext = model;
            if (isUnbound)
            {
                border.Style = canvas.FindResource("UnboundBorder") as Style;
            }

            Canvas.SetTop(border, y);
            Canvas.SetLeft(border, x);
            canvas.Children.Add(border);

            return border;
        }

        private void MoveToTimePosition(ref double x, ref double y, TimeRange range)
        {
            y = _timePosConverter.ConvertToPos(range.StartDateTime);
            _borderHeight = _timePosConverter.ConvertToPos(range.EndDateTime) - y;
        }

        private void MoveToNextObject(ref double x, ref double y, double width)
        {
            x += width;
        }

        public override void GenerateBorders(Canvas canvas)
        {
            double currentTop = 0;
            double currentLeft = 0;

            foreach (var obj in _axisDataCollection)
            {
                if (obj.IsDisplayed == false)
                {
                    continue;
                }
                foreach (var time in TimeRangeCollection)
                {
                    var tbModel = new TimeBorderViewModel()
                    {
                        StartDateTime = time.StartDateTime,
                        EndDateTime = time.EndDateTime,
                        SourceObject = obj
                    };

                    tbModel.MyBrush = obj.DrawBrush;
                    MoveToTimePosition(ref currentLeft, ref currentTop, time);

                    GenerateBorder(canvas, tbModel, currentLeft, currentTop, obj.Width, obj.IsUnbound);                    
                }

                MoveToNextObject(ref currentLeft, ref currentTop, obj.Width);
            }
        }

        #region 横軸を作る
        private string GetLargestText(TimeRange range)
        {
            switch (TimeRangeCollection.Kind)
            {
                case TimeRangeDivideKind.Sec30:
                    return range.StartDateTime.ToString("HH時");
                case TimeRangeDivideKind.Min1:
                case TimeRangeDivideKind.Min2:
                case TimeRangeDivideKind.Min5:
                case TimeRangeDivideKind.Min15:
                case TimeRangeDivideKind.Min30:
                    return range.StartDateTime.ToString("dd日(ddd)");
                case TimeRangeDivideKind.Hour1:
                case TimeRangeDivideKind.Hour2:
                case TimeRangeDivideKind.Hour4:
                case TimeRangeDivideKind.Hour8:
                    return range.StartDateTime.ToString("MM月");
                case TimeRangeDivideKind.Day1:
                case TimeRangeDivideKind.Day2:
                case TimeRangeDivideKind.MonthHalf:
                    return range.StartDateTime.ToString("yy年");
            }
            return "";
        }

        private string GetMiddleText(TimeRange range)
        {
            switch (TimeRangeCollection.Kind)
            {
                case TimeRangeDivideKind.Sec30:
                    return range.StartDateTime.ToString("mm分");
                case TimeRangeDivideKind.Min1:
                case TimeRangeDivideKind.Min2:
                case TimeRangeDivideKind.Min5:
                case TimeRangeDivideKind.Min15:
                case TimeRangeDivideKind.Min30:
                    return range.StartDateTime.ToString("HH時");
                case TimeRangeDivideKind.Hour1:
                case TimeRangeDivideKind.Hour2:
                case TimeRangeDivideKind.Hour4:
                case TimeRangeDivideKind.Hour8:
                    return range.StartDateTime.ToString("dd日(ddd)");
                case TimeRangeDivideKind.Day1:
                case TimeRangeDivideKind.Day2:
                case TimeRangeDivideKind.MonthHalf:
                    return range.StartDateTime.ToString("MM月");
            }
            return "";
        }


        private string GetSmallText(TimeRange range)
        {
            switch (TimeRangeCollection.Kind)
            {
                case TimeRangeDivideKind.Sec30:
                    return range.StartDateTime.ToString("ss秒");
                case TimeRangeDivideKind.Min1:
                case TimeRangeDivideKind.Min2:
                case TimeRangeDivideKind.Min5:
                case TimeRangeDivideKind.Min15:
                case TimeRangeDivideKind.Min30:
                    return range.StartDateTime.ToString("mm分");
                case TimeRangeDivideKind.Hour1:
                case TimeRangeDivideKind.Hour2:
                case TimeRangeDivideKind.Hour4:
                case TimeRangeDivideKind.Hour8:
                    return range.StartDateTime.ToString("HH時");
                case TimeRangeDivideKind.Day1:
                case TimeRangeDivideKind.Day2:
                case TimeRangeDivideKind.MonthHalf:
                    return range.StartDateTime.ToString("dd日(ddd)");
            }
            return "";
        }

        private TextBlock GetLargestTextBlock(TimeRange prevRange, TimeRange currentRange)
        {
            var largestText = GetLargestText(currentRange);
            if (prevRange == null || GetLargestText(prevRange) != largestText)
            {
                return GetLargestTextBlock(largestText);
            }

            return GetVacantTextBlock();
        }

        private TextBlock GetMiddleTextBlock(TimeRange prevRange, TimeRange currentRange)
        {
            var middleText = GetMiddleText(currentRange);
            if (prevRange == null || GetMiddleText(prevRange) != middleText)
            {
                return GetMiddleTextBlock(middleText);
            }

            return GetVacantTextBlock();
        }

        private TextBlock GetSmallTextBlock(TimeRange prevRange, TimeRange currentRange)
        {
            var smallText = GetSmallText(currentRange);
            if (prevRange == null || GetSmallText(prevRange) != smallText)
            {
                return GetSmallTextBlock(smallText);
            }

            return GetVacantTextBlock();
        }

        public UIElement GenerateScaleControl(TimeRange prevRange, TimeRange currentRange)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Right;
            panel.Children.Add(GetLargestTextBlock(prevRange, currentRange));
            panel.Children.Add(GetMiddleTextBlock(prevRange, currentRange));
            panel.Children.Add(GetSmallTextBlock(prevRange, currentRange));

            return panel;
        }

        public override void GenerateScale(Canvas canvas)
        {
            double currentTop = 0;
            double currentLeft = 0;

            TimeRange prev = null;
            foreach (var time in TimeRangeCollection)
            {
                var tbModel = new TimeBorderViewModel()
                {
                    StartDateTime = time.StartDateTime,
                    EndDateTime = time.EndDateTime,
                    SourceObject = null
                };
                MoveToTimePosition(ref currentLeft, ref currentTop, time);
                var border = GenerateBorder(canvas, tbModel, currentLeft, currentTop, _scaleWidth, false);

                border.Child = GenerateScaleControl(prev, time);

                prev = time;
            }
        }

        #endregion

        #region event系


        public double GetTop(DateTime time)
        {
            return Math.Max(_timePosConverter.ConvertToPos(time), _minPos);
        }

        public double GetHeight(DateTime startTime, DateTime endTime)
        {
            var height = Math.Min(_timePosConverter.ConvertToPos(endTime) - GetTop(startTime), _maxPos);
            return Math.Max(height, 0.0);
        }

        public override String GetOverView(EventBorderViewModel evt)
        {
            StringBuilder build = new StringBuilder();
            build.AppendLine("イベント名：" + evt.Title);
            build.AppendLine("開始日時:" + evt.Parent.StartDateTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss"));
            build.AppendLine("終了日時:" + evt.Parent.EndDateTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss"));
            build.AppendLine("【参加者】");

            foreach (var id in evt.Parent.Participants)
            {
                var axis = GetAxis(id);
                if (!axis.IsUnbound)
                {
                    build.AppendLine(axis.HeaderName);
                }
            }

            return build.ToString();
        }

        public override void GenerateEvents(Canvas canvas, EventModelManager eventManager)
        {
            var dictionary = eventManager.GetEventModel(TimeRangeCollection.First().StartDateTime, TimeRangeCollection.Last().EndDateTime);

            foreach (var axis in _axisDataCollection)
            {
                if (axis.IsDisplayed == false)
                {
                    continue;
                }

                if (!dictionary.ContainsKey(axis.Id))
                {
                    continue;
                }

                var placableRectList = new List<PlacableRect>();

                foreach (var evModl in dictionary[axis.Id])
                {
                    var top = GetTop(evModl.StartDateTime);
                    var height = GetHeight(evModl.StartDateTime, evModl.EndDateTime.AddSeconds(-1));

                    var viewModel = new EventBorderViewModel() { Parent = evModl, MyBrush = axis.DrawBrush, Title = evModl.Title };
                    placableRectList.Add(new PlacableRect(){rect = new Rect(0, top, 0, height), source = viewModel});

                }
                GenerateEventBorders(canvas, placableRectList, GetAxisLeft(axis.Id), axis.Width, axis.IsUnbound);
            }
        }

        #endregion
    }
}
