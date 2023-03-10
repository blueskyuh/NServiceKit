using System.Collections.Specialized;
using NUnit.Framework;
using Moq;

namespace NServiceKit.ServiceHost.Tests
{
    [TestFixture]
    class HttpRequestAuthenticatonTests
    {
        /// <summary>Correct commas in digest authentication parsing.</summary>
        [Test]
        public void Correct_commas_in_digestAuth_parsing()
        {
            var requestMock = new Mock<IHttpRequest>();
            const string authHeader = "Digest username=\"кенкен\", realm=\"SWP\", nonce=\"NjM1MDk1NjA0NjExMjMuMTozOGVkMDcyYWQ1ODY5NzhhYTIxODAwNzkyYzRiNzZmYw==\", uri=\"/api/v1/projects/2969/tests?select=metadata,results\", response=\"5f818c8d263e26e787d75b60b78157d1\", qop=auth, nc=00000001, cnonce=\"7e06df0b911151b2\", ";
            var headers = new NameValueCollection();
            headers.Add("Authorization", authHeader);

            requestMock.Expect(e => e.Headers).Returns(headers);

            var res = HttpRequestAuthentication.GetDigestAuth(requestMock.Object);

            Assert.NotNull(res);
        }
    }
}
