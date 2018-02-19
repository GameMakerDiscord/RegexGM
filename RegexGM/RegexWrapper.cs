using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace RegexGM
{
    /// <summary>
    /// A Gamemaker wrapper for the Regex class.
    /// </summary>
    public static class RegexWrapper
    {
        private static List<object> _slots = new List<object>();
        private static Queue<int> _openSlots = new Queue<int>();

        /// <summary>
        /// Creates a regex object and returns it's id.
        /// </summary>
        /// <param name="pattern">The regex pattern to use.</param>
        /// <param name="options">A bit flag representing the regex options (RO_*) to use.</param>
        /// <param name="timeout">How long the regex can run before timing out.</param>
        /// <returns>Regex id</returns>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.-ctor?view=netframework-4.7#System_Text_RegularExpressions_Regex__ctor_System_String_System_Text_RegularExpressions_RegexOptions_System_TimeSpan_)</ref>
        [DllExport("RegexCreate", CallingConvention.Cdecl)]
        public static double RegexCreate(string pattern, double options, double timeout)
        {
            var regex = new Regex(pattern, (RegexOptions)options, TimeSpan.FromMilliseconds(timeout));
            return Add(regex);
        }

        /// <summary>
        /// Destroys a created object.
        /// </summary>
        /// <param name="id">The id of the object to destroy.</param>
        /// <returns>Bool</returns>
        [DllExport("DestroyId", CallingConvention.Cdecl)]
        public static double RegexDestroyId(double id)
        {
            var index = (int)id;
            if (index < 0 || index >= _slots.Count || _slots[index] == null)
                return 0;

            _slots[index] = null;
            _openSlots.Enqueue(index);
            return 1;
        }

        /// <summary>
        /// Destroys all of the created objects. Automatically called on game end.
        /// </summary>
        /// <returns>1</returns>
        [DllExport("DestroyAll", CallingConvention.Cdecl)]
        public static double RegexDestroyAll()
        {
            _slots.Clear();
            _openSlots.Clear();
            return 1;
        }

        /// <summary>
        /// Searches a string for all occurrences of a regex, and returns the id of the collection.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <returns>Matches id</returns>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matches?view=netframework-4.7#System_Text_RegularExpressions_Regex_Matches_System_String_)</ref>
        [DllExport("Matches", CallingConvention.Cdecl)]
        public static double RegexGetMatches(double regex_id, string input)
        {
            try
            {
                return Add(GetRegex(regex_id).Matches(input));
            }
            catch(RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Searches a string for all occurrences of a regex, and returns a MATCHES json object.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <param name="json_options">Additional json options (JO_*).</param>
        /// <returns>String (MATCHES object)</returns>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matches?view=netframework-4.7#System_Text_RegularExpressions_Regex_Matches_System_String_)</ref>
        [DllExport("MatchesJson", CallingConvention.Cdecl)]
        public static string RegexMatchesJson(double regex_id, string input, double json_options)
        {
            try
            {
                return MatchCollectionToJson(GetRegex(regex_id).Matches(input), (JsonOptions)json_options);
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Searches a string for all occurrences of a regex, beginning at the specified location, and returns the id of the collection.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <param name="startAt">The position to start from.</param>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matches?view=netframework-4.7#System_Text_RegularExpressions_Regex_Matches_System_String_System_Int32_)</ref>
        /// <returns>Matches id</returns>
        [DllExport("MatchesFrom", CallingConvention.Cdecl)]
        public static double RegexMatchesFrom(double regex_id, string input, double startAt)
        {
            try
            {
                return Add(GetRegex(regex_id).Matches(input, (int)startAt));
            }
            catch (RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Searches a string for all occurrences of a regex, beginning at the specified location, and returns a MATCHES json object.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The id of the string to search.</param>
        /// <param name="startat">The position to start from.</param>
        /// <param name="json_options">Additional json options (JO_*).</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matches?view=netframework-4.7#System_Text_RegularExpressions_Regex_Matches_System_String_System_Int32_)</ref>
        /// <returns>String (MATCHES)</returns>
        [DllExport("MatchesFromJson", CallingConvention.Cdecl)]
        public static string RegexMatchesFromJson(double regex_id, string input, double startat, double json_options)
        {
            try
            {
                return MatchCollectionToJson(GetRegex(regex_id).Matches(input, (int)startat), (JsonOptions)json_options);
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the number of matches in a matches object.
        /// </summary>
        /// <param name="matches_id">The id of the matches object to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.matchcollection.count?view=netframework-4.7#System_Text_RegularExpressions_MatchCollection_Count)</ref>
        /// <returns>Real</returns>
        [DllExport("MatchesGetCount", CallingConvention.Cdecl)]
        public static double MatchesGetCount(double matches_id)
        {
            return GetMatches(matches_id).Count;
        }

        /// <summary>
        /// Gets the match at the given index from a matches object.
        /// </summary>
        /// <param name="matches_id">The id of the matches object to use.</param>
        /// <param name="index">The index of the match.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.matchcollection.item?view=netframework-4.7#System_Text_RegularExpressions_MatchCollection_Item_System_Int32_)</ref>
        /// <exception cref="ArgumentOutOfRangeException">Returns -1</exception>
        /// <returns>Match id</returns>
        [DllExport("MatchesGetMatch", CallingConvention.Cdecl)]
        public static double MatchesGetMatch(double matches_id, double index)
        {
            try
            {
                return Add(GetMatches(matches_id)[(int)index]);
            }
            catch (ArgumentOutOfRangeException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Determines whether the regex finds a match in the given string.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.ismatch?view=netframework-4.7#System_Text_RegularExpressions_Regex_IsMatch_System_String_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -1</exception>
        /// <returns>Bool</returns>
        [DllExport("IsMatch", CallingConvention.Cdecl)]
        public static double RegexIsMatch(double regex_id, string input)
        {
            try
            {
                return GetRegex(regex_id).IsMatch(input) ? 1 : 0;
            }
            catch(RegexMatchTimeoutException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Determines whether the regex finds a match in the given string, beginning from the specified location.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <param name="startat">The position to start from.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.ismatch?view=netframework-4.7#System_Text_RegularExpressions_Regex_IsMatch_System_String_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -1</exception>
        /// <returns>Bool</returns>
        [DllExport("IsMatchFrom", CallingConvention.Cdecl)]
        public static double RegexIsMatchFrom(double regex_id, string input, double startat)
        {
            try
            {
                return GetRegex(regex_id).IsMatch(input, (int)startat) ? 1 : 0;
            }
            catch (RegexMatchTimeoutException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Searches a string for the first occurrence of a regex, and returns the match id.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.match?view=netframework-4.7#System_Text_RegularExpressions_Regex_Match_System_String_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <returns>Match id on success; otherwise -1</returns>
        [DllExport("Match", CallingConvention.Cdecl)]
        public static double RegexMatch(double regex_id, string input)
        {
            try
            {
                Match match = GetRegex(regex_id).Match(input);
                if (match.Success)
                    return Add(match);
                return -1;
            }
            catch(RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Searches a string for the first occurrence of a regex, and returns a MATCH json object.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <param name="json_options">Additional json options (JO_*).</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.match?view=netframework-4.7#System_Text_RegularExpressions_Regex_Match_System_String_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <returns>String (MATCH object) on success; otherwise ""</returns>
        [DllExport("MatchJson", CallingConvention.Cdecl)]
        public static string RegexMatchJson(double regex_id, string input, double json_options)
        {
            try
            {
                Match match = GetRegex(regex_id).Match(input);

                if (match.Success)
                    return MatchToJson(match, (JsonOptions)json_options);

                return string.Empty;
            }
            catch(RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Searches a string for the first occurrence of a regex, beginning from the specified location, and returns the match id.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <param name="startat">The position to start from.</param>
        /// <exception cref="RegexMatchTimeoutException">Returns -2.</exception>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matches?view=netframework-4.7#System_Text_RegularExpressions_Regex_Matches_System_String_System_Int32_)</ref>
        /// <returns>Match id on success; Otherwise -1</returns>
        [DllExport("MatchFrom", CallingConvention.Cdecl)]
        public static double RegexMatchFrom(double regex_id, string input, double startat)
        {
            try
            {
                Match match = GetRegex(regex_id).Match(input, (int)startat);
                if (match.Success)
                    return Add(match);
                return -1;
            }
            catch(RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Searches a string for the first occurrence of a regex, beginning from the specified location, and returns a MATCH json object.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to search.</param>
        /// <param name="startat">The location to start from.</param>
        /// <param name="json_options">Additional json options (JO_*).</param>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matches?view=netframework-4.7#System_Text_RegularExpressions_Regex_Matches_System_String_System_Int32_)</ref>
        /// <returns>String (MATCH object) on success; otherwise ""</returns>
        [DllExport("MatchFromJson", CallingConvention.Cdecl)]
        public static string RegexMatchFromJson(double regex_id, string input, double startat, double json_options)
        {
            try
            {
                Match match = GetRegex(regex_id).Match(input, (int)startat);

                if (match.Success)
                    return MatchToJson(match, (JsonOptions)json_options);

                return string.Empty;
            }
            catch(RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the match after the given match.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.match.nextmatch?view=netframework-4.7#System_Text_RegularExpressions_Match_NextMatch)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <returns>Match id on success; Otherwise -1.</returns>
        [DllExport("MatchGetNextMatch", CallingConvention.Cdecl)]
        public static double MatchGetNextMatch(double match_id)
        {
            try
            {
                Match match = GetMatch(match_id).NextMatch();

                if (match.Success)
                    return Add(match);

                return -1;
            }
            catch(RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Gets the position in the original string where the match was found.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.index?view=netframework-4.7#System_Text_RegularExpressions_Capture_Index)</ref>
        /// <returns>Real</returns>
        [DllExport("MatchGetIndex", CallingConvention.Cdecl)]
        public static double MatchGetIndex(double match_id)
        {
            return GetMatch(match_id).Index;
        }

        /// <summary>
        /// Gets the length of the matched string.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.length?view=netframework-4.7#System_Text_RegularExpressions_Capture_Length)</ref>
        /// <returns>Real</returns>
        [DllExport("MatchGetLength", CallingConvention.Cdecl)]
        public static double MatchGetLength(double match_id)
        {
            return GetMatch(match_id).Length;
        }

        /// <summary>
        /// Gets the string that was matched.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.value?view=netframework-4.7#System_Text_RegularExpressions_Capture_Value)</ref>
        /// <returns>String</returns>
        [DllExport("MatchGetValue", CallingConvention.Cdecl)]
        public static string MatchGetValue(double match_id)
        {
            return GetMatch(match_id).Value;
        }

        /// <summary>
        /// Gets the matched string formatted as specified.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <param name="format">The replacement pattern to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.match.result?view=netframework-4.7#System_Text_RegularExpressions_Match_Result_System_String_)</ref>
        /// <exception cref="NotSupportedException">Returns ""</exception>
        /// <returns>String</returns>
        [DllExport("MatchGetResult", CallingConvention.Cdecl)]
        public static string MatchGetResult(double match_id, string format)
        {
            try
            {
                return GetMatch(match_id).Result(format);
            }
            catch (NotSupportedException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the groups captured by the match, and returns it as a GROUPS json object
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <param name="json_options">Additional json options (JO_*)</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.match.groups?view=netframework-4.7#System_Text_RegularExpressions_Match_Groups)</ref>
        /// <returns>String (GROUPS object)</returns>
        [DllExport("MatchGetGroupsJson", CallingConvention.Cdecl)]
        public static string MatchGetGroupsJson(double match_id, double json_options)
        {
            return GroupCollectionToJson(GetMatch(match_id).Groups, (JsonOptions)json_options);
        }

        /// <summary>
        /// Gets a group captured by the match via its name, and returns its id.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <param name="groupName">The name of the group to get.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.groupcollection.item?view=netframework-4.7#System_Text_RegularExpressions_GroupCollection_Item_System_String_)</ref>
        /// <returns>Group id.</returns>
        [DllExport("MatchGetGroupByName", CallingConvention.Cdecl)]
        public static double MatchGetGroupByName(double match_id, string groupName)
        {
            return Add(GetMatch(match_id).Groups[groupName]);
        }

        /// <summary>
        /// Gets a group captured by the match via it's name, and returns it as a GROUP json object.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <param name="name">The name of the group to get.</param>
        /// <param name="json_options">Additional json options (JO_*).</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.groupcollection.item?view=netframework-4.7#System_Text_RegularExpressions_GroupCollection_Item_System_String_)</ref>
        /// <returns>String (GROUP object)</returns>
        [DllExport("MatchGetGroupByNameJson", CallingConvention.Cdecl)]
        public static string MatchGetGroupByNameJson(double match_id, string name, double json_options)
        {
            return GroupToJson(GetMatch(match_id).Groups[name], (JsonOptions)json_options);
        }

        /// <summary>
        /// Gets a group captured by the match via its index, and returns its id.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <param name="index">The index of the group to get.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.groupcollection.item?view=netframework-4.7#System_Text_RegularExpressions_GroupCollection_Item_System_Int32_)</ref>
        /// <returns>Group id.</returns>
        [DllExport("MatchGetGroupByIndex", CallingConvention.Cdecl)]
        public static double MatchGetGroupByIndex(double match_id, double index)
        {
            return Add(GetMatch(match_id).Groups[(int)index]);
        }

        /// <summary>
        /// Gets a group captured by the match via its index, and returns it as a GROUP json object.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <param name="index">The index of the group to get.</param>
        /// <param name="json_options">Additional json options (JO_*).</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.groupcollection.item?view=netframework-4.7#System_Text_RegularExpressions_GroupCollection_Item_System_Int32_)</ref>
        /// <returns>String (GROUP object)</returns>
        [DllExport("MatchGetGroupByIndexJson", CallingConvention.Cdecl)]
        public static string MatchGetGroupByIndexJson(double match_id, double index, double json_options)
        {
            return GroupToJson(GetMatch(match_id).Groups[(int)index], (JsonOptions)json_options);
        }

        /// <summary>
        /// Gets the number of groups captured by a match.
        /// </summary>
        /// <param name="match_id">The id of the match to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.groupcollection.count?view=netframework-4.7#System_Text_RegularExpressions_GroupCollection_Count)</ref>
        /// <returns>Real</returns>
        [DllExport("MatchGetGroupCount", CallingConvention.Cdecl)]
        public static double MatchGetGroupCount(double match_id)
        {
            return GetMatch(match_id).Groups.Count;
        }

        /// <summary>
        /// Gets the position in the original string where the group was found.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.index?view=netframework-4.7#System_Text_RegularExpressions_Capture_Index)</ref>
        /// <returns>Real</returns>
        [DllExport("GroupGetIndex", CallingConvention.Cdecl)]
        public static double GroupGetIndex(double group_id)
        {
            return GetGroup(group_id).Index;
        }

        /// <summary>
        /// Gets the length of the string captured by the group.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.length?view=netframework-4.7#System_Text_RegularExpressions_Capture_Length)</ref>
        /// <returns>Real</returns>
        [DllExport("GroupGetLength", CallingConvention.Cdecl)]
        public static double GroupGetLength(double group_id)
        {
            return GetGroup(group_id).Length;
        }

        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.group.name?view=netframework-4.7#System_Text_RegularExpressions_Group_Name)</ref>
        /// <returns>String</returns>
        [DllExport("GroupGetName", CallingConvention.Cdecl)]
        public static string GroupGetName(double group_id)
        {
            return GetGroup(group_id).Name;
        }

        /// <summary>
        /// Returns whether or not the group successfully captured a string.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.group.success?view=netframework-4.7#System_Text_RegularExpressions_Group_Success)</ref>
        /// <returns>Bool</returns>
        [DllExport("GroupGetSuccess", CallingConvention.Cdecl)]
        public static double GroupGetSuccess(double group_id)
        {
            return GetGroup(group_id).Success ? 1 : 0;
        }

        /// <summary>
        /// Gets the value of the string captured by the group.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.value?view=netframework-4.7#System_Text_RegularExpressions_Capture_Value)</ref>
        /// <returns>String</returns>
        [DllExport("GroupGetValue", CallingConvention.Cdecl)]
        public static string GroupGetValue(double group_id)
        {
            return GetGroup(group_id).Value;
        }

        /// <summary>
        /// Gets all of the captures captured by the group, and returns them as a CAPTURES json object.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.group.captures?view=netframework-4.7#System_Text_RegularExpressions_Group_Captures)</ref>
        /// <returns>String (CAPTURES object)</returns>
        [DllExport("GroupGetCapturesJson", CallingConvention.Cdecl)]
        public static string GroupGetCapturesJson(double group_id)
        {
            return CaptureCollectionToJson(GetGroup(group_id).Captures);
        }

        /// <summary>
        /// Gets the number of captures captured by the group.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capturecollection.count?view=netframework-4.7#System_Text_RegularExpressions_CaptureCollection_Count)</ref>
        /// <returns>Real</returns>
        [DllExport("GroupGetCaptureCount", CallingConvention.Cdecl)]
        public static double GroupGetCaptureCount(double group_id)
        {
            return GetGroup(group_id).Captures.Count;
        }

        /// <summary>
        /// Gets a capture captured by the group via its index, and returns its id.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <param name="index">The index of the capture to get.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capturecollection.item?view=netframework-4.7#System_Text_RegularExpressions_CaptureCollection_Item_System_Int32_)</ref>
        /// <returns>Capture id</returns>
        [DllExport("GroupGetCapture", CallingConvention.Cdecl)]
        public static double GroupGetCapture(double group_id, double index)
        {
            return Add(GetGroup(group_id).Captures[(int)index]);
        }

        /// <summary>
        /// Gets a capture captured by the group via its id, and returns it as a CAPTURE json object.
        /// </summary>
        /// <param name="group_id">The id of the group to use.</param>
        /// <param name="index">The index of the capture to get.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capturecollection.item?view=netframework-4.7#System_Text_RegularExpressions_CaptureCollection_Item_System_Int32_)</ref>
        /// <returns>String (CAPTURE object)</returns>
        [DllExport("GroupGetCaptureJson", CallingConvention.Cdecl)]
        public static string GroupGetCaptureJson(double group_id, double index)
        {
            return CaptureToJson(GetGroup(group_id).Captures[(int)index]);
        }

        /// <summary>
        /// Gets the position in the original string where the capture was found.
        /// </summary>
        /// <param name="capture_id">The id of the capture to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.index?view=netframework-4.7#System_Text_RegularExpressions_Capture_Index)</ref>
        /// <returns>Real</returns>
        [DllExport("CaptureGetIndex", CallingConvention.Cdecl)]
        public static double CaptureGetIndex(double capture_id)
        {
            return GetCapture(capture_id).Index;
        }

        /// <summary>
        /// Gets the length of the captured string.
        /// </summary>
        /// <param name="capture_id">The id of the capture to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.length?view=netframework-4.7#System_Text_RegularExpressions_Capture_Length)</ref>
        /// <returns>Real</returns>
        [DllExport("CaptureGetLength", CallingConvention.Cdecl)]
        public static double CaptureGetLength(double capture_id)
        {
            return GetCapture(capture_id).Length;
        }

        /// <summary>
        /// Gets the string that was captured.
        /// </summary>
        /// <param name="capture_id">The id of the capture to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture.value?view=netframework-4.7#System_Text_RegularExpressions_Capture_Value)</ref>
        /// <returns>String</returns>
        [DllExport("CaptureGetValue", CallingConvention.Cdecl)]
        public static string CaptureGetValue(double capture_id)
        {
            return GetCapture(capture_id).Value;
        }

        /// <summary>
        /// Replaces all occurrences of a regex in the input string, and returns the resulting string.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to modify.</param>
        /// <param name="replacement">The value to replace matches with.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.replace?view=netframework-4.7#System_Text_RegularExpressions_Regex_Replace_System_String_System_String_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <returns>String</returns>
        [DllExport("Replace", CallingConvention.Cdecl)]
        public static string RegexReplace(double regex_id, string input, string replacement)
        {
            try
            {
                return GetRegex(regex_id).Replace(input, replacement);
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Replaces up to a given amount, occurrences of a regex in the input string, and returns the resulting string.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to modify.</param>
        /// <param name="replacement">The value to replace matches with.</param>
        /// <param name="count">The maximum number of replacements.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.replace?view=netframework-4.7#System_Text_RegularExpressions_Regex_Replace_System_String_System_String_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <returns>String</returns>
        [DllExport("ReplaceNumber", CallingConvention.Cdecl)]
        public static string RegexReplaceNumber(double regex_id, string input, string replacement, double count)
        {
            try
            {
                return GetRegex(regex_id).Replace(input, replacement, (int)count);
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Replaces all occurrences of a regex in the input string, beginning at the specified location, and returns the resulting string.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to modify.</param>
        /// <param name="replacement">The value to replace matches with.</param>
        /// <param name="startAt">The position to start from.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.replace?view=netframework-4.7#System_Text_RegularExpressions_Regex_Replace_System_String_System_String_System_Int32_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <returns>String</returns>
        [DllExport("ReplaceFrom", CallingConvention.Cdecl)]
        public static string RegexReplaceFrom(double regex_id, string input, string replacement, double startAt)
        {
            try
            {
                return GetRegex(regex_id).Replace(input, replacement, -1, (int)startAt);
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Splits the input string into a json array.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to split.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.7#System_Text_RegularExpressions_Regex_Split_System_String_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <returns>String (ARRAY object)</returns>
        [DllExport("SplitJson", CallingConvention.Cdecl)]
        public static string RegexSplitJson(double regex_id, string input)
        {
            try
            {
                return SplitToJson(GetRegex(regex_id).Split(input));
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Splits the input string into a json array with a maximum amount of splits.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to split.</param>
        /// <param name="count">The maximum amount of splits.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.7#System_Text_RegularExpressions_Regex_Split_System_String_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <returns>String (ARRAY object)</returns>
        [DllExport("SplitCountJson", CallingConvention.Cdecl)]
        public static string RegexSplitCountJson(double regex_id, string input, double count)
        {
            try
            {
                return SplitToJson(GetRegex(regex_id).Split(input, (int)count));
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Splits the input string into a json array beginning at the specified location.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to split.</param>
        /// <param name="startAt">The position to start from.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.7#System_Text_RegularExpressions_Regex_Split_System_String_System_Int32_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns ""</exception>
        /// <returns>String (ARRAY object)</returns>
        [DllExport("SplitFromJson", CallingConvention.Cdecl)]
        public static string RegexSplitFromJson(double regex_id, string input, double startAt)
        {
            try
            {
                return SplitToJson(GetRegex(regex_id).Split(input, -1, (int)startAt));
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Splits the input string, and returns the split id.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to split.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.7#System_Text_RegularExpressions_Regex_Split_System_String_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <returns>Split id</returns>
        [DllExport("Split", CallingConvention.Cdecl)]
        public static double _regex_split(double regex_id, string input)
        {
            try
            {
                return Add(GetRegex(regex_id).Split(input));
            }
            catch (RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Splits the input string with a given amount of splits and returns the split id.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to split.</param>
        /// <param name="count">The maximum amount of splits.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.7#System_Text_RegularExpressions_Regex_Split_System_String_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <returns>Split id</returns>
        [DllExport("SplitCount", CallingConvention.Cdecl)]
        public static double _regex_split_count(double regex_id, string input, double count)
        {
            try
            {
                return Add(GetRegex(regex_id).Split(input, (int)count));
            }
            catch (RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Splits the input string beginning at the specified location, and returns the split id.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to split.</param>
        /// <param name="startAt">The position to start from.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.7#System_Text_RegularExpressions_Regex_Split_System_String_System_Int32_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <returns>Split id</returns>
        [DllExport("SplitFrom", CallingConvention.Cdecl)]
        public static double _regex_split_from(double regex_id, string input, double startAt)
        {
            try
            {
                return Add(GetRegex(regex_id).Split(input, -1, (int)startAt));
            }
            catch (RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Splits the input string beginning at the specified location with a maximum amount of splits, and returns the split id.
        /// </summary>
        /// <param name="regex_id">The id of the regex to use.</param>
        /// <param name="input">The string to split.</param>
        /// <param name="count">The maximum amount of splits.</param>
        /// <param name="startAt">The position to start from.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.7#System_Text_RegularExpressions_Regex_Split_System_String_System_Int32_System_Int32_)</ref>
        /// <exception cref="RegexMatchTimeoutException">Returns -2</exception>
        /// <returns>Split id</returns>
        [DllExport("SplitFromCount", CallingConvention.Cdecl)]
        public static double _regex_split_from_count(double regex_id, string input, double count, double startAt)
        {
            try
            {
                return Add(GetRegex(regex_id).Split(input, (int)count, (int)startAt));
            }
            catch (RegexMatchTimeoutException)
            {
                return -2;
            }
        }

        /// <summary>
        /// Gets the size of the split in bytes. Used to create a buffer of the appropriate size.
        /// </summary>
        /// <param name="split_id">The id of the split to use.</param>
        /// <returns>Real</returns>
        [DllExport("SplitGetSize", CallingConvention.Cdecl)]
        public static double _split_get_size(double split_id)
        {
            int i = 0;
            foreach(var split in GetSplit(split_id))
            {
                i += split.Length + 1;
            }
            return i;
        }

        /// <summary>
        /// Gets the number of strings in the split.
        /// </summary>
        /// <param name="split_id">The id of the split to use.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.array.length?view=netframework-4.7#System_Array_Length)</ref>
        /// <returns>Real</returns>
        [DllExport("SplitGetCount", CallingConvention.Cdecl)]
        public static double _split_get_count(double split_id)
        {
            return GetSplit(split_id).Length;
        }
        
        /// <summary>
        /// Gets the string at the given index in the split.
        /// </summary>
        /// <param name="split_id">The id of the split to use.</param>
        /// <param name="index">The index of the string to get.</param>
        /// <returns>String</returns>
        [DllExport("SplitGetIndex", CallingConvention.Cdecl)]
        public static string _split_get_index(double split_id, double index)
        {
            return GetSplit(split_id)[(int)index];
        }

        /// <summary>
        /// Transfers data from a split to a buffer.
        /// </summary>
        /// <param name="split_id">The id of the split to use.</param>
        /// <param name="buffer">The memory address of the buffer.</param>
        /// <returns>1</returns>
        [DllExport("SplitFillBuffer", CallingConvention.Cdecl)]
        public static unsafe double _split_fill_buffer(double split_id, char* buffer)
        {
            var ptr = new IntPtr(buffer);
            var split = GetSplit(split_id);
            var pos = 0;
            for(var i = 0; i < split.Length; i++)
            {
                Marshal.Copy(Encoding.ASCII.GetBytes(split[i]), 0, ptr + pos, split[i].Length);
                pos += split[i].Length;
                buffer[pos++] = '\0';
            }
            return 1;
        }

        /// <summary>
        /// Gets the size of the compiled regex cache.
        /// </summary>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.cachesize?view=netframework-4.7#System_Text_RegularExpressions_Regex_CacheSize)</ref>
        /// <returns>Real</returns>
        [DllExport("RegexGetCacheSize", CallingConvention.Cdecl)]
        public static double RegexGetCacheSize()
        {
            return Regex.CacheSize;
        }

        /// <summary>
        /// Sets the size of the compiled regex cache.
        /// </summary>
        /// <param name="size">The new size of the cache.</param>
        /// <ref>[Microsoft](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.cachesize?view=netframework-4.7#System_Text_RegularExpressions_Regex_CacheSize)</ref>
        /// <returns>True if success; Otherwise false</returns>
        [DllExport("RegexSetCacheSize", CallingConvention.Cdecl)]
        public static double RegexSetCacheSize(double size)
        {
            if ((int)size < 1)
                return 0;
            Regex.CacheSize = (int)size;
            return 1;
        }

        private static int Add(object obj)
        {
            int index = 0;
            if (_openSlots.Count == 0)
            {
                index = _slots.Count;
                _slots.Add(obj);
            }
            else
            {
                index = _openSlots.Dequeue();
                _slots[index] = obj;
            }
            return index;
        }

        /// <summary>
        /// Converts a <see cref="MatchCollection"/> into a JSON string that's compatible with Gamemaker.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string MatchCollectionToJson(MatchCollection collection, JsonOptions options)
        {
            var sb = new StringBuilder("[ ");
            int count = 0;
            foreach (Match match in collection)
            {
                if (count++ > 0)
                    sb.Append(", ");
                sb.Append(MatchToJson(match, options));
            }
            sb.Append(" ]");
            return sb.ToString();
        }

        private static string MatchToJson(Match match, JsonOptions options)
        {
            var sb = new StringBuilder($"{{ \"index\": {match.Index}, \"length\": {match.Length}, \"success\": {(match.Success ? 1 : 0)}, \"value\": \"{match.Value}\"");
            if (options.HasFlag(JsonOptions.Groups))
            {
                sb.Append(", \"groups\": ");
                sb.Append(GroupCollectionToJson(match.Groups, options));
            }
            sb.Append(" }");
            return sb.ToString();
        }

        private static string GroupCollectionToJson(GroupCollection collection, JsonOptions options)
        {
            var sb = new StringBuilder("[ ");
            var count = 0;
            foreach (Group group in collection)
            {
                if (count++ > 0)
                    sb.Append(", ");
                sb.Append(GroupToJson(group, options));
            }
            sb.Append(" ]");
            return sb.ToString();
        }

        private static string GroupToJson(Group group, JsonOptions option)
        {
            var sb = new StringBuilder($"{{ \"index\": {group.Index}, \"length\": {group.Length}, \"success\": {(group.Success ? 1 : 0)}, \"name\": \"{group.Name}\", \"value\": \"{group.Value}\"");
            if (option.HasFlag(JsonOptions.Captures))
            {
                sb.Append(", \"captures\": ");
                sb.Append(CaptureCollectionToJson(group.Captures));
            }
            sb.Append(" }");
            return sb.ToString();
        }

        private static string CaptureCollectionToJson(CaptureCollection collection)
        {
            var sb = new StringBuilder("[ ");
            var count = 0;
            foreach (Capture capture in collection)
            {
                if (count++ > 0)
                    sb.Append(", ");
                sb.Append(CaptureToJson(capture));
            }
            sb.Append(" ]");
            return sb.ToString();
        }

        private static string CaptureToJson(Capture capture)
        {
            return $"{{ \"index\": {capture.Index}, \"length\": {capture.Length}, \"value\": \"{capture.Value}\" }}";
        }

        private static string SplitToJson(string[] strings)
        {
            var sb = new StringBuilder("[ ");
            var count = 0;
            foreach (var split in strings)
            {
                if (count++ > 0)
                    sb.Append(", ");
                sb.Append('"');
                sb.Append(split);
                sb.Append("\"");
            }
            sb.Append(" ]");
            return sb.ToString();
        }

        private static Regex GetRegex(double index)
        {
            var i = (int)index;

            if (i < 0 || i >= _slots.Count || _slots[i] == null || !(_slots[i] is Regex regex))
                throw new ArgumentOutOfRangeException("id", $"Regex with the id {i} does not exist.");

            return regex;
        }

        private static MatchCollection GetMatches(double index)
        {
            var i = (int)index;

            if (i < 0 || i >= _slots.Count || _slots[i] == null || !(_slots[i] is MatchCollection matches))
                throw new ArgumentOutOfRangeException("id", $"Match with the id {i} does not exist.");

            return matches;
        }

        private static Match GetMatch(double index)
        {
            var i = (int)index;

            if (i < 0 || i >= _slots.Count || _slots[i] == null || !(_slots[i] is Match match))
                throw new ArgumentOutOfRangeException("id", $"Match with the id {i} does not exist.");

            return match;
        }

        private static Group GetGroup(double index)
        {
            var i = (int)index;

            if (i < 0 || i >= _slots.Count || _slots[i] == null || !(_slots[i] is Group group))
                throw new ArgumentOutOfRangeException("id", $"Match with the id {i} does not exist.");

            return group;
        }

        private static Capture GetCapture(double index)
        {
            var i = (int)index;

            if (i < 0 || i >= _slots.Count || _slots[i] == null || !(_slots[i] is Capture capture))
                throw new ArgumentOutOfRangeException("id", $"Match with the id {i} does not exist.");

            return capture;
        }

        private static string[] GetSplit(double index)
        {
            var i = (int)index;

            if (i < 0 || i >= _slots.Count || _slots[i] == null || !(_slots[i] is string[] split))
                throw new ArgumentOutOfRangeException("id", $"Match with the id {i} does not exist.");

            return split;
        }
    }

    [Flags]
    internal enum JsonOptions
    {
        None = 0,
        Groups = 1,
        Captures = 2
    }
}
