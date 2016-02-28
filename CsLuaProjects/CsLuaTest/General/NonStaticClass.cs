﻿namespace CsLuaTest.General
{
    public class NonStaticClass
    {
        public int Value;

        public int CallWithSameClass(NonStaticClass other)
        {
            return other.Value + this.Value;
        }

        public static void StaticMethod(int x)
        {
            GeneralTests.Output = "StaticMethodInt";
        }

        public static void StaticMethod(string x)
        {
            GeneralTests.Output = "StaticMethodString";
        }


    }
}