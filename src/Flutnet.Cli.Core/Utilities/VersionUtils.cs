using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Flutnet.Cli.Core.Utilities
{
    internal class VersionUtils
    {
        /// <summary>
        /// Hashtable of the greek letters for quick lookup.
        /// </summary>
        static readonly Hashtable GreekLetters = new Hashtable
        {
            {"alpha", 0},
            {"beta", 1},
            {"gamma", 2},
            {"delta", 3},
            {"epsilon", 4},
            {"zeta", 5},
            {"eta", 6},
            {"theta", 7},
            {"iota", 8},
            {"kappa", 9},
            {"lambda", 10},
            {"mu", 11},
            {"nu", 12},
            {"xi", 13},
            {"omicron", 14},
            {"pi", 15},
            {"rho", 16},
            {"sigma", 17},
            {"tau", 18},
            {"upsilon", 19},
            {"phi", 20},
            {"chi", 21},
            {"psi", 22},
            {"omega", 23},
            {"rc", 24} // RC = release candidate
        };

        /// <summary>
        /// Compares two versions and returns an integer that indicates their relationship in the sort order.
        /// </summary>
        /// <param name="versionA">The first version to compare.</param>
        /// <param name="versionB">The second version to compare.</param>
        /// <returns>Return a negative number if versionA is less than versionB, 0 if they're equal, a positive number if versionA is greater than versionB.</returns>
        public static int Compare(string versionA, string versionB)
        {
            if (versionA == null) return -1;
            if (versionB == null) return 1;

            // Convert version to lowercase, and
            // replace all instances of "release candidate" with "rc"
            versionA = Regex.Replace(versionA.ToLowerInvariant(), @"release[\s]+candidate", "rc");
            versionB = Regex.Replace(versionB.ToLowerInvariant(), @"release[\s]+candidate", "rc");

            //compare indices
            int iVerA = 0, iVerB = 0;

            bool lastAWasLetter = true, lastBWasLetter = true;

            for (; ; )
            {
                //store index before GetNextObject just in case we need to rollback
                int greekIndA = iVerA;
                int greekIndB = iVerB;

                string objA = GetNextObject(versionA, ref iVerA, ref lastAWasLetter);
                string objB = GetNextObject(versionB, ref iVerB, ref lastBWasLetter);

                //normalize versions so comparing integer against integer, 
                //(i.e. "1 a" is expanded to "1.0.0 a" when compared with "1.0.0 XXX")
                //also, rollback the index on the version modified
                if ((!lastBWasLetter && objB != null) && (objA == null || lastAWasLetter))
                {
                    objA = "0";
                    iVerA = greekIndA;
                }
                else if ((!lastAWasLetter && objA != null) && (objB == null || lastBWasLetter))
                {
                    objB = "0";
                    iVerB = greekIndB;
                }

                // find greek index for A and B
                greekIndA = lastAWasLetter ? GetGreekIndex(objA) : -1;
                greekIndB = lastBWasLetter ? GetGreekIndex(objB) : -1;

                if (objA == null && objB == null)
                    return 0; //versions are equal

                if (objA == null) // objB != null
                {
                    //if versionB has a greek word, then A is greater
                    if (greekIndB != -1)
                        return 1;

                    return -1;
                }

                if (objB == null) // objA != null
                {
                    //if versionA has a greek word, then B is greater
                    if (greekIndA != -1)
                        return -1;

                    return 1;
                }

                if (char.IsDigit(objA[0]) == char.IsDigit(objB[0]))
                {
                    int strComp;
                    if (char.IsDigit(objA[0]))
                    {
                        //compare integers
                        strComp = IntCompare(objA, objB);

                        if (strComp != 0)
                            return strComp;
                    }
                    else
                    {
                        if (greekIndA == -1 && greekIndB == -1)
                        {
                            //compare non-greek strings
                            strComp = string.Compare(objA, objB, StringComparison.Ordinal);

                            if (strComp != 0)
                                return strComp;
                        }
                        else if (greekIndA == -1)
                            return 1; //versionB has a greek word, thus A is newer
                        else if (greekIndB == -1)
                            return -1; //versionA has a greek word, thus B is newer
                        else
                        {
                            //compare greek words
                            if (greekIndA > greekIndB)
                                return 1;

                            if (greekIndB > greekIndA)
                                return -1;
                        }
                    }
                }
                else if (char.IsDigit(objA[0]))
                    return 1; //versionA is newer than versionB
                else
                    return -1; //versionB is newer than versionA
            }
        }
        private static string GetNextObject(string version, ref int index, ref bool lastWasLetter)
        {
            //1 == string, 2 == int, -1 == neither
            int isStringOrInt = -1;

            int startIndex = index;

            while (version.Length != index)
            {
                if (isStringOrInt == -1)
                {
                    if (char.IsLetter(version[index]))
                    {
                        startIndex = index;
                        isStringOrInt = 1;
                    }
                    else if (char.IsDigit(version[index]))
                    {
                        startIndex = index;
                        isStringOrInt = 2;
                    }
                    else if (lastWasLetter && !char.IsWhiteSpace(version[index]))
                    {
                        index++;
                        lastWasLetter = false;
                        return "0";
                    }
                }
                else if (isStringOrInt == 1 && !char.IsLetter(version[index]))
                    break;
                else if (isStringOrInt == 2 && !char.IsDigit(version[index]))
                    break;

                index++;
            }

            // set the last "type" retrieved
            lastWasLetter = isStringOrInt == 1;

            // return the retrieved sub-string
            if (isStringOrInt == 1 || isStringOrInt == 2)
                return version.Substring(startIndex, index - startIndex);

            // was neither a string nor and int
            return null;
        }

        /// <summary>
        /// Checks if the string chunk is a greek letter (e.g. "beta") and if so, return the relative index used for comparison.
        /// </summary>
        /// <param name="str">The string chunk to check.</param>
        /// <returns>Returns -1 if it's not a greek letter. Otherwise the relative index is returned.</returns>
        private static int GetGreekIndex(object str)
        {
            object val = GreekLetters.ContainsKey(str)
                ? GreekLetters[str]
                : null;

            if (val == null)
                return -1;

            return (int) val;
        }

        /// <summary>
        /// Compare integers of "infinite" length without converting to an integer type.
        /// </summary>
        /// <param name="a">The first integer to compare.</param>
        /// <param name="b">The second integer to compare.</param>
        /// <returns>Returns less than 0 if "a" is less than "b", zero if "a" == "b", greater than zero if "a" is greater than "b".</returns>
        private static int IntCompare(string a, string b)
        {
            int lastZero = -1;

            // Clear any preceding zeros

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != '0')
                    break;

                lastZero = i;
            }

            if (lastZero != -1)
                a = a.Substring(lastZero + 1, a.Length - (lastZero + 1));

            lastZero = -1;

            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] != '0')
                    break;

                lastZero = i;
            }

            if (lastZero != -1)
                b = b.Substring(lastZero + 1, b.Length - (lastZero + 1));

            if (a.Length > b.Length)
                return 1;

            if (a.Length < b.Length)
                return -1;

            return string.Compare(a, b, StringComparison.Ordinal);
        }
    }
}
