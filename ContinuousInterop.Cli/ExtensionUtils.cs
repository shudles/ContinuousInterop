using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace ContinuousInterop.Cli;

internal class ExtensionUtils
{
    public static T? NullifyAndReturn<T>(ref T? value)
    {
        if (value is T notNullValue)
        {
            value = default;
            return notNullValue;
        }
        return value;
    }
}
