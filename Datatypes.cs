using System;
using System.Text.RegularExpressions;
using Token = System.Collections.Generic.List<string?>;
using TokenList = System.Collections.Generic.List<System.Collections.Generic.List<string?>>;
using Spectre.Console;
using OneOf;

using ASTNode = Compiler.ASTNode;

namespace QTypes {
    class Object(object obj) {

    }
    class String : Object {
        private string self = "";
        public int length = 0;

        public String(object obj) : base(obj) {
            self = obj.ToString();
            length = self.Length;
        }
        public char this[int index] {
            get {
                if (index < 0) {
                    index += length;
                }
                return self[index];
            } set {
                self = (sub(0, index) + value + sub(index+1, -1)).ToString();
            }
        }

        public String sub(int start = 0, int? end = null, int step = 1) {
            if (end == null) {
                end = length;
            } else if (end < 0) {
                end += length;
            }

            if (start < 0) {
                start += length;
            }

            String res = new("");
            for (int i = start; i < end; i += step) {
                res += self[i];
            }
            return res;
        }

        public static String operator+(String s1, String s2) {
            return new String(s1.self + s2.self);
        }
        public static String operator+(String s1, string s2) {
            return new String(s1.self + s2);
        }
        public static String operator+(String s, char c) {
            return s + new String(c);
        }
        public static String operator*(String s, int n) {
            String res = new("");
            for (int i = 0; i < n; i++) {
                res += s;
            }
            return res;
        }
        public static String operator-(String s, char c) {
            if (s[-1] == c) {
                return s.sub(0, -1);
            }
            return s;
        }
        public static String operator-(String s1, String s2) {
            if (s1 < s2) {
                return s1;
            } else if (s1.length == s2.length) {
                if (s1 == s2) return new String("");
                else return s1;
            } else {
                String subs1 = s1.sub(-s2.length, s1.length);
                if (subs1 == s2) {
                    return s1.sub(0, -s2.length);
                } else {
                    return s1;
                }
            }
        }

        public static bool operator<(String s1, String s2) {
            return s1.length < s2.length;
        }
        public static bool operator>(String s1, String s2) {
            return s1.length > s2.length;
        }
        public static bool operator<=(String s1, String s2) {
            return !(s1 > s2);
        }
        public static bool operator>=(String s1, String s2) {
            return !(s1 < s2);
        }
        public static bool operator==(String s1, String s2) {
            return s1.self == s2.self;
        }
        public static bool operator!=(String s1, String s2) {
            return !(s1 == s2);
        }

        public override bool Equals(object? o) {
            try {
                return self == o;
            } catch {
                return false;
            }
        }
        public override string ToString() {
            return self;
        }
    }
}