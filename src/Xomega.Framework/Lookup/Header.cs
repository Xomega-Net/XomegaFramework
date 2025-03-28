﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Xomega.Framework
{
    /// <summary>
    /// A general-purpose class that represents the header information of any object,
    /// that includes the most relevant fields to identify the object and any additional attributes
    /// that can be used for filtering or to support various display options.
    /// The Type string of a header determines the class of objects it represents.
    /// It has also a string based internal ID and a Text field for display purposes.
    /// It can also have any number of additional named attributes that can hold any value or a list of values.
    /// </summary>
    /// <remarks>
    /// Headers can be converted to a string representation with an arbitrary format,
    /// which can contain any combination of the header's ID, Text or any of its additional attributes.
    /// </remarks>
    [DataContract]
    [KnownType(typeof(List<object>))]
    public class Header : DynamicObject, IComparable
    {
        /// <summary>
        /// A constant that represents the ID field when used as part of the display format.
        /// </summary>
        public const string FieldId = "[i]";

        /// <summary>
        /// A constant that represents the Text field when used as part of the display format.
        /// </summary>
        public const string FieldText = "[t]";

        /// <summary>
        /// A constant that represents a named attribute when used as part of the display format.
        /// The placeholder {0} should be replaced with the attribute name by calling
        /// <c>String.Format(AttrPattern, attrName);</c>
        /// </summary>
        public const string AttrPattern = "[a:{0}]";

        /// <summary>
        /// Dummy parameterless constructor to satisfy creation by Activator.
        /// </summary>
        public Header()
        {
            IsValid = false;
        }

        /// <summary>
        /// Constructs an invalid header of the given type with just an ID.
        /// </summary>
        /// <param name="type">The type string for the header.</param>
        /// <param name="id">The ID string for the header.</param>
        public Header(string type, string id)
            : this(type, id, id)
        {
            IsValid = false;
        }

        /// <summary>
        ///  Constructs a valid header of the given type with the specified ID and text.
        /// </summary>
        /// <param name="type">The type string for the header.</param>
        /// <param name="id">The ID string for the header.</param>
        /// <param name="text">The user friendly text that identifies this header.</param>
        public Header(string type, string id, string text)
        {
            Type = type;
            Id = id;
            Text = text;
            IsValid = true;
            IsActive = true;
        }

        /// <summary>
        /// Constructs a deep copy of a header from another header.
        /// The copies of the attribute values are shallow though.
        /// </summary>
        /// <param name="hdr">Another header to copy from.</param>
        public Header(Header hdr)
        {
            Type = hdr.Type;
            Id = hdr.Id;
            Text = hdr.Text;
            IsValid = hdr.IsValid;
            IsActive = hdr.IsActive;
            DefaultFormat = hdr.DefaultFormat;

            foreach (string attr in hdr.Attributes.Keys)
                this[attr] = hdr[attr];
        }

        /// <summary>
        /// Constructs a clone of the current header that is of the same type as this one.
        /// </summary>
        /// <returns>A clone of the current header.</returns>
        public virtual Header Clone() { return new Header(this); }

        /// <summary>
        /// The Type string of a header determines the class of objects it represents.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// String-based ID that should be unique for all headers of the given type.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// A user friendly text that identifies this header.
        /// </summary>
        [DataMember]
        public string Text { get; set; }

        /// <summary>
        /// A flag indicating if the header was properly constructed with both ID and the text.
        /// This is typically False if the header was not found in the corresponding lookup table
        /// and therefore was merely constructed from the user input.
        /// </summary>
        public bool IsValid { get; protected set; }

        /// <summary>
        /// A flag indicating if the header is currently active.
        ///  Typically, only the active headers can be selected by the user,
        ///  but the code can still look up and display an inactive header.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Default format to use when converting the header to a string. By default, it displays the header text.
        /// </summary>
        [DataMember]
        public string DefaultFormat { get; set; } = FieldText;

        /// <summary>
        /// Internal dictionary for named attributes, but made public for Json serialization.
        /// </summary>
        [DataMember]
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Returns a value of the given named attribute.
        /// </summary>
        /// <param name="attribute">Attribute name.</param>
        /// <returns>The value of the attribute.</returns>
        public object this[string attribute]
        {
            get { return Attributes.ContainsKey(attribute) ? Attributes[attribute] : null; }
            set { Attributes[attribute] = value; }
        }

        /// <summary>
        /// Sets the attribute value if it has never been set. Otherwise adds a value
        /// to the list of values of the given attribute unless it already has such a value.
        /// If the current attribute value is not a list, it creates a list and adds it to the list first.
        /// </summary>
        /// <param name="attribute">Attribute name.</param>
        /// <param name="value">The value to add to the attribute.</param>
        public void AddToAttribute(string attribute, object value)
        {
            object curVal = this[attribute];
            if (curVal == null && value != null)
            {
                this[attribute] = value;
                return;
            }
            if (value == null || value.Equals(curVal)) return;
            if (!(curVal is IList lst))
            {
                lst = new List<object>();
                if (curVal != null) lst.Add(curVal);
                this[attribute] = lst;
            }
            if (!lst.Contains(value)) lst.Add(value);
        }

        /// <summary>
        /// Redefines the equals operator to compare headers by values and not references.
        /// </summary>
        public static bool operator ==(Header h1, Header h2) { return Equals(h1, h2); }

        /// <summary>
        /// Redefines the 'not equals' operator to compare headers by values and not references.
        /// </summary>
        public static bool operator !=(Header h1, Header h2) { return !Equals(h1, h2); }

        /// <summary>
        /// Compares this header with another header for equality by values.
        /// Two headers are considered equal if they have the same type and the same ID.
        /// </summary>
        /// <param name="obj">An object to compare this header to.</param>
        /// <returns>True of the other object is also a header with the same type and ID, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            Header hdr = obj as Header;
            if (hdr != null)
                return hdr.Type == Type && hdr.Id == Id;
            return base.Equals(obj);
        }

        /// <summary>
        /// Overrides the hash code function to be based on the combination of the header's type and ID.
        /// </summary>
        /// <returns>The header's hash code.</returns>
        public override int GetHashCode()
        {
            return (Type + "|" + Id).GetHashCode();
        }

        /// <summary>
        /// Gets the header's localized text using the specified resource manager and any language attribtes.
        /// </summary>
        /// <param name="resMgr">Resource manager to use for static resource localization.</param>
        /// <returns>Localized text for the header.</returns>
        public virtual string GetText(ResourceManager resMgr)
        {
            var attr = this[$"lang-{CultureInfo.CurrentUICulture.Name}"] ??
                this[$"lang-{CultureInfo.CurrentUICulture.Parent.Name}"];
            if (attr != null) return Convert.ToString(attr);

            string resKey = $"Enum_{Type}.{Id}";
            return resMgr?.GetString(resKey) ?? Text;
        }

        /// <summary>
        /// Returns a string representation of the header based on the default format.
        /// </summary>
        /// <returns>A string representation of the header based on the default format.</returns>
        public override string ToString()
        {
            return ToString(DefaultFormat, null);
        }

        /// <summary>
        /// Returns a string representation of the header based on the specified format.
        /// The format string can use the constants <c>FieldId</c>, <c>FieldText</c> and <c>AttrPattern</c>
        /// to refer to the value of ID, Text or any named attribute of the header respectively.
        /// </summary>
        /// <param name="format">The format string to use.</param>
        /// <param name="resMgr">Resource manager for text lookup.</param>
        /// <returns>A string representation of the header formatted according to the given format.</returns>
        public string ToString(string format, ResourceManager resMgr)
        {
            // for performance purposes check standard fields first
            if (format == FieldId || !IsValid) return Id?.Trim();
            if (format == FieldText) return GetText(resMgr)?.Trim();

            return Regex.Replace(format, @"\[(i|t|a:)(.*?)\]", m => EvaluateMatch(m, resMgr));
        }

        /// <summary>
        /// Evaluates a regular expression match for the string formatting.
        /// Substitutes the constants <c>FieldId</c>, <c>FieldText</c> and <c>AttrPattern</c>
        /// with the corresponding value of ID, Text or the named attribute of the header respectively.
        /// </summary>
        /// <param name="m">The match to evaluate.</param>
        /// <param name="resMgr">Resource manager for text lookup.</param>
        /// <returns>The result of the evaluation.</returns>
        private string EvaluateMatch(Match m, ResourceManager resMgr)
        {
            string field = m.Result("$1");
            string attrName = m.Result("$2");
            if (string.IsNullOrEmpty(attrName))
            {
                if (field == "i") return Id?.Trim();
                if (field == "t") return GetText(resMgr)?.Trim();
            }
            else if (field == "a:")
            {
                string res;
                object attr = this[attrName];
                if (attr is IList lst)
                    res = string.Join(", ", lst);
                else res = Convert.ToString(attr);
                return res.Trim();
            }
            return m.Value;
        }

        /// <summary>
        /// Compares this header to another object for sorting.
        /// By default, the headers are sorted by their Text field.
        /// </summary>
        /// <param name="obj">Another object to compare this header to for sorting.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects
        ///     being compared. The return value has these meanings: Value Meaning Less than
        ///     zero This instance is less than obj. Zero This instance is equal to obj.
        ///     Greater than zero This instance is greater than obj.</returns>
        public int CompareTo(object obj)
        {
            Header h = obj as Header;
            if (h != null) return string.Compare(Text, h.Text);
            return 0;
        }

        #region Dynamic object

        /// <inheritdoc/>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var members = Attributes.Keys.ToList();
            members.Add("Id");
            members.Add("Text");
            return members;
        }

        /// <inheritdoc/>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == "Id")
            {
                result = Id;
                return true;
            }
            if (binder.Name == "Text")
            {
                result = Text;
                return true;
            }
            if (Attributes.ContainsKey(binder.Name))
            {
                result = this[binder.Name];
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        /// <inheritdoc/>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder.Name == "Id")
            {
                Id = value?.ToString();
                return true;
            }
            if (binder.Name == "Text")
            {
                Text = value?.ToString();
                return true;
            }
            if (Attributes.ContainsKey(binder.Name))
            {
                this[binder.Name] = value;
                return true;
            }
            return base.TrySetMember(binder, value);
        }

        #endregion
    }
}
