using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Common.BVdf
{
    public static class BVdfFile
    {
        public static BVProperty ReadProperty(byte token, BinaryReader reader)
        {
            //byte token = reader.ReadByte();
            string Key = ReadString(reader);
            //Console.WriteLine("{0:X2} {1}", token, Key);
            BVToken Value;
            switch (token)
            {
                case 0x00:
                    Value = ReadPropertyArray(reader);
                    break;
                case 0x01:
                    Value = ReadString(reader);
                    //Value = BVToken.Make(ReadString(reader));
                    break;
                case 0x02:
                    Value = reader.ReadInt32();
                    //Value = BVToken.Make(reader.ReadInt32());
                    break;
                case 0x03:
                    Value = reader.ReadSingle();
                    //Value = BVToken.Make(reader.ReadSingle());
                    break;
                case 0x04:
                    throw new Exception("PTR");
                    //break;
                case 0x05:
                    throw new Exception("WSTRING");
                    //break;
                case 0x06:
                    throw new Exception("COLOR");
                    //break;
                case 0x07:
                    Value = reader.ReadUInt64();
                    //Value = BVToken.Make(reader.ReadUInt64());
                    break;
                default:
                    throw new Exception(string.Format("Unknown Type Byte {0:X2}", token));
            }
            return new BVProperty(Key, Value);
        }

        private static void WriteProperty(BinaryWriter writer, BVProperty data)
        {
            if (data.Value.GetType() == typeof(BVPropertyCollection))
            {
                writer.Write((byte)0x00);
                WriteString(writer, data.Key);
                WritePropertyArray(writer, (BVPropertyCollection)data.Value);
                writer.Write((byte)0x08);
            }
            else if (data.Value.GetType() == typeof(BVStringToken))
            {
                writer.Write((byte)0x01);
                WriteString(writer, data.Key);
                WriteString(writer, ((BVStringToken)data.Value).Value);
            }
            else if (data.Value.GetType() == typeof(BVInt32Token))
            {
                writer.Write((byte)0x02);
                WriteString(writer, data.Key);
                writer.Write(((BVInt32Token)data.Value).Value);
            }
            else if (data.Value.GetType() == typeof(BVSingleToken))
            {
                writer.Write((byte)0x03);
                WriteString(writer, data.Key);
                writer.Write(((BVSingleToken)data.Value).Value);
            }
            else if (data.Value.GetType() == typeof(BVUInt64Token))
            {
                writer.Write((byte)0x07);
                WriteString(writer, data.Key);
                writer.Write(((BVUInt64Token)data.Value).Value);
            }
            else
            {
                throw new Exception(string.Format("Unknown Type {0}", data.GetType().ToString()));
            }
        }

        public static BVPropertyCollection ReadPropertyArray(BinaryReader reader)
        {
            BVPropertyCollection Values = new BVPropertyCollection();
            byte ReadByte = 0x00;
            while ((ReadByte = reader.ReadByte()) != 0x08)
            {
                //Values.Add(ReadProperty(reader));
                Values.Add(ReadProperty(ReadByte, reader));
            }
            //reader.ReadByte();
            return Values;
        }

        public static void WritePropertyArray(BinaryWriter writer, BVPropertyCollection data)
        {
            data.Properties.ForEach(dr =>
            {
                WriteProperty(writer, dr);
            });
        }

        public static string ReadString(BinaryReader reader)
        {
            List<byte> values = new List<byte>();
            byte tmp;
            while ((tmp = reader.ReadByte()) != (byte)0x00)
            {
                values.Add(tmp);
            }

            return System.Text.Encoding.UTF8.GetString(values.ToArray());
        }

        public static void WriteString(BinaryWriter writer, string data)
        {
            writer.Write(System.Text.Encoding.UTF8.GetBytes(data));
            writer.Write((byte)0x00);
        }
    }
    public abstract class BVToken
    {
        static public implicit operator BVToken(Int32 value)
        {
            return new BVInt32Token(value);
        }
        static public implicit operator BVToken(Single value)
        {
            return new BVSingleToken(value);
        }
        static public implicit operator BVToken(UInt64 value)
        {
            return new BVUInt64Token(value);
        }
        static public implicit operator BVToken(string value)
        {
            return new BVStringToken(value);
        }

        public abstract object GetValue();

        public T GetValue<T>()
        {
            var val = GetValue();

            if (val is T)
            {
                return (T)val;
            }
            else
            {
                if (typeof(T) == typeof(bool)
                 && val.GetType() == typeof(string))
                {
                    string valueRawString = (string)Convert.ChangeType(val, typeof(string));
                    Int64 tmpOut = 0;
                    if (Int64.TryParse(valueRawString, out tmpOut))
                    {
                        if (tmpOut == 0 || tmpOut == 1)
                            return (T)Convert.ChangeType(tmpOut, typeof(T));
                    }
                    throw new Exception("This isn't a bool, use something else");
                }

                if (typeof(T) == typeof(bool) && val.GetType() == typeof(UInt64))
                {
                    UInt64 valueRaw = (UInt64)Convert.ChangeType(val, typeof(UInt64));
                    if (valueRaw == 0 || valueRaw == 1)
                        return (T)Convert.ChangeType(valueRaw, typeof(T));
                    throw new Exception("This isn't a bool, use something else");
                }

                if (typeof(T) == typeof(bool) && val.GetType() == typeof(Int32))
                {
                    Int32 valueRaw = (Int32)Convert.ChangeType(val, typeof(Int32));
                    if (valueRaw == 0 || valueRaw == 1)
                        return (T)Convert.ChangeType(valueRaw, typeof(T));
                    throw new Exception("This isn't a bool, use something else");
                }

                try
                {
                    return (T)Convert.ChangeType(val, typeof(T));
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }
        }

        /*static public BVToken Make(Int32 value)
        {
            return new BVInt32Token(value);
        }
        static public BVToken Make(Single value)
        {
            return new BVSingleToken(value);
        }
        static public BVToken Make(UInt64 value)
        {
            return new BVUInt64Token(value);
        }
        static public BVToken Make(string value)
        {
            return new BVStringToken(value);
        }*/
    }
    public class BVProperty
    {
        public string Key { get; set; }
        public BVToken Value { get; set; }

        public BVProperty(string key, BVToken value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
    public class BVInt32Token : BVToken
    {
        public Int32 Value { get; private set; }

        public BVInt32Token(Int32 value)
        {
            this.Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
    public class BVSingleToken : BVToken
    {
        public Single Value { get; private set; }

        public BVSingleToken(Single value)
        {
            this.Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
    public class BVUInt64Token : BVToken
    {
        public UInt64 Value { get; private set; }

        public BVUInt64Token(UInt64 value)
        {
            this.Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
    public class BVStringToken : BVToken
    {
        public string Value { get; private set; }

        public BVStringToken(string value)
        {
            this.Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
    public class BVPropertyCollection : BVToken
    {
        public List<BVProperty> Properties { get; private set; }

        public BVPropertyCollection()
        {
            Properties = new List<BVProperty>();
        }

        public void Add(BVProperty vProperty)
        {
            Properties.Add(vProperty);
            NumericMemo = null; // we don't know anymore
        }

        public void Add(string key, BVToken token)
        {
            Properties.Add(new BVProperty(key, token));
            NumericMemo = null; // we don't know anymore
        }

        public void Add(BVToken token)
        {
            if (!IsNumeric())
                throw new Exception("Collection is not array");

            Properties.Add(new BVProperty(Properties.Count > 0 ? (Properties.Max(dr => int.Parse(dr.Key)) + 1).ToString() : "0", token));
        }

        public int Remove(string key)
        {
            if (IsNumeric())
            {
                int countRemoved = Properties.RemoveAll(dr => dr.Key == key);
                int keyNumber;
                if (int.TryParse(key, out keyNumber))
                {
                    Properties.Where(dr => int.Parse(dr.Key) > keyNumber).ToList().ForEach(dr =>
                    {
                        dr.Key = (int.Parse(dr.Key) - 1).ToString();
                    });
                }
                NumericMemo = true; // we were numeric and we removed numerics and shifted
                return countRemoved;
            }
            else
            {
                NumericMemo = null; // we don't know anymore
                return Properties.RemoveAll(dr => dr.Key == key);
            }
        }

        public int Remove(BVToken value)
        {
            if (IsNumeric())
            {
                var qry = Properties.Where(dr => dr.Value == value);
                if (qry.Count() > 0)
                {
                    int keyNumber = int.Parse(qry.First().Key);
                    int countRemoved = Properties.RemoveAll(dr => dr.Value == value);
                    Properties.Where(dr => int.Parse(dr.Key) > keyNumber).ToList().ForEach(dr =>
                    {
                        dr.Key = (int.Parse(dr.Key) - 1).ToString();
                    });
                    NumericMemo = true; // we're still numeric
                    return countRemoved;
                }
                NumericMemo = true; // we're still numeric
                return 0;
            }
            else
            {
                NumericMemo = null; // we don't know, we might have removed the thing that made us not numeric
                return Properties.RemoveAll(dr => dr.Value == value);
            }
        }

        private bool _ForceNumericWhenUnsure;
        private bool _ForceObjectWhenUnsure;
        public bool ForceNumericWhenUnsure { get { return _ForceNumericWhenUnsure; } set { _ForceNumericWhenUnsure = value; if (value) _ForceObjectWhenUnsure = false; } }
        public bool ForceObjectWhenUnsure { get { return _ForceObjectWhenUnsure; } set { _ForceObjectWhenUnsure = value; if (value) _ForceNumericWhenUnsure = false; } }

        private bool? NumericMemo;
        public bool IsNumeric()
        {
            if (Properties.Count == 0)
            {
                if (_ForceNumericWhenUnsure)
                    return true;
                if (ForceObjectWhenUnsure)
                    return false;
                return false;//return true;
            }

            if (Properties.Count == 1 && Properties[0].Key == "0")
            {
                if (_ForceNumericWhenUnsure)
                    return true;
                if (ForceObjectWhenUnsure)
                    return false;
                return false;//return true;
            }

            if (NumericMemo.HasValue)
                return NumericMemo.Value;

            int tmp;
            if (!Properties.All(dr => int.TryParse(dr.Key, out tmp)))
                return false;

            var baseQry = Properties.Select(dr => dr.Key);
            if (!baseQry.SequenceEqual(baseQry.Distinct()))
                return false;

            int counter = 0;
            bool orderPreserved = true;
            baseQry.Select(dr => int.Parse(dr)).OrderBy(dr => dr).ToList().ForEach(dr =>
            {
                orderPreserved = orderPreserved && (dr == counter);
                counter++;
            });

            NumericMemo = orderPreserved;
            return orderPreserved;
        }

        public BVToken this[string key]
        {
            get
            {
                return Properties.Where(dr => dr.Key == key).FirstOrDefault()?.Value;
            }
            set
            {
                var qry = Properties.Where(dr => dr.Key == key);
                if (qry.Count() > 0)
                {
                    qry.First().Value = value;
                }
                else
                {
                    Properties.Add(new BVProperty(key, value));
                }
            }
        }

        public List<BVProperty> Children()
        {
            return Properties;
        }

        public override object GetValue()
        {
            return Properties;
        }
    }
}
