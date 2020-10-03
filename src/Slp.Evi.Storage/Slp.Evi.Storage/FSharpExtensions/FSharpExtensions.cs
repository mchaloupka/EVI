using Microsoft.FSharp.Core;

namespace Slp.Evi.Storage.Common.FSharpExtensions
{
    public static class FSharpExtensions
    {
        public static bool IsSome<T>(this FSharpOption<T> option)
        {
            return FSharpOption<T>.get_IsSome(option);
        }

        public static bool IsNone<T>(this FSharpOption<T> option)
        {
            return FSharpOption<T>.get_IsNone(option);
        }

        public static T? ToNullable<T>(this FSharpOption<T> option) where T: struct
        {
            if (IsSome(option))
            {
                return option.Value;
            }
            else
            {
                return null;
            }
        }
    }
}
