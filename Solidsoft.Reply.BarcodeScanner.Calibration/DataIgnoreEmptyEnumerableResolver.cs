// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalibrationDataIgnoreEmptyEnumerableResolver.cs" company="Solidsoft Reply Ltd.">
//   (c) 2020 Solidsoft Reply Ltd. All rights reserved.
// </copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>
// <summary>
// A JSON contract resolver. Ignores empty enumerable properties and fields on calibration data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
///   A JSON contract resolver. Ignores empty enumerable properties and fields on calibration data.
/// </summary>
public class DataIgnoreEmptyEnumerableResolver : DefaultContractResolver
{
    /// <summary>
    ///   Creates a JSON property with conditional serialization.
    /// </summary>
    /// <param name="member">The member that will be conditionally serialized.</param>
    /// <param name="memberSerialization">The member serialization options.</param>
    /// <returns></returns>
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyType != typeof(string) &&
            (typeof(IEnumerable<KeyValuePair<char, char>>).IsAssignableFrom(property.PropertyType) ||
             typeof(IEnumerable<KeyValuePair<string, char>>).IsAssignableFrom(property.PropertyType) ||
             typeof(IEnumerable<KeyValuePair<string, string>>).IsAssignableFrom(property.PropertyType) ||
             typeof(IEnumerable<KeyValuePair<char, char>>).IsAssignableFrom(property.PropertyType) ||
             typeof(IEnumerable<char>).IsAssignableFrom(property.PropertyType) ||
             typeof(IEnumerable<string>).IsAssignableFrom(property.PropertyType) ||
             typeof(IEnumerable<Information>).IsAssignableFrom(property.PropertyType)))
        {
            property.ShouldSerialize = instance =>
            {
                // this value could be in a public field or public property
                var enumerable = member.MemberType switch
                                 {
                                     MemberTypes.Property => instance.GetType().GetProperty(member.Name)?.GetValue(instance, null) as
                                                                 IEnumerable,
                                     MemberTypes.Field => instance.GetType().GetField(member.Name)?.GetValue(instance) as IEnumerable,
                                     _ => null
                                 };

                var enumerator = enumerable?.GetEnumerator();
                using var disposableEnumerator = enumerator as IDisposable;

                return enumerable is null 
                       || enumerator is null
                       || enumerator.MoveNext();
                // if the list is null, we defer the decision to NullValueHandling
            };
        }

        return property;
    }
}