﻿//   Copyright (c) Thortech Solutions, LLC. All rights reserved.
//   The use and distribution terms for this software are covered by the
//   Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php)
//   which can be found in the file epl-v10.html at the root of this distribution.
//   By using this software in any fashion, you are agreeing to be bound by
//   the terms of this license.
//   You must not remove this notice, or any other, from this software.
//
//   Authors: Dimitrios Kapsalis, Mark Perrotta
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EDNTypes;

using EDNReaderWriter;

namespace EDNReaderTestCS
{
    /// <summary>
    /// Sample EDN reader handler. Has the following custom behavior
    /// - Converts EDN maps to Dictionary<object, object> instead of EDNMap.
    /// - Handles System.TimeZoneInfo via a special tag. See the SampleCustomPrinter for writing of the type. 
    /// </summary>
    public class SampleCustomHandler : TypeHandlers.BaseTypeHandler
    {
        public override object handleValue(EDNParserTypes.EDNValueParsed ednValueParsed)
        {
            if (ednValueParsed.ednValue.IsEDNMap)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                var enumerator = ((EDNParserTypes.EDNValue.EDNMap)ednValueParsed.ednValue).Item.AsEnumerable().GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var key = handleValue(enumerator.Current);
                    if (!enumerator.MoveNext())
                        throw new Exception("Map must have even number of elements");

                    var value = handleValue(enumerator.Current);
                    dict.Add(key.ToString(), value.ToString());
                }

                return dict;
            }
            else
                return base.handleValue(ednValueParsed);
        }

        public override object handleTag(Tuple<EDNParserTypes.QualifiedSymbol, EDNParserTypes.EDNValueParsed> tagAndValue)
        {
            var tag = tagAndValue.Item1;
            var value = tagAndValue.Item2;

            if (tag.prefix == "sample-custom-type" && tag.name == "timezone")
            {
                if(value.ednValue.IsEDNMap)
                {
                    var timeZoneMap = ((Dictionary<string, string>)handleValue(value));

                    return System.TimeZoneInfo.CreateCustomTimeZone(
                        timeZoneMap["Id"].ToString(),
                        System.TimeSpan.Parse(timeZoneMap["BaseUtcOffset"].ToString()),
                        timeZoneMap["DisplayName"].ToString(),
                        timeZoneMap["StandardDisplayName"].ToString());
                }
                else
                    throw new Exception(string.Format("{0} is not a valid sample-custom-type/timezone object", value.ednValue.ToString()));
            }
            else
                return base.handleTag(tagAndValue);
        }
    }
}
