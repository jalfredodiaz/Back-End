using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Back_End.ModelsBD;

namespace Back_End
{
    internal static class Extenciones
    {
        public static bool IsNumeric(this string text)
        {
            return double.TryParse(text, out _);
        }
        public static int ValorInt(this string text)
        {

            int.TryParse(text, out int res);

            return res;
        }
        public static decimal ValorDecimal(this string text)
        {

            decimal.TryParse(text, out decimal res);

            return res;
        }
        public static T Add2<T>(this List<T> lista, T item)
        {
            lista.Add(item);

            return item;
        }


        // Usuarios
        public static IEnumerable<CAT_Usuarios> SinPassword(this IEnumerable<CAT_Usuarios> users)
        {
            return users.Select(x => x.SinPassword());
        }
        public static IQueryable<CAT_Usuarios> SinPassword(this IQueryable<CAT_Usuarios> users)
        {
            return users.Select(x => x.SinPassword());
        }
        public static List<CAT_Usuarios> SinPassword(this List<CAT_Usuarios> users)
        {
            return users.Select(x => x.SinPassword()).ToList();
        }
        public static CAT_Usuarios SinPassword(this CAT_Usuarios user)
        {
            user.cPassword = null;
            return user;
        }
    }
}