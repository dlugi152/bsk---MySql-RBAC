using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace bsk___proba_2
{
    public class Scanner
    {
        private readonly Hashtable typePatterns;

        public Scanner() {
            typePatterns = new Hashtable {
                {"String", @"[\w\d\S]+"},
                {"Int16", @"-[0-9]+|[0-9]+"},
                {"UInt16", @"[0-9]+"},
                {"Int32", @"-[0-9]+|[0-9]+"},
                {"UInt32", @"[0-9]+"},
                {"Int64", @"-[0-9]+|[0-9]+"},
                {"UInt64", @"[0-9]+"},
                {"Single", @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+"},
                {"Double", @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+"},
                {"Boolean", @"true|false"},
                {"Byte", @"[0-9]{1,3}"},
                {"SByte", @"-[0-9]{1,3}|[0-9]{1,3}"},
                {"Char", @"[\w\S]{1}"},
                {"Decimal", @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+"}
            };

        }

        /// <summary>
        /// Scan memics scanf.
        /// A master regular expression pattern is created that will group each "word" in the text and using regex grouping
        /// extract the values for the field specifications.
        /// Example text: "Hello true 6.5"  fieldSpecification: "{String} {Boolean} {Double}"
        /// The fieldSpecification will result in the generation of a master Pattern:
        /// ([\w\d\S]+)\s+(true|false)\s+([-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+)
        /// This masterPattern is ran against the text string and the groups are extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fieldSpecification">A string that may contain simple field specifications of the form {Int16}, {String}, etc</param>
        /// <returns>object[] that contains values for each field</returns>
        public object[] Scan(string text, string fieldSpecification) {
            fieldSpecification = ConvertSpecification(fieldSpecification);
            text = text.Replace("'", "");
            object[] targets;
            try {
                ArrayList targetMatchGroups = new ArrayList();
                ArrayList targetTypes = new ArrayList();

                //masterPattern is going to hold a "big" regex pattern that will be ran against the original text
                string masterPattern = fieldSpecification.Trim();
                string matchingPattern = @"(\S+)";
                masterPattern = Regex.Replace(masterPattern, matchingPattern, "($1)"); //insert grouping parens

                //store the group location of the format tags so that we can select the correct group values later.
                matchingPattern = @"(\([\w\d\S]+\))";
                Regex reggie = new Regex(matchingPattern);
                MatchCollection matches = reggie.Matches(masterPattern);
                for (int i = 0; i < matches.Count; i++) {
                    Match m = matches[i];
                    string sVal = m.Groups[1].Captures[0].Value;

                    //is this value a {n} value. We will determine this by checking for {
                    if (sVal.IndexOf('{') >= 0) {
                        targetMatchGroups.Add(i);
                        string p = @"\(\{(\w*)\}\)"; //pull out the type
                        sVal = Regex.Replace(sVal, p, "$1");
                        targetTypes.Add(sVal);
                    }
                }

                //Replace all of the types with the pattern that matches that type
                masterPattern = Regex.Replace(masterPattern, @"\{String\}", (string)typePatterns["String"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Int16\}", (string)typePatterns["Int16"]);
                masterPattern = Regex.Replace(masterPattern, @"\{UInt16\}", (string)typePatterns["UInt16"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Int32\}", (string)typePatterns["Int32"]);
                masterPattern = Regex.Replace(masterPattern, @"\{UInt32\}", (string)typePatterns["UInt32"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Int64\}", (string)typePatterns["Int64"]);
                masterPattern = Regex.Replace(masterPattern, @"\{UInt64\}", (string)typePatterns["UInt64"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Single\}", (string)typePatterns["Single"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Double\}", (string)typePatterns["Double"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Boolean\}", (string)typePatterns["Boolean"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Byte\}", (string)typePatterns["Byte"]);
                masterPattern = Regex.Replace(masterPattern, @"\{SByte\}", (string)typePatterns["SByte"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Char\}", (string)typePatterns["Char"]);
                masterPattern = Regex.Replace(masterPattern, @"\{Decimal\}", (string)typePatterns["Decimal"]);

                masterPattern =
                    Regex.Replace(masterPattern, @"\s+",
                                  "\\s+"); //replace the white space with the pattern for white space

                //run our generated pattern against the original text.
                reggie = new Regex(masterPattern);
                matches = reggie.Matches(text);
                //PrintMatches(matches);

                //allocate the targets
                targets = new object[targetMatchGroups.Count];
                for (int x = 0; x < targetMatchGroups.Count; x++) {
                    int i = (int)targetMatchGroups[x];
                    string tName = (string)targetTypes[x];
                    if (i < matches[0].Groups.Count) {
                        //add 1 to i because i is a result of serveral matches each resulting in one group.
                        //this query is one match resulting in serveral groups.
                        string sValue = matches[0].Groups[i + 1].Captures[0].Value;
                        targets[x] = ReturnValue(tName, sValue);
                    }
                }
            }
            catch (Exception ex) {
                throw new ScanExeption("Scan exception", ex);
            }

            return targets;
        }

        private static string ConvertSpecification(string fieldSpecification) {
            fieldSpecification = fieldSpecification.Replace("%s", "{String}");
            fieldSpecification = fieldSpecification.Replace("%ld", "{Int32}");
            fieldSpecification = fieldSpecification.Replace("%d", "{Int32}");
            fieldSpecification = fieldSpecification.Replace("'", "");
            return fieldSpecification;
        }

        /// <summary>
        /// Return the Value inside of an object that boxes the built in type or references the string
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="sValue"></param>
        /// <returns></returns>
        private object ReturnValue(string typeName, string sValue) {
            object o = null;
            switch (typeName) {
                case "String":
                    o = sValue;
                    break;

                case "Int16":
                    o = short.Parse(sValue);
                    break;

                case "UInt16":
                    o = ushort.Parse(sValue);
                    break;

                case "Int32":
                    o = int.Parse(sValue);
                    break;

                case "UInt32":
                    o = uint.Parse(sValue);
                    break;

                case "Int64":
                    o = long.Parse(sValue);
                    break;

                case "UInt64":
                    o = ulong.Parse(sValue);
                    break;

                case "Single":
                    o = float.Parse(sValue);
                    break;

                case "Double":
                    o = double.Parse(sValue);
                    break;

                case "Boolean":
                    o = bool.Parse(sValue);
                    break;

                case "Byte":
                    o = byte.Parse(sValue);
                    break;

                case "SByte":
                    o = sbyte.Parse(sValue);
                    break;

                case "Char":
                    o = char.Parse(sValue);
                    break;

                case "Decimal":
                    o = decimal.Parse(sValue);
                    break;
            }
            return o;
        } //ReturnValue

        /// <summary>
        /// Return a pattern for regular expressions that will match the built in type specified by name
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private string ReturnPattern(string typeName) {
            string innerPattern = "";
            switch (typeName) {
                case "Int16":
                    innerPattern = (string)typePatterns["Int16"];
                    break;

                case "UInt16":
                    innerPattern = (string)typePatterns["UInt16"];
                    break;

                case "Int32":
                    innerPattern = (string)typePatterns["Int32"];
                    break;

                case "UInt32":
                    innerPattern = (string)typePatterns["UInt32"];
                    break;

                case "Int64":
                    innerPattern = (string)typePatterns["Int64"];
                    break;

                case "UInt64":
                    innerPattern = (string)typePatterns["UInt64"];
                    break;

                case "Single":
                    innerPattern = (string)typePatterns["Single"];
                    break;

                case "Double":
                    innerPattern = (string)typePatterns["Double"];
                    break;

                case "Boolean":
                    innerPattern = (string)typePatterns["Boolean"];
                    break;

                case "Byte":
                    innerPattern = (string)typePatterns["Byte"];
                    break;

                case "SByte":
                    innerPattern = (string)typePatterns["SByte"];
                    break;

                case "Char":
                    innerPattern = (string)typePatterns["Char"];
                    break;

                case "Decimal":
                    innerPattern = (string)typePatterns["Decimal"];
                    break;

                case "String":
                    innerPattern = (string)typePatterns["String"];
                    break;
            }
            return innerPattern;
        } //ReturnPattern
    }

    /// <summary>
    /// Exceptions that are thrown by this namespace and the Scanner Class
    /// </summary>
    class ScanExeption : Exception
    {
        public ScanExeption(string message, Exception inner) : base(message, inner) { }
    }
}