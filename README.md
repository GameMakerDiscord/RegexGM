# RegexGM

This is an extension for Gamemaker written in C# that ports the wonderful, easy to use, and powerful .NET Regex class over to Gamemaker.

Ths source code for the library can be found in the RegexGM folder, and an example project can be found in the RegexTests.gmx folder.

In addition to the source, the library is fully documented [here](https://precisamento.github.io/extensions/regex-for-gm/). 
To quickly install the library, you can find it for free on the [marketplace](https://marketplace.yoyogames.com/assets/6358/regexgm).

# Example
```cs
// Create a regex with no additional options and a timeout of 3000ms.
// The pattern [^\d] matches all non-digit characters.
var regex = regex_create("[^\d]", RO_NONE, 3000);
var input = "12ff45hj9";
var replace = "";

// This will replace all matches with nothing, effectively removing them from the string.
var result = regex_replace(regex, input, replace);
show_debug_message(result);

// Outputs:
// 12459
```
