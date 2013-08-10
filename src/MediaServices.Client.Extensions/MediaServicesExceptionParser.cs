// <copyright file="MediaServicesExceptionParser.cs" company="open-source">
//  No rights reserved. Copyright (c) 2013 by Mariano Converti
//   
//  Redistribution and use in source and binary forms, with or without modification, are permitted.
//
//  The names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;

    public static class MediaServicesExceptionParser
    {
        private const string DataServicesMetadataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        private static readonly XName MessageXName = XName.Get("message", DataServicesMetadataNamespace);
        private static readonly XName CodeXName = XName.Get("code", DataServicesMetadataNamespace);

        public static Exception Parse(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            try
            {
                Exception baseException = exception.GetBaseException();
                XDocument doc = XDocument.Parse(baseException.Message);

                return ParseExceptionErrorXElement(doc.Root) ?? exception;
            }
            catch
            {
                return exception;
            }
        }

        private static Exception ParseExceptionErrorXElement(XElement errorElement)
        {
            string errorCode = errorElement.GetElementValueOrDefault(CodeXName);
            string errorMessage = errorElement.GetElementValueOrDefault(MessageXName);

            Exception exception = null;
            if (!string.IsNullOrWhiteSpace(errorCode) && !string.IsNullOrWhiteSpace(errorMessage))
            {
                exception = new Exception(
                    string.Format(CultureInfo.InvariantCulture, "{0}: {1}", errorCode, errorMessage));
            }
            else if (!string.IsNullOrWhiteSpace(errorCode))
            {
                exception = new Exception(errorCode);
            }
            else if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                exception = new Exception(errorMessage);
            }

            return exception;
        }

        private static string GetElementValueOrDefault(this XElement element, XName name)
        {
            string value = null;
            XElement childElement = element.Element(name);
            if (childElement != null)
            {
                value = childElement.Value.ToString();
            }

            return value;
        }
    }
}