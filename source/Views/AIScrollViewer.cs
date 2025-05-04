using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpfgui.Converters;

namespace wpfgui.Views
{
	/// <summary>
	/// Interaction logic for AIScrollViewer.xaml
	/// </summary>
	[TemplatePart(Name = AIScrollViewerPart.PART_ScrollViewerCanvas, Type = typeof(Canvas))]
	[TemplatePart(Name = AIScrollViewerPart.PART_ContentPresenter, Type = typeof(ContentPresenter))]
	[TemplatePart(Name = AIScrollViewerPart.PART_HorizontalScrollBar, Type = typeof(ScrollBar))]
	[TemplatePart(Name = AIScrollViewerPart.PART_VerticalScrollBar, Type = typeof(ScrollBar))]
	public class AIScrollViewer : ContentControl
	{
		public static class AIScrollViewerPart
		{
			public const string PART_ScrollViewerCanvas = nameof(PART_ScrollViewerCanvas);
			public const string PART_HorizontalScrollBar = nameof(PART_HorizontalScrollBar);
			public const string PART_VerticalScrollBar = nameof(PART_VerticalScrollBar);
			public const string PART_ContentPresenter = nameof(PART_ContentPresenter);
		}

		private bool isDragStarted = false;
		private Point canvasCenter;
		private Point canvasDragLastPoint = new Point(0, 0);
		private Point dragStartCenter;

		private ContentPresenter ContentPresenter => GetTemplateChild(AIScrollViewerPart.PART_ContentPresenter) as ContentPresenter;
		private Canvas MainCanvas => GetTemplateChild(AIScrollViewerPart.PART_ScrollViewerCanvas) as Canvas;
		private ScrollBar VerticalScrollBar => GetTemplateChild(AIScrollViewerPart.PART_VerticalScrollBar) as ScrollBar;
		private ScrollBar HorizontalScrollBar => GetTemplateChild(AIScrollViewerPart.PART_HorizontalScrollBar) as ScrollBar;

		public AIScrollViewer()
		{
			this.Resources.MergedDictionaries.Add(new Styles.Styles());
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			DependencyPropertyDescriptor.FromProperty(ContentPresenter.ActualHeightProperty, typeof(ContentPresenter))
				.AddValueChanged(ContentPresenter, presenterHeightChanged);
			DependencyPropertyDescriptor.FromProperty(ContentPresenter.ActualWidthProperty, typeof(ContentPresenter))
				.AddValueChanged(ContentPresenter, presenterWidthChanged);
			//TODO biding with top and left canvas properties and offsets

			DependencyPropertyDescriptor.FromProperty(Canvas.ActualHeightProperty, typeof(ContentControl))
				.AddValueChanged(MainCanvas, presenterHeightChanged);
			DependencyPropertyDescriptor.FromProperty(Canvas.ActualWidthProperty, typeof(ContentControl))
				.AddValueChanged(MainCanvas, presenterWidthChanged);

			HorizontalScrollBar.SetBinding(ScrollBar.ValueProperty, new MultiBinding()
			{
				Mode = BindingMode.TwoWay,
				NotifyOnSourceUpdated = true,
				NotifyOnTargetUpdated = true,
				Converter = CallbackConveter.InitMultiValueConverter(calculateScrollValue, calculateScrollPosH),
				Bindings  =
				{
						new Binding(nameof(AIScrollViewer.HorizontalPosition))
						{ Source = this, Mode = BindingMode.TwoWay },
						new Binding(nameof(ContentControl.ActualWidth))
						{ Source = ContentPresenter, Mode = BindingMode.OneWay },
						new Binding(nameof(ContentControl.ActualWidth))
						{ Source = MainCanvas, Mode = BindingMode.OneWay },

				}
			});
			HorizontalScrollBar.SetBinding(ScrollBar.VisibilityProperty,
				new MultiBinding()
				{
					NotifyOnSourceUpdated = true,
					Mode = BindingMode.OneWay,
					Converter = CallbackConveter.InitMultiValueConverter(calculateScrollBarVisibility, null),
					Bindings =
					{
						new Binding(nameof(AIScrollViewer.WidthRatio))
						{ Source = this, Mode = BindingMode.OneWay },
						new Binding(nameof(AIScrollViewer.HorizontalScrollBarVisibility))
						{ Source = this, Mode = BindingMode.OneWay },
					}
				});
			VerticalScrollBar.SetBinding(ScrollBar.ValueProperty, new MultiBinding()
			{
				Mode = BindingMode.TwoWay,
				NotifyOnSourceUpdated = true,
				NotifyOnTargetUpdated = true,
				Converter = CallbackConveter.InitMultiValueConverter(calculateScrollValue, calculateScrollPosV),
				Bindings =
				{
						new Binding(nameof(AIScrollViewer.VerticalPosition))
						{ Source = this, Mode = BindingMode.TwoWay },
						new Binding(nameof(ContentControl.ActualHeight))
						{ Source = ContentPresenter, Mode = BindingMode.OneWay },
						new Binding(nameof(ContentControl.ActualHeight))
						{ Source = MainCanvas, Mode = BindingMode.OneWay },

				}
			});
			VerticalScrollBar.SetBinding(ScrollBar.VisibilityProperty,
				new MultiBinding()
				{
					NotifyOnSourceUpdated = true,
					Mode = BindingMode.OneWay,
					Converter = CallbackConveter.InitMultiValueConverter(calculateScrollBarVisibility, null),
					Bindings =
					{
						new Binding(nameof(AIScrollViewer.HeightRatio))
						{ Source = this, Mode = BindingMode.OneWay },
						new Binding(nameof(AIScrollViewer.VerticalScrollBarVisibility))
						{ Source = this, Mode = BindingMode.OneWay },
					}
				});

			ContentPresenter.SetBinding(Canvas.TopProperty,
				new MultiBinding()
				{
					NotifyOnSourceUpdated = true,
					Mode = BindingMode.OneWay,
					Converter = CallbackConveter.InitMultiValueConverter(calculateContentOffset, null),
					Bindings =
					{
						new Binding(nameof(VerticalPosition))
						{ Source = this, Mode = BindingMode.OneWay },
						new Binding(nameof(ContentControl.ActualHeight))
						{ Source = ContentPresenter, Mode = BindingMode.OneWay },
						new Binding(nameof(ContentControl.ActualHeight))
						{ Source = MainCanvas, Mode = BindingMode.OneWay },
					}
				});

			ContentPresenter.SetBinding(Canvas.LeftProperty,
				new MultiBinding()
				{
					NotifyOnSourceUpdated = true,
					Mode = BindingMode.OneWay,
					Converter = CallbackConveter.InitMultiValueConverter(calculateContentOffset, null),
					Bindings =
					{
						new Binding(nameof(HorizontalPosition))
						{ Source = this, Mode = BindingMode.OneWay },
						new Binding(nameof(ContentControl.ActualWidth))
						{ Source = ContentPresenter, Mode = BindingMode.OneWay },
						new Binding(nameof(ContentControl.ActualWidth))
						{ Source = MainCanvas, Mode = BindingMode.OneWay },
					}
				});

			MainCanvas.MouseLeftButtonDown += OnMouseLeftButtonDown;
			MainCanvas.MouseMove += MainCanvas_MouseMove;
			MainCanvas.MouseLeftButtonUp += MainCanvas_MouseLeftButtonUp; ;
			MainCanvas.MouseLeave += MainCanvas_MouseLeave;
		}

		private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
			=> MainCanvas_MouseLeave(sender, e);

		private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
		{
			if (isDragStarted)
			{
				isDragStarted = false;
				MainCanvas.ReleaseMouseCapture();
				e.Handled = true;
			}
		}

