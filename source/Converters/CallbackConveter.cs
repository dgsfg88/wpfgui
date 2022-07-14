using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace wpfgui.Converters
{
	/// <summary>
	/// Converter implementation that use callback to generate output
	/// </summary>
	public class CallbackConveter : IValueConverter, IMultiValueConverter
	{
		public delegate object ValueConvertDelegate(object value, Type targetType, object parameter, CultureInfo culture);
		public delegate object ValueMultiConvertDelegate(object[] values, Type targetType, object parameter, CultureInfo culture);
		public delegate object[] ValueMultiConvertBackDelegate(object value, Type[] targetTypes, object parameter, CultureInfo culture);
		/// <summary>
		/// Callback called in <see cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>
		/// </summary>
		public ValueConvertDelegate ValueConvertCallback { get; set; }
		/// <summary>
		/// Callback called in <see cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>
		/// </summary>
		public ValueConvertDelegate ValueConvertBackCallback { get; set; }
		/// <summary>
		/// Callback called in <see cref="IMultiValueConverter.Convert(object[], Type, object, CultureInfo)"/>
		/// </summary>
		public ValueMultiConvertDelegate MultiValueConvertCallback { get; set; }
		/// <summary>
		/// Callback called in <see cref="IMultiValueConverter.ConvertBack(object, Type[], object, CultureInfo)"/>
		/// </summary>
		public ValueMultiConvertBackDelegate MultiValueConvertBackCallback { get; set; }
		/// <summary>
		/// Init a new instance of a <see cref="IValueConverter"/>
		/// </summary>
		/// <param name="ValueConvertCallback"><see cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/> method</param>
		/// <param name="ValueConvertBackCallback"><see cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/> method</param>
		/// <returns>The callback converter generated</returns>
		public static IValueConverter InitValueConverter(ValueConvertDelegate ValueConvertCallback,
			ValueConvertDelegate ValueConvertBackCallback)
			=> new CallbackConveter() { ValueConvertCallback = ValueConvertCallback, ValueConvertBackCallback = ValueConvertBackCallback };
		/// <summary>
		/// Init a new instance of a <see cref="IMultiValueConverter"/>
		/// </summary>
		/// <param name="ValueConvertCallback"><see cref="IMultiValueConverter.Convert(object[], Type, object, CultureInfo)"/> method</param>
		/// <param name="ValueConvertBackCallback"><see cref="IMultiValueConverter.ConvertBack(object, Type[], object, CultureInfo)"/> method</param>
		/// <returns>The callback converter generated</returns>
		public static IMultiValueConverter InitMultiValueConverter(ValueMultiConvertDelegate MultiValueConvertCallback,
			ValueMultiConvertBackDelegate MultiValueConvertBackCallback)
			=> new CallbackConveter() { MultiValueConvertCallback = MultiValueConvertCallback, MultiValueConvertBackCallback = MultiValueConvertBackCallback };

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (MultiValueConvertCallback != null)
				return MultiValueConvertCallback(values, targetType, parameter, culture);
			return DependencyProperty.UnsetValue;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (ValueConvertCallback != null)
				return ValueConvertCallback(value, targetType, parameter, culture);
			return DependencyProperty.UnsetValue;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			if (MultiValueConvertBackCallback != null)
				return MultiValueConvertBackCallback(value, targetTypes, parameter, culture);
			return targetTypes.Select(x => Binding.DoNothing).ToArray();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (ValueConvertBackCallback != null)
				return ValueConvertBackCallback(value, targetType, parameter, culture);
			return Binding.DoNothing;
		}
	}
}
