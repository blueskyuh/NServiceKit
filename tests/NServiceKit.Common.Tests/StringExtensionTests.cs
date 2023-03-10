using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using NUnit.Framework;
using NServiceKit.Text;

namespace NServiceKit.Common.Tests
{
    /// <summary>A string extension tests.</summary>
	[TestFixture]
	public class StringExtensionTests
	{
        /// <summary>Converts this object to the works with value types.</summary>
		[Test]
		public void To_works_with_ValueTypes()
		{
			Assert.That(1.ToString().To<int>(), Is.EqualTo(1));
		}

        /// <summary>Converts this object to an on null or empty string returns default value supplied.</summary>
		[Test]
		public void To_on_null_or_empty_string_returns_default_value_supplied()
		{
			const string nullString = null;
			Assert.That("".To(1), Is.EqualTo(1));
			Assert.That("".To(default(int)), Is.EqualTo(default(int)));
			Assert.That(nullString.To(1), Is.EqualTo(1));
		}

        /// <summary>Converts this object to a value type on null or empty string returns default value.</summary>
		[Test]
		public void To_ValueType_on_null_or_empty_string_returns_default_value()
		{
			Assert.That("".To<int>(), Is.EqualTo(default(int)));
		}

        /// <summary>Converts this object to an URL encode.</summary>
		[Test]
		public void To_UrlEncode()
		{
			const string url = "http://www.NServiceKit.net/a?b=c&d=f";
			var urlEncoded = url.UrlEncode();

			Assert.That(urlEncoded, Is.EqualTo(HttpUtility.UrlEncode(url)));
		}

        /// <summary>Converts this object to an URL decode.</summary>
		[Test]
		public void To_UrlDecode()
		{
			const string url = "http://www.NServiceKit.net/a?b=c&d=f";
			var urlEncoded = url.UrlEncode();
			var decodedUrl = urlEncoded.UrlDecode();

			Assert.That(decodedUrl, Is.EqualTo(url));
		}

        /// <summary>URL format encodes components.</summary>
		[Test]
		public void UrlFormat_encodes_components()
		{
			const string url = "http://www.NServiceKit.net/a?b={0}&d={1}";
			const string arg1 = "as@if.com";
			const string arg2 = "&=";

			var urlFormat = url.UrlFormat(arg1, arg2);
			var expectedUrlFormat = string.Format(url, arg1.UrlEncode(), arg2.UrlEncode());

			Assert.That(urlFormat, Is.EqualTo(expectedUrlFormat));
		}

        /// <summary>Error code to english format.</summary>
		[Test]
		public void ErrorCode_to_English_format()
		{
			const string code = "EmailAddressIsInvalid";
			Assert.That(code.ToEnglish(), Is.EqualTo("Email address is invalid"));
		}

        /// <summary>Print special characters.</summary>
		[Test]
		public void Print_special_chars()
		{
			var specialChars = new List<char> { '"', ':', ',', '%' };
			specialChars.ForEach(x => Console.WriteLine(x + " = " + ((int)x).ToString("x")));
		}

        /// <summary>Hexadecimal escape escapes special characters.</summary>
		[Test]
		public void HexEscape_escapes_special_chars()
		{
			var specialChars = new List<char> { '"', ':', ',', '%' };
			const string unescapedString = "\"1st 2:nd 3r,d 4th%";
			const string expectedString = "%221st 2%3and 3r%2cd 4th%25";
			Assert.That(unescapedString.HexEscape(specialChars.ToArray()), Is.EqualTo(expectedString));
		}

        /// <summary>Hexadecimal unescape unescapes special characters.</summary>
		[Test]
		public void HexUnescape_unescapes_special_chars()
		{
			var specialChars = new List<char> { '"', ':', ',', '%' };
			const string escapedString = "%221st 2%3and 3r%2cd 4th%25";
			const string expectedString = "\"1st 2:nd 3r,d 4th%";
			Assert.That(escapedString.HexUnescape(specialChars.ToArray()), Is.EqualTo(expectedString));
		}

        /// <summary>Safe variable name strips illegal characters.</summary>
		[Test]
		public void SafeVarName_strips_illegal_chars()
		{
			Assert.That("with space".SafeVarName(), Is.EqualTo("with_space"));
			Assert.That("with @+:\\illegals".SafeVarName(), Is.EqualTo("with_____illegals"));
			Assert.That("UPPER_lower_0123456789".SafeVarName(), Is.EqualTo("UPPER_lower_0123456789"));
		}