		private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDragStarted)
			{
				if (!IsMouseDragEnabled)
					isDragStarted = false;
				else
				{
					var curCanvPt = e.GetPosition(MainCanvas);
					if (Math.Abs((curCanvPt - canvasDragLastPoint).Length) > 1)
					{
						var dragDiff = canvasCenter - curCanvPt;
						dragDiff.X /= ContentPresenter.ActualWidth;
						dragDiff.Y /= ContentPresenter.ActualHeight;

						canvasDragLastPoint = curCanvPt;

						VerticalPosition = dragStartCenter.Y + dragDiff.Y;
						HorizontalPosition = dragStartCenter.X + dragDiff.X;

						e.Handled = true;
					}
				}
			}
		}

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!isDragStarted && IsMouseDragEnabled)
			{
				isDragStarted = true;
				canvasCenter = e.GetPosition(MainCanvas);
				canvasDragLastPoint = e.GetPosition(MainCanvas);

				dragStartCenter = new Point(HorizontalPosition, VerticalPosition);

				MainCanvas.CaptureMouse();

				e.Handled = true;
			}
		}

		private object[] calculateScrollPosV(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			var outArray = targetTypes.Select(x => Binding.DoNothing).ToArray();
			if (value is double scrollValue)
			{
				var contentSize = ContentPresenter.ActualHeight;
				var containerSize = MainCanvas.ActualHeight;

				outArray[0] = calculateScrollPos(scrollValue, contentSize, containerSize);
			}
			return outArray;
		}

		private object[] calculateScrollPosH(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			var outArray = targetTypes.Select(x => Binding.DoNothing).ToArray();
			if (value is double scrollValue)
			{
				var contentSize = ContentPresenter.ActualWidth;
				var containerSize = MainCanvas.ActualWidth;

				outArray[0] = calculateScrollPos(scrollValue, contentSize, containerSize);
			}
			return outArray;
		}

		private double calculateScrollPos(double scrollValue, double contentSize, double containerSize)
		{
			if (contentSize <= containerSize)
				return 0.5;
			else
				return ((contentSize - containerSize) * scrollValue + containerSize / 2) / contentSize;
		}

		private object calculateScrollValue(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length > 2 &&
				values[0] is double scrollPos &&
				values[1] is double contentSize &&
				values[2] is double containerSize)
			{
				if (contentSize <= containerSize)
					return 0.5;
				else
				{
					return (scrollPos * contentSize - containerSize / 2) / (contentSize - containerSize);
				}
			}
			return DependencyProperty.UnsetValue;
		}

		private object calculateScrollBarVisibility(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length > 1 &&
				values[0] is double ratio &&
				values[1] is ScrollBarVisibility scrollBarVisibility)
			{
				switch (scrollBarVisibility)
				{
					case ScrollBarVisibility.Visible:
						return Visibility.Visible;
					case ScrollBarVisibility.Auto:
						if (ratio >= 1)
							return Visibility.Collapsed;
						else
							return Visibility.Visible;
					case ScrollBarVisibility.Hidden:
					case ScrollBarVisibility.Disabled:
						return Visibility.Collapsed;
				}
			}
			return DependencyProperty.UnsetValue;
		}

		private object calculateContentOffset(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length > 2 &&
				values[0] is double scrollBarPosition &&
				values[1] is double contentSize &&
				values[2] is double containerSize)
			{
				double pos = 0;
				var pixelDiff = contentSize - containerSize;
				if (pixelDiff <= 0)
				{
					pos = -pixelDiff / 2;
				}
				else
				{
					pos = -(scrollBarPosition * contentSize - containerSize / 2);
				}
				return pos;
			}
			return DependencyProperty.UnsetValue;
		}

		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(ScrollViewer.VerticalScrollBarVisibilityProperty);
			set => SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, value);
		}
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get => (ScrollBarVisibility)GetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty);
			set => SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, value);
		}

		public double VerticalPosition
		{
			get { return (double)GetValue(VerticalPositionProperty); }
			set { SetValue(VerticalPositionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for VerticalPosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty VerticalPositionProperty =
			DependencyProperty.Register(nameof(VerticalPosition), typeof(double), typeof(AIScrollViewer),
				new PropertyMetadata(0.5, null, coerceVerticalPosition));

		private static object coerceVerticalPosition(DependencyObject d, object baseValue)
			=> coercePosition(d, baseValue, ActualHeightProperty);

		private static double coercePosition(DependencyObject d, object baseValue, 
			DependencyProperty lengthProperty)
		{
			if (d is AIScrollViewer v && baseValue is double value &&
				v.ContentPresenter.GetValue(lengthProperty) is double contentSize &&
				v.MainCanvas.GetValue(lengthProperty) is double containerSize)
			{ 
				var min = v.calculateScrollPos(0, contentSize,
					containerSize);
				var max = v.calculateScrollPos(1, contentSize,
					containerSize);

				if (double.IsNaN(value) || double.IsInfinity(value))
					return 0.5;
				else if (value < min)
					return min;
				else if (value > max)
					return max;
				return value;
			}
			return 0.5;
		}

		public double HorizontalPosition
		{
			get { return (double)GetValue(HorizontalPositionProperty); }
			set { SetValue(HorizontalPositionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for HorizontalPosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HorizontalPositionProperty =
			DependencyProperty.Register(nameof(HorizontalPosition), typeof(double), typeof(AIScrollViewer), new PropertyMetadata(0.5, null, coerceHorizontalPosition));

		private static object coerceHorizontalPosition(DependencyObject d, object baseValue)
			=> coercePosition(d, baseValue, ActualWidthProperty);

		private static readonly DependencyPropertyKey WidthRatioPropertyKey
		= DependencyProperty.RegisterReadOnly(
			nameof(WidthRatio),
			typeof(double), typeof(AIScrollViewer),
			new FrameworkPropertyMetadata(default(double),
				FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty WidthRatioProperty
			= WidthRatioPropertyKey.DependencyProperty;

		public double WidthRatio
		{
			get { return (double)GetValue(WidthRatioProperty); }
			protected set { SetValue(WidthRatioPropertyKey, value); }
		}

		private static readonly DependencyPropertyKey HeightRatioPropertyKey
		= DependencyProperty.RegisterReadOnly(
			nameof(HeightRatio),
			typeof(double), typeof(AIScrollViewer),
			new FrameworkPropertyMetadata(default(double),
				FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty HeightRatioProperty
			= HeightRatioPropertyKey.DependencyProperty;

		public double HeightRatio
		{
			get { return (double)GetValue(HeightRatioProperty); }
			protected set { SetValue(HeightRatioPropertyKey, value); }
		}



		public bool IsMouseDragEnabled
		{
			get { return (bool)GetValue(IsMouseDragEnabledProperty); }
			set { SetValue(IsMouseDragEnabledProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsMouseDragEnabled.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsMouseDragEnabledProperty =
			DependencyProperty.Register(nameof(IsMouseDragEnabled), typeof(bool), 
				typeof(AIScrollViewer), new PropertyMetadata(true));

		private void presenterWidthChanged(object sender, EventArgs e)
		{
			if (ContentPresenter != null && MainCanvas != null)
			{
				WidthRatio = MainCanvas.ActualWidth / ContentPresenter.ActualWidth;
				SetScrollBarViewportSize(WidthRatio, HorizontalScrollBar, HorizontalPositionProperty);
			}
		}

		private void presenterHeightChanged(object sender, EventArgs e)
		{
			if (ContentPresenter != null && MainCanvas != null)
			{
				HeightRatio = MainCanvas.ActualHeight / ContentPresenter.ActualHeight;
				SetScrollBarViewportSize(HeightRatio, VerticalScrollBar, VerticalPositionProperty);
			}
		}

		private void SetScrollBarViewportSize(double ratio, ScrollBar scrollBar,
			DependencyProperty scrollPosition)
		{
			if (ratio >= 1)
			{
				scrollBar.Maximum = 0;
				scrollBar.ViewportSize = 1;
				SetValue(scrollPosition, 0.5);
			}
			else
			{
				scrollBar.Maximum = 1;
				double range = 1.0;
				scrollBar.ViewportSize = (ratio * range) / (1 - ratio);
			}
		}
	}
}
