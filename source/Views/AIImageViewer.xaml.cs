using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
	/// Interaction logic for AIImageViewer.xaml
	/// </summary>
	public partial class AIImageViewer : Control
	{
		public BitmapSource Source
		{
			get { return (BitmapSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register(nameof(Source), typeof(BitmapSource), typeof(AIImageViewer), 
				new PropertyMetadata(null));

		public double ZoomValue
		{
			get { return (double)GetValue(ZoomValueProperty); }
			set { SetValue(ZoomValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ZoomValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ZoomValueProperty =
			DependencyProperty.Register(nameof(ZoomValue), typeof(double), typeof(AIImageViewer), new PropertyMetadata(1.0));

		public ObservableCollection<Shape> Shapes
		{
			get { return (ObservableCollection<Shape>)GetValue(ShapesProperty); }
			set { SetValue(ShapesProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Shapes.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShapesProperty =
			DependencyProperty.Register(nameof(Shapes), typeof(ObservableCollection<Shape>), typeof(AIImageViewer), 
				new PropertyMetadata(null));

		public AIImageViewer()
		{
			SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
			SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);

			Shapes = new ObservableCollection<Shape>();

			InitializeComponent();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			InitializeBindings();
		}

		private void InitializeBindings()
		{
			var mainGrid = GetTemplateChild("MainGrid_PART") as Grid;
			//Match the grid size with the image size TODO add null check
			mainGrid.SetBinding(Grid.WidthProperty, new Binding(nameof(Source) + "." + nameof(BitmapSource.PixelWidth))
			{
				Source = this,
				Mode = BindingMode.OneWay,
				Converter = CallbackConveter.InitValueConverter(checkNullInput, null)
			});
			mainGrid.SetBinding(Grid.HeightProperty, new Binding(nameof(Source) + "." + nameof(BitmapSource.PixelHeight))
			{
				Source = this,
				Mode = BindingMode.OneWay,
				Converter = CallbackConveter.InitValueConverter(checkNullInput, null)
			});

			var sizeRatioConv = CallbackConveter.InitMultiValueConverter(getScale, null);
			ScaleTransform zoomTransform = new ScaleTransform();
			BindingOperations.SetBinding(zoomTransform, ScaleTransform.ScaleXProperty,
				new MultiBinding()
				{
					Converter = sizeRatioConv,
					Mode = BindingMode.OneWay,
					NotifyOnSourceUpdated = true,
					NotifyOnTargetUpdated = true,
					Bindings =
					{
						new Binding(nameof(ActualWidth)) { Source = this },
						new Binding(nameof(ActualWidth)) { Source = mainGrid },
						new Binding(nameof(ActualHeight)) { Source = this },
						new Binding(nameof(ActualHeight)) { Source = mainGrid },
						new Binding(nameof(ZoomValue)) { Source = this }
					}
				});
			BindingOperations.SetBinding(zoomTransform, ScaleTransform.ScaleYProperty,
				new Binding(nameof(ScaleTransform.ScaleX)) { Source = zoomTransform });
			mainGrid.LayoutTransform = zoomTransform;

			//Main Image Binding
			var bkImage = (Image)Template.FindName("BackgroundImage_PART", this);
			bkImage.SetBinding(Image.SourceProperty, new Binding(nameof(Source))
			{
				Source = this,
				Mode = BindingMode.OneWay
			});
			//Get the shapes container
			var shapesContainer = GetTemplateChild("ShapesContainer_PART") as ItemsControl;
			//Links the shapes with their container
			shapesContainer.SetBinding(ItemsControl.ItemsSourceProperty, 
				new Binding(nameof(Shapes)) { Source = this });
		}

		private object checkNullInput(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is int size)
			{
				return (double)size;
			}
			return DependencyProperty.UnsetValue;
		}

		private object getScale(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(double) &&
				values.Length > 4 &&
				values[0] is double containerSizeX &&
				values[1] is double contentSizeX &&
				values[2] is double containerSizeY &&
				values[3] is double contentSizeY &&
				values[4] is double zoomValue)
			{
				double ratioX = containerSizeX / contentSizeX;
				double ratioY = containerSizeY / contentSizeY;

				double ratio;
				if (ratioX < ratioY)
					ratio = ratioX;
				else
					ratio = ratioY;

				if (double.IsNaN(ratio) || double.IsInfinity(ratio))
					ratio = 1;

				return ratio * zoomValue;
			}
			return DependencyProperty.UnsetValue;
		}
	}
}