        /// <summary>Glob finds right strings.</summary>
        [Test]
        public void Glob_finds_right_strings()
        {
            var input = new[] { "Foo", "Boo", "Hoo", "Baz" }.ToList();
            var expected = input.Where(s => s.EndsWith("oo")).ToList();

            var actual = input.Where(s => s.Glob("*oo")).ToList();
            
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        /// <summary>Does combine paths.</summary>
	    [Test]
	    public void Does_combine_paths()
	    {
            Assert.That("/".CombineWith("/some/other/path"), Is.EqualTo("/some/other/path"));
            Assert.That("a".CombineWith("/some/other/path"), Is.EqualTo("a/some/other/path"));
            Assert.That("a/".CombineWith("/some/other/path"), Is.EqualTo("a/some/other/path"));
            Assert.That("/a".CombineWith("/some/other/path"), Is.EqualTo("/a/some/other/path"));
            Assert.That("/a/".CombineWith("/some/other/path"), Is.EqualTo("/a/some/other/path"));
            Assert.That("/a".CombineWith("some", "other", "path"), Is.EqualTo("/a/some/other/path"));

            Assert.That("/a".CombineWith("/some/other/path/"), Is.EqualTo("/a/some/other/path/"));
            Assert.That("/a".CombineWith("/some/", "other", "/path/"), Is.EqualTo("/a/some/other/path/"));
            Assert.That("/a".CombineWith("some", "other", "path/"), Is.EqualTo("/a/some/other/path/"));
            
            Assert.That("".CombineWith("some", "other", "path/"), Is.EqualTo("some/other/path/"));
        }

        /// <summary>Converts this object to the HTTPS is idempotent.</summary>
        [Test]
        public void ToHttps_is_idempotent()
        {
            Assert.That("https://host.example.com/path".ToHttps().ToHttps(), Is.EqualTo("https://host.example.com/path"));
        }

        /// <summary>Converts this object to the HTTPS replaces HTTP with HTTPS.</summary>
        [Test]
        public void ToHttps_replaces_http_with_https()
        {
            Assert.That("http://host.example.com/path".ToHttps(), Is.EqualTo("https://host.example.com/path"));
        }

        /// <summary>Converts this object to the HTTPS only replaces HTTP at beginning of string.</summary>
        [Test]
        public void ToHttps_only_replaces_http_at_beginning_of_string()
        {
            Assert.That("http://host.example.com/http/path".ToHttps(), Is.EqualTo("https://host.example.com/http/path"));
        }

        /// <summary>Converts this object to the HTTPS ignores whitespace at beginning of string.</summary>
        [Test]
        public void ToHttps_ignores_whitespace_at_beginning_of_string()
        {
            Assert.That("  http://host.example.com".ToHttps(), Is.EqualTo("https://host.example.com"));
        }

        /// <summary>Converts this object to the HTTPS is not case sensitive.</summary>
        [Test]
        public void ToHttps_is_not_case_sensitive()
        {
            Assert.That("HTTP://HOST.EXAMPLE.COM".ToHttps(), Is.EqualTo("https://HOST.EXAMPLE.COM"));
        }

        /// <summary>HTML strip unescapes all HTML character codes correctly.</summary>
        [Test]
        public void HtmlStrip_unescapes_all_html_character_codes_correctly()
        {
            foreach (var value in Enumerable.Range(ushort.MinValue, ushort.MaxValue).Select(i => (ushort)i))
            {
                var expected = ((char)value).ToString(CultureInfo.InvariantCulture);

                var decimalNotation = String.Format("&#{0};", value);
                var decimalActual = decimalNotation.StripHtml().ConvertHtmlCodes();
                Assert.AreEqual(expected, decimalActual);

                var hexNotation = String.Format("&#x{0:X};", value);
                var hexActual = hexNotation.StripHtml().ConvertHtmlCodes();
                Assert.AreEqual(expected, hexActual);
            }

            foreach (var htmlNotation in StringUtils.HtmlCharacterCodes)
            {
                var actual = htmlNotation.Key.StripHtml().ConvertHtmlCodes();
                var expected = htmlNotation.Value;
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>HTML strip fixes actual production example.</summary>
        [Test]
        public void HtmlStrip_fixes_actual_production_example()
        {
            var encoded = @"Du tr&auml;umst von node.js und willst mithelfen einen Request in unter 50ms auszuliefern? PHP ist Deine Muttersprache und Dein Verstand schreit nach immer neuen Herausforderungen? Dann passt du zu uns. Bringe Deine Kompetenzen in ein Unternehmen ein, das Dir neben hervorragenden Arbeitsbedingungen wirklich etwas zu bieten hat: Perspektiven!
Werde Teil unseres Teams und gestalte aktiv die technische Zukunft der weltweit gr&ouml;&szlig;ten Online Hotelsuche mit. Arbeite mit neuesten Technologien in einem global aufgestellten Unternehmen. Nutze die Freiheit Bestehendes in Frage zu stellen, Deinen Horizont zu erweitern und Neues zu entwickeln.";
            var expected = @"Du träumst von node.js und willst mithelfen einen Request in unter 50ms auszuliefern? PHP ist Deine Muttersprache und Dein Verstand schreit nach immer neuen Herausforderungen? Dann passt du zu uns. Bringe Deine Kompetenzen in ein Unternehmen ein, das Dir neben hervorragenden Arbeitsbedingungen wirklich etwas zu bieten hat: Perspektiven!
Werde Teil unseres Teams und gestalte aktiv die technische Zukunft der weltweit größten Online Hotelsuche mit. Arbeite mit neuesten Technologien in einem global aufgestellten Unternehmen. Nutze die Freiheit Bestehendes in Frage zu stellen, Deinen Horizont zu erweitern und Neues zu entwickeln.";
            var actual = encoded.StripHtml().ConvertHtmlCodes();
            Assert.AreEqual(expected, actual);
        }
	}
}
