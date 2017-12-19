using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace RockyToy.Common
{
	[PublicAPI]
	public static class ArgChk
	{
		/// <summary>
		/// assert(<paramref name="argument"/> != <see langword="null"/>)
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[ContractAnnotation("argument:null => halt")]
		[LocalizationRequired(false)]
		public static void NotNull(object argument, [InvokerParameterName] string argumentName)
		{
			if (argument == null)
				throw new ArgumentNullException(argumentName, $"{argumentName} cannot be null");
		}

		/// <summary>
		/// assert(<paramref name="argument"/> == <see langword="null"/>)
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[ContractAnnotation("argument:notnull => halt")]
		[LocalizationRequired(false)]
		public static void Null(object argument, [InvokerParameterName] string argumentName)
		{
			if (argument != null)
				throw new ArgumentNullException(argumentName, $"{argumentName} must be null");
		}

		/// <summary>
		/// assert((<paramref name="argument"/>?.Count ?? 0) &gt; 0)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[ContractAnnotation("argument:null => halt")]
		[LocalizationRequired(false)]
		public static void NotNullOrEmpty<T>(ICollection<T> argument, [InvokerParameterName] string argumentName)
		{
			NotNull(argument, argumentName);
			if (argument.Count == 0)
				throw new ArgumentNullException(argumentName, $"{argumentName} ({argument.Count}) is not null nor empty");
		}

		/// <summary>
		/// assert(<see cref="string.IsNullOrEmpty"/>)
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[ContractAnnotation("argument:null => halt")]
		[LocalizationRequired(false)]
		public static void NotNullOrEmpty(string argument, [InvokerParameterName] string argumentName)
		{
			if (string.IsNullOrEmpty(argument))
				throw new ArgumentNullException(argumentName, $"{argumentName} is null or empty string");
		}

		/// <summary>
		/// assert((<paramref name="argument"/>?.Count ?? 0) == 0)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[ContractAnnotation("argument:notnull => halt")]
		[LocalizationRequired(false)]
		public static void NullOrEmpty<T>(ICollection<T> argument, [InvokerParameterName] string argumentName)
		{
			if ((argument?.Count ?? 0) > 0)
				throw new ArgumentNullException(argumentName, $"{argumentName} ({argument.Count}) is not null nor empty");
		}

		/// <summary>
		/// assert(!<see cref="string.IsNullOrEmpty"/>)
		/// </summary>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[ContractAnnotation("argument:notnull => halt")]
		[LocalizationRequired(false)]
		public static void NullOrEmpty(string argument, [InvokerParameterName] string argumentName)
		{
			if (!string.IsNullOrEmpty(argument))
				throw new ArgumentNullException(argumentName, $"{argumentName} is not null nor empty string");
		}

		/// <summary>
		/// assert(<paramref name="argument"/> &lt; <paramref name="lengthOffset"/> &amp;&amp; <paramref name="argument"/> &gt;= <paramref name="offset"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="lengthOffset"></param>
		/// <param name="offset">default is default value of <see cref="T"/></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void InBound<T>([NotNull]T argument, [InvokerParameterName] string argumentName, T lengthOffset, T offset = default(T))
			where T : IComparable<T>
		{
			NotNull(argument, argumentName);
			if (lengthOffset.CompareTo(offset) <= 0)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"The boundary is invalid [{offset},{lengthOffset})");

			if (argument.CompareTo(offset) < 0 || argument.CompareTo(lengthOffset) >= 0)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"{argumentName} ({argument}) is out of bound [{offset},{lengthOffset})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/> &lt; <paramref name="collection"/>.Count &amp;&amp; <paramref name="argument"/> &gt;= 0)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="collection"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void InBound<T>(int argument, [InvokerParameterName] string argumentName, [NotNull]ICollection<T> collection)
		{
			NotNull(collection, argumentName);
			InBound(argument, argumentName, collection.Count);
		}

		/// <summary>
		/// assert(<paramref name="predicate"/>() == true)
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Predicate(Func<bool> predicate, [InvokerParameterName] string argumentName)
		{
			if (!predicate())
				throw new ArgumentException($"{argumentName} failed predicate", argumentName);
		}

		/// <summary>
		/// assert(<paramref name="argument"/> &gt; <paramref name="value"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="value"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Gt<T>([NotNull]T argument, [InvokerParameterName] string argumentName, T value)
			where T : IComparable<T>
		{
			NotNull(argument, argumentName);
			if (argument.CompareTo(value) <= 0)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"{argumentName} ({argument}) is not greater than {value})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/> &lt; <paramref name="value"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="value"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Lt<T>([NotNull]T argument, [InvokerParameterName] string argumentName, T value)
			where T : IComparable<T>
		{
			NotNull(argument, argumentName);
			if (argument.CompareTo(value) >= 0)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"{argumentName} ({argument}) is not less than {value})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/> == <paramref name="value"/>)
		/// Note that if both value are <see langword="null"/>, it will not throw exception.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="value"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Eq<T>(T argument, [InvokerParameterName] string argumentName, T value)
			where T : IComparable<T>
		{
			// special case?
			if (argument == null && value == null)
				return;
			if (argument == null)
				throw new ArgumentOutOfRangeException(argumentName, null, $"{argumentName} (null) is not equal to {value})");
			if (value == null)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"{argumentName} ({argument}) is not equal to null)");
			if (argument.CompareTo(value) != 0)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"{argumentName} ({argument}) is not equal to {value})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/> != <paramref name="value"/>)
		/// Note that if both value are <see langword="null"/>, it will throw exception.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="value"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Ne<T>(T argument, [InvokerParameterName] string argumentName, T value)
			where T : IComparable<T>
		{
			// special case?
			if (argument == null && value == null)
				throw new ArgumentOutOfRangeException(argumentName, null, $"{argumentName} (null) is equal to null)");
			if (argument == null || value == null)
				return;
			if (argument.CompareTo(value) == 0)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"{argumentName} ({argument}) is equal to {value})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/>.Count &lt; <paramref name="size"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="size"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Lt<T>([NotNull]ICollection<T> argument, [InvokerParameterName] string argumentName, int size)
		{
			NotNull(argument, argumentName);
			if (argument.Count >= size)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"size of {argumentName} ({argument.Count}) is not less than {size})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/>.Count &gt; <paramref name="size"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="size"></param>[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Gt<T>([NotNull]ICollection<T> argument, [InvokerParameterName] string argumentName, int size)
		{
			NotNull(argument, argumentName);
			if (argument.Count <= size)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"size of {argumentName} ({argument.Count}) is not greater than {size})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/>.Count == <paramref name="size"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="size"></param>[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Eq<T>([NotNull]ICollection<T> argument, [InvokerParameterName] string argumentName, int size)
		{
			NotNull(argument, argumentName);
			if (argument.Count != size)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"size of {argumentName} ({argument.Count}) is not equal to {size})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/>.Count != <paramref name="size"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		/// <param name="size"></param>[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void Ne<T>([NotNull]ICollection<T> argument, [InvokerParameterName] string argumentName, int size)
		{
			NotNull(argument, argumentName);
			if (argument.Count == size)
				throw new ArgumentOutOfRangeException(argumentName, argument, $"size of {argumentName} ({argument.Count}) is equal to {size})");
		}

		/// <summary>
		/// assert(<paramref name="argument"/> is defined in <typeparamref name="TEnum"/>)
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <param name="argument"></param>
		/// <param name="argumentName"></param>
		[DebuggerStepThrough]
		[LocalizationRequired(false)]
		public static void DefinedInEnum<TEnum>(TEnum argument, string argumentName)
			where TEnum : struct
		{
			if (!typeof(TEnum).GetTypeInfo().IsEnum)
				throw new ArgumentException($"{argumentName}'s type ({typeof(TEnum)}) is not Enum", argumentName);
			if (!Enum.IsDefined(typeof(TEnum), argument))
				throw new ArgumentException($"{argumentName} ({argument}) is not defined in ({typeof(TEnum)})", argumentName);
		}
	}
}
