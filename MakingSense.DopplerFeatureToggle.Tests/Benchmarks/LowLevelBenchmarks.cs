﻿#region License
// Copyright (c) 2017 Doppler Relay Team
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if HAVE_BENCHMARKS

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace MakingSense.DopplerFeatureToggle.Tests.Benchmarks
{
    public class LowLevelBenchmarks
    {
        private const string FloatText = "123.123";
        private static readonly char[] FloatChars = FloatText.ToCharArray();

        private static readonly Dictionary<string, object> NormalDictionary = new Dictionary<string, object>();

        private static readonly ConcurrentDictionary<string, object> ConcurrentDictionary = new ConcurrentDictionary<string, object>();

        static LowLevelBenchmarks()
        {
            for (int i = 0; i < 10; i++)
            {
                string key = i.ToString();
                object value = new object();

                NormalDictionary.Add(key, value);
                ConcurrentDictionary.TryAdd(key, value);
            }
        }

        [Benchmark]
        public void DictionaryGet()
        {
            NormalDictionary.TryGetValue("1", out object _);
        }

        [Benchmark]
        public void ConcurrentDictionaryGet()
        {
            ConcurrentDictionary.TryGetValue("1", out object _);
        }

        [Benchmark]
        public void ConcurrentDictionaryGetOrCreate()
        {
            ConcurrentDictionary.GetOrAdd("1", Dummy);
        }

        private object Dummy(string arg)
        {
            throw new Exception("Should never get here.");
        }

        [Benchmark]
        public void DecimalTryParseString()
        {
            decimal value;
            decimal.TryParse(FloatText, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value);
        }
    }
}

#endif