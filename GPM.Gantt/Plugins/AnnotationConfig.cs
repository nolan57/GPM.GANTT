using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace GPM.Gantt.Plugins
{
    /// <summary>
    /// Base annotation configuration implementation
    /// </summary>
    public class AnnotationConfig : IAnnotationConfig, INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = string.Empty;
        private AnnotationType _type;
        private double _x;
        private double _y;
        private double _width = 100;
        private double _height = 30;
        private string _data = string.Empty;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public AnnotationType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Text annotation configuration
    /// </summary>
    public class TextAnnotationConfig : AnnotationConfig
    {
        public string Text { get; set; } = "New text annotation";
        public string FontFamily { get; set; } = "Microsoft YaHei";
        public double FontSize { get; set; } = 12;
        public string FontWeight { get; set; } = "Normal";
        public string Color { get; set; } = "#FF000000";
        public string BackgroundColor { get; set; } = "#00FFFFFF";
        public double Opacity { get; set; } = 1.0;
        public string HorizontalAlignment { get; set; } = "Left";
        public string VerticalAlignment { get; set; } = "Top";

        public TextAnnotationConfig()
        {
            Type = AnnotationType.Text;
            Name = "Text Annotation";
        }
    }

    /// <summary>
    /// Shape annotation configuration
    /// </summary>
    public class ShapeAnnotationConfig : AnnotationConfig
    {
        public string ShapeType { get; set; } = "Rectangle"; // Rectangle, Ellipse, Triangle, Diamond
        public string FillColor { get; set; } = "#FF4CAF50";
        public string StrokeColor { get; set; } = "#FF2196F3";
        public double StrokeThickness { get; set; } = 2;
        public double Opacity { get; set; } = 0.7;
        public string StrokeDashArray { get; set; } = string.Empty;

        public ShapeAnnotationConfig()
        {
            Type = AnnotationType.Shape;
            Name = "Shape Annotation";
        }
    }

    /// <summary>
    /// Line annotation configuration
    /// </summary>
    public class LineAnnotationConfig : AnnotationConfig
    {
        public double X2 { get; set; } = 150;
        public double Y2 { get; set; } = 30;
        public string StrokeColor { get; set; } = "#FFFF5722";
        public double StrokeThickness { get; set; } = 3;
        public double Opacity { get; set; } = 0.8;
        public string StrokeDashArray { get; set; } = string.Empty;
        public string StartCapType { get; set; } = "None"; // None, Arrow, Circle, Square
        public string EndCapType { get; set; } = "Arrow";

        public LineAnnotationConfig()
        {
            Type = AnnotationType.Line;
            Name = "Line Annotation";
        }
    }
}