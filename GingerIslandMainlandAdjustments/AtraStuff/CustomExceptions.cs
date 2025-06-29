using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GingerIslandMainlandAdjustments.AtraStuff;

/// <summary>
/// Thrown when an unexpected enum value is received.
/// </summary>
/// <typeparam name="T">The enum type that received an unexpected value.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="UnexpectedEnumValueException{T}"/> class.
/// </remarks>
/// <param name="value">The unexpected enum value.</param>
public class UnexpectedEnumValueException<T>(T value)
    : Exception($"Enum {typeof(T).Name} received unexpected value {value}");

/// <summary>
/// Throw helper for these exceptions.
/// </summary>
public static class TKThrowHelper
{
    /// <inheritdoc cref="UnexpectedEnumValueException{T}"/>
    [StackTraceHidden]
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowUnexpectedEnumValueException<TEnum>(TEnum value)
    {
        throw new UnexpectedEnumValueException<TEnum>(value);
    }

    /// <inheritdoc cref="UnexpectedEnumValueException{T}"/>
    [StackTraceHidden]
    [DoesNotReturn]
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ThrowUnexpectedEnumValueException<TEnum, TReturn>(TEnum value)
    {
        throw new UnexpectedEnumValueException<TEnum>(value);
    }
}
