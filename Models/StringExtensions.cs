using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prova2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace prova2.Models;

using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;
using System.Text.Json;
using System.Globalization;
using System.Linq;
using System.Text;

public static class StringExtensions
{
    public static string NormalizeString(this string input)
    {
        return string.IsNullOrWhiteSpace(input) 
            ? input 
            : input.Normalize(NormalizationForm.FormD)
                   .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                   .Aggregate("", (current, c) => current + c)
                   .Normalize(NormalizationForm.FormC);
    }
}