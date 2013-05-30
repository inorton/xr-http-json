using System;
using System.Collections;
using System.Diagnostics;

namespace XR.Client.Json
{
	public class JsonServerErrorException : Exception
	{
		public IDictionary[] Errors { get; set; }

		public string FirstErrorMessage { get; set; }

		public string FirstErrorType { get; set; }

		StackTrace trace = null;

		public JsonServerErrorException ( string message, IDictionary[] errors ) : base( message )
		{
			Errors = errors;
			trace = new StackTrace( 9 , true );
		}

		public override string ToString ()
		{
			return string.Format ("JsonServerErrorException: {0} '{1}'"
			                      + Environment.NewLine 
			                      + "{2}", FirstErrorType, FirstErrorMessage, trace);
		}
	}
}

