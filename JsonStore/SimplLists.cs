using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace JsonStore
{
    public class SimplLists
    {
        private string[] _strings;
        private ushort[] _integers;
        private ushort[] _bools;

        public string[] Strings { get { return _strings; } }
        public ushort[] Integers { get { return _integers; } }
        public ushort[] Bools { get { return _bools; } }
        public ushort StringsLength { get { return (ushort)_strings.Length; } }
        public ushort IntegersLength { get { return (ushort)_integers.Length; } }
        public ushort BoolsLength { get { return (ushort)_bools.Length; } }

        public SimplLists()
        {
            _strings = new string[0];
            _integers = new ushort[0];
            _bools = new ushort[0];
        }

        public SimplLists(string[] strings, int[] integers, bool[] bools)
        {
            _strings = strings;
            _integers = integers.Select(x => (ushort)x).ToArray();
            _bools = bools.Select(x => x ? (ushort)1 : (ushort)0).ToArray();
        }

        public void Resize(ushort size)
        {
            _strings = new string[size];
            _integers = new ushort[size];
            _bools = new ushort[size];
        }
    }
}