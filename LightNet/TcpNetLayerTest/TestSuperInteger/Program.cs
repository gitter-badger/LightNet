/*
   Copyright 2015 Tyler Crandall

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using SuperInteger;
namespace TestSuperInteger
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var Sample = new Integer ();
			Sample += new Integer(555);
			Sample = Integer.Pow (Sample, 2);
			var output = Sample.ToBytes ();
			var Test = new Integer (output);
			Console.WriteLine (Sample.ToString ());
			Console.WriteLine ();
			Console.WriteLine (Test.ToString ());
			if (Sample.IsEqual(Test))
				Console.WriteLine ("Success");
			else
				Console.WriteLine ("Failed");
			Console.ReadLine ();
		}
	}
}
