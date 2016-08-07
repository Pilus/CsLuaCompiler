﻿namespace CsLuaTest.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class CollectionsTests : BaseTest
    {
        public CollectionsTests()
        {
            this.Name = "Collections";
            this.Tests["TestListInterfaces"] = TestListInterfaces;
            this.Tests["TestListImplementation"] = TestListImplementation;
            this.Tests["TestDictionaryInterfaces"] = TestDictionaryInterfaces;
            this.Tests["TestCountAndAny"] = TestCountAndAny;
            this.Tests["TestSelect"] = TestSelect;
            this.Tests["TestUnion"] = TestUnion;
            this.Tests["TestOrderBy"] = TestOrderBy;
        }

        private static void TestListInterfaces()
        {
            var list = new List<int>();
            Assert(true, list is IList);
            Assert(true, list is IList<int>);
            Assert(true, list is ICollection);
            Assert(true, list is ICollection<int>);
            Assert(true, list is IEnumerable);
            Assert(true, list is IEnumerable<int>);
            Assert(true, list is IReadOnlyList<int>);
            Assert(true, list is IReadOnlyCollection<int>);
        }

        private static void TestListImplementation()
        {
            var list = new List<int>();
            var iList = list as IList;

            Assert(0, list.Capacity);
            Assert(0, list.Count);

            list.Add(43);

            Assert(4, list.Capacity);
            Assert(1, list.Count);

            Assert(false, iList.IsFixedSize);
            Assert(false, iList.IsReadOnly);
            Assert(false, iList.IsSynchronized);
            Assert(false, iList.SyncRoot == null);

            list.Add(5);
            Assert(2, iList.Add(50));
            list.Add(75);
            Assert(4, list.Count);

            // Test Index
            Assert(43, list[0]);
            Assert(5, list[1]);

            list[1] = 6;
            Assert(6, list[1]);

            try
            {
                var x = list[-1];
                throw new Exception("Expected IndexOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index",
                    ex.Message);
            }

            try
            {
                list[4] = 10;
                throw new Exception("Expected IndexOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index",
                    ex.Message);
            }

            var verificationList = new List<int>();
            foreach (var item in list)
            {
                verificationList.Add(item);
            }

            Assert(list.Count, verificationList.Count);
            Assert(list[0], verificationList[0]);
            Assert(list[1], verificationList[1]);

            var list2 = new List<int>(new [] {7, 9, 13});
            Assert(3, list2.Count);
            Assert(7, list2[0]);

            list2.AddRange(new[] {21, 28});
            Assert(5, list2.Count);
            Assert(21, list2[3]);
            Assert(28, list2[4]);

            list2.Clear();

            Assert(0, list2.Count);

            Assert(true, list.Contains(6));

            list.Add(6);

            Assert(6, list.Find(i => i == 6));
            Assert(1, list.FindIndex(i => i == 6));
            Assert(6, list.FindLast(i => i == 6));
            Assert(4, list.FindLastIndex(i => i == 6));

            var all = list.FindAll(i => i == 6);
            Assert(2, all.Count);
            Assert(6, all[0]);
            Assert(6, all[1]);

            Assert(1, list.IndexOf(6));
            Assert(-1, list.IndexOf(500));
            Assert(4, list.LastIndexOf(6));

            list.Insert(1, 24);
            Assert(6, list.Count);
            Assert(24, list[1]);

            var res = list.GetRange(1, 2);
            Assert(2, res.Count);
            Assert(24, res[0]);
            Assert(6, res[1]);

            list.InsertRange(1, new [] {110, 120});
            Assert(8, list.Count);
            Assert(110, list[1]);
            Assert(120, list[2]);

            list.RemoveRange(2, 2);
            Assert(6, list.Count);
            Assert(110, list[1]);
            Assert(6, list[2]);
            Assert(50, list[3]);

            Assert(true, list.Remove(50));
            Assert(false, list.Remove(50));
        }

        private static void TestDictionaryInterfaces()
        {
            var list = new Dictionary<int, string>();
            Assert(true, list is IDictionary);
            Assert(true, list is IDictionary<int, string>);
            Assert(true, list is ICollection);
            Assert(true, list is ICollection<KeyValuePair<int, string>>);
            Assert(true, list is IEnumerable);
            Assert(true, list is IEnumerable<KeyValuePair<int, string>>);
            Assert(true, list is IReadOnlyDictionary<int, string>);
            Assert(true, list is IReadOnlyCollection<KeyValuePair<int, string>>);
        }

        private static void TestCountAndAny()
        {
            var a = new int[] {2, 4, 8, 16, 32, 64};
            Assert(true, a.Any());
            Assert(6, a.Count());

            var list = new List<string>();
            list.Add("a");
            list.Add("b");

            Assert(true, list.Any());
            Assert(2, list.Count());

            var enumerable = a.Where(e => e > 10 && e < 50);
            Assert(2, enumerable.Count());
            Assert(2, enumerable.Count()); // Test of multiple enumerations of enumerable

            var enumerable2 = list.Where(e => e.Length == 1);
            Assert(2, enumerable2.Count());
            list.Add("c");
            Assert(3, enumerable2.Count());
        }

        private static void TestSelect()
        {
            var a = new int[] { 2, 4, 8, 16, 32, 64 };

            var l1 = a.Select(v => v.ToString()).ToList();
            Assert(true, l1 is List<string>);

            var l2 = a.Select(ToFloat).ToList();
            Assert(true, l2 is List<float>);
        }

        private static void TestUnion()
        {
            var a = new int[] {1, 3, 5, 7};
            var b = new int[] {3, 9, 11, 7};

            var result = a.Union(b).ToArray();
            Assert(6, result.Length);
            Assert(1, result[0]);
            Assert(3, result[1]);
            Assert(5, result[2]);
            Assert(7, result[3]);
            Assert(9, result[4]);
            Assert(11, result[5]);
        }

        private static void TestOrderBy()
        {
            var input = new ClassWithProperties[]
            {
                new ClassWithProperties() { Number = 13 },
                new ClassWithProperties() { Number = 7 },
                new ClassWithProperties() { Number = 9 },
                new ClassWithProperties() { Number = 5 },
            };

            var ordered = input.OrderBy(v => v.Number).ToArray();

            Assert(5, ordered[0].Number);
            Assert(7, ordered[1].Number);
            Assert(9, ordered[2].Number);
            Assert(13, ordered[3].Number);
        }



        private static float ToFloat(int value)
        {
            return value;
        }
    }
}