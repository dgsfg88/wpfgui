using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace wpfgui.Views
{
	/// <summary>
	/// Interaction logic for AIScrollViewer.xaml
	/// </summary>
	public partial class AIScrollViewer : ContentControl
	{
		private ContentPresenter presenter;
		private Canvas canvas;

		public AIScrollViewer()
		{
			InitializeComponent();
		}

		public double VerticalPosition
		{
			get { return (double)GetValue(VerticalPositionProperty); }
			set { SetValue(VerticalPositionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for VerticalPosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty VerticalPositionProperty =
			DependencyProperty.Register(nameof(VerticalPosition), typeof(double), typeof(AIScrollViewer), new PropertyMetadata(0.5));

		public double HorizontalPosition
		{
			get { return (double)GetValue(HorizontalPositionProperty); }
			set { SetValue(HorizontalPositionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for HorizontalPosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HorizontalPositionProperty =
			DependencyProperty.Register(nameof(HorizontalPosition), typeof(double), typeof(AIScrollViewer), new PropertyMetadata(0.5));

		public ScrollBarVisibility VerticalScrollBarVisibiliy
		{
			get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibiliyProperty); }
			set { SetValue(VerticalScrollBarVisibiliyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for VerticalScrollBarVisibiliy.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty VerticalScrollBarVisibiliyProperty =
			DependencyProperty.Register(nameof(VerticalScrollBarVisibiliy), typeof(ScrollBarVisibility), 
				typeof(AIScrollViewer), new PropertyMetadata(ScrollBarVisibility.Auto));

		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
			set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
		}

		// Using a DependencyProperty as the backing store for HorizontalScrollBarVisibility.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
			DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), 
				typeof(AIScrollViewer), new PropertyMetadata(ScrollBarVisibility.Auto));

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
			get { return (int)GetValue(WidthRatioProperty); }
			protected set { SetValue(WidthRatioPropertyKey, value); }
		}

		private static readonly DependencyPropertyKey HeightRatioPropertyKey
		= DependencyProperty.RegisterReadOnly(
			nameof(HeightRatio),
			typeof(double), typeof(AIScrollViewer),
			new FrameworkPropertyMetadata(default(double),
				FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty HeightRatioProperty
			= WidthRatioPropertyKey.DependencyProperty;

		public double HeightRatio
		{
			get { return (int)GetValue(HeightRatioProperty); }
			protected set { SetValue(HeightRatioPropertyKey, value); }
		}

		private void VerticalScrollBarInitialized(object sender, EventArgs e)
			=> InitializeScrollBar(sender as ScrollBar, nameof(VerticalPosition), nameof(VerticalScrollBarVisibiliy), nameof(ActualHeight));

		private void HorizontalScrollBarInitialized(object sender, EventArgs e)
			=> InitializeScrollBar(sender as ScrollBar, nameof(HorizontalPosition), nameof(HorizontalScrollBarVisibility), nameof(ActualWidth));

		private void InitializeScrollBar(ScrollBar scrollBar, string positionBinding, string visibilityBinding, string currentSizeName)
		{
			scrollBar.SetBinding(ScrollBar.ValueProperty, new Binding(positionBinding) { Mode = BindingMode.TwoWay, Source = this });
			scrollBar.SetBinding(ScrollBar.ViewportSizeProperty, new Binding(currentSizeName) { Mode = BindingMode.OneWay, Source = scrollBar });
		}

		private void ContentPresenterInizilized(object sender, EventArgs e)
		{
			if (sender is ContentPresenter presenter)
			{
				this.presenter = presenter;

				DependencyPropertyDescriptor.FromProperty(ContentPresenter.ActualHeightProperty, typeof(ContentPresenter))
					.AddValueChanged(presenter, presenterHeightChanged);
				DependencyPropertyDescriptor.FromProperty(ContentPresenter.ActualWidthProperty, typeof(ContentPresenter))
					.AddValueChanged(presenter, presenterWidthChanged);
				//TODO biding with top and left canvas properties and offsets
			}
		}

		private void CanvasInitialized(object sender, EventArgs e)
		{
			if (sender is Canvas canvas)
			{
				DependencyPropertyDescriptor.FromProperty(Canvas.ActualHeightProperty, typeof(ContentControl))
					.AddValueChanged(canvas, presenterHeightChanged);
				DependencyPropertyDescriptor.FromProperty(Canvas.ActualWidthProperty, typeof(ContentControl))
					.AddValueChanged(canvas, presenterWidthChanged);
			}
		}

		private void presenterWidthChanged(object sender, EventArgs e)
		{
			if (presenter != null && canvas != null)
			{
				WidthRatio = presenter.ActualWidth / canvas.ActualWidth;
			}
		}

		private void presenterHeightChanged(object sender, EventArgs e)
		{
			if (presenter != null && canvas != null)
			{
				HeightRatio = presenter.ActualHeight / canvas.ActualHeight;
			}
		}
	}
}
