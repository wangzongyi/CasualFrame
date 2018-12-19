using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MiniJSON;

public class StringParser {

	private static Regex sBoolRegex = new Regex("^\\d$");
	private static Regex sIntRegex = new Regex("^[+-]?\\d+$");
	private static Regex sColorRegex = new Regex("^\\d+(,\\d+)*$");
	private static Regex sVectorRegex = new Regex("^-?\\d*\\.?\\d+(,-?\\d*\\.?\\d+)*$");
	private static Regex sVector4Regex = new Regex("^-?\\d*\\.?\\d+(,-?\\d*\\.?\\d+){3,}$");

	public static string Format(string format, params object[] args)
	{
		int argLength = args.Length;
		object[] argsTemp = new object[argLength];
		for (int i = 0; i < argLength; ++i)
		{
			argsTemp[i] = args[i] ?? "";
		}
		return string.Format(format ?? "", argsTemp);
	}

	public static T ListItem2Type<T>(IList<string> list, int index)
	{
		return ListItem2Type(list, index, default(T));
	}

	public static T ListItem2Type<T>(IList<string> list, int index, T defaultValue)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Type<T>(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static T String2Type<T>(string str)
	{
		return String2Type(str, default(T));
	}

	public static T String2Type<T>(string str, T defaultValue)
	{
		Type type = typeof(T);
		if (str is T)
		{
			return (T) (object) str;
		}
		else if (type == typeof(int))
		{
			return (T) (object) String2Int(str);
		}
		else if (type == typeof(long))
		{
			return (T) (object) String2Long(str);
		}
		else if (type == typeof(short))
		{
			return (T) (object) String2Short(str);
		}
		else if (type == typeof(float))
		{
			return (T) (object) String2Float(str);
		}
		else if (type == typeof(double))
		{
			return (T) (object) String2Double(str);
		}
		else if (type == typeof(bool))
		{
			return (T) (object) String2Bool(str);
		}
		else if (type == typeof(Vector2))
		{
			return (T) (object) String2Vector2(str);
		}
		else if (type == typeof(Vector3))
		{
			return (T) (object) String2Vector3(str);
		}
		else if (type == typeof(Vector4))
		{
			return (T) (object) String2Vector4(str);
		}
		else if (type == typeof(Quaternion))
		{
			return (T) (object) String2Quaternion(str);
		}
		else if (type == typeof(List<object>))
		{
			return (T) (object) String2List(str);
		}
		else if (type == typeof(Dictionary<string, object>))
		{
			return (T) (object) String2Dict(str);
		}
		else
		{
			return defaultValue;
		}
	}

	public static string ListItem2String(IList<string> list, int index, string defaultValue = null)
	{
		if (index >= 0 && index < list.Count)
		{
			return list[index];
		}
		return defaultValue;
	}

	public static byte ListItem2Byte(IList<string> list, int index, byte defaultValue = 0)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Byte(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static byte String2Byte(string str, byte defaultValue = 0)
	{
		try
		{
			if (string.IsNullOrEmpty(str))
			{
				Debug.LogError("Can not convert null or empty to byte.");
				return defaultValue;
			}
			if (sIntRegex.IsMatch(str))
			{
				return Convert.ToByte(str);
			}
			return Convert.ToByte(Convert.ToInt32(Convert.ToDouble(str)));
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("Can not convert {0} to int with {1}", str, e.GetType().Name));
		}
		return defaultValue;
	}

	public static int ListItem2Int(IList<string> list, int index, int defaultValue = 0)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Int(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static int String2Int(string str, int defaultValue = 0)
	{
		try
		{
			if (string.IsNullOrEmpty(str))
			{
				Debug.LogWarning("Can not convert null or empty to int.");
				return defaultValue;
			}
			if (sIntRegex.IsMatch(str))
			{
				return Convert.ToInt32(str);
			}
			return Convert.ToInt32(Convert.ToDouble(str));
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("Can not convert {0} to int with {1}", str, e.GetType().Name));
		}
		return defaultValue;
	}

	public static long ListItem2Long(IList<string> list, int index, long defaultValue = 0)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Long(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static long String2Long(string str, long defaultValue = 0)
	{
		try
		{
			if (string.IsNullOrEmpty(str))
			{
				Debug.LogWarning("Can not convert null or empty to long.");
				return defaultValue;
			}
			if (sIntRegex.IsMatch(str))
			{
				return Convert.ToInt64(str);
			}
			return Convert.ToInt64(Convert.ToDouble(str));
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("Can not convert {0} to long with {1}", str, e.GetType().Name));
		}
		return defaultValue;
	}

	public static short ListItem2Short(IList<string> list, int index, short defaultValue = 0)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Short(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static short String2Short(string str, short defaultValue = 0)
	{
		try
		{
			if (string.IsNullOrEmpty(str))
			{
				Debug.LogWarning("Can not convert null or empty to short.");
				return defaultValue;
			}
			if (sIntRegex.IsMatch(str))
			{
				return Convert.ToInt16(str);
			}
			return Convert.ToInt16(Convert.ToDouble(str));
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("Can not convert {0} to short with {1}", str, e.GetType().Name));
		}
		return defaultValue;
	}

	public static float ListItem2Float(IList<string> list, int index, float defaultValue = 0)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Float(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static float String2Float(string str, float defaultValue = 0)
	{
		try
		{
			if (string.IsNullOrEmpty(str))
			{
				Debug.LogWarning("Can not convert null or empty to float.");
				return defaultValue;
			}
			return Convert.ToSingle(str);
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("Can not convert {0} to float with {1}", str, e.GetType().Name));
		}
		return defaultValue;
	}

	public static double ListItem2Double(IList<string> list, int index, double defaultValue = 0)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Double(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static double String2Double(string str, double defaultValue = 0)
	{
		try
		{
			if (string.IsNullOrEmpty(str))
			{
				Debug.LogWarning("Can not convert null or empty to double.");
				return defaultValue;
			}
			return Convert.ToDouble(str);
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("Can not convert {0} to double with {1}", str, e.GetType().Name));
		}
		return defaultValue;
	}

	public static bool ListItem2Bool(IList<string> list, int index, bool defaultValue = false)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Bool(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static bool String2Bool(string str, bool defaultValue = false)
	{
		try
		{
			if (string.IsNullOrEmpty(str))
			{
				Debug.LogWarning("Can not convert null or empty to bool.");
				return defaultValue;
			}
			if (sBoolRegex.IsMatch(str))
			{
				return Convert.ToBoolean(Convert.ToDouble(str));
			}
			return Convert.ToBoolean(str);
		}
		catch (Exception e)
		{
			Debug.LogError(string.Format("Can not convert {0} to bool with {1}", str, e.GetType().Name));
		}
		return defaultValue;
	}

	public static Color32 String2Color32(string str)
	{
		Color32 defualtValue = Color.white;
		return String2Color32(str, defualtValue);
	}

	public static Color32 String2Color32(string str, Color32 defualtValue)
	{
		if (sColorRegex.IsMatch(str ?? ""))
		{
			string[] strComps = str.Split(',');
			byte r = ListItem2Byte(strComps, 0);
			byte g = ListItem2Byte(strComps, 1);
			byte b = ListItem2Byte(strComps, 2);
			byte a = ListItem2Byte(strComps, 3, 255);
			return new Color32(r, g, b, a);
		}
		return defualtValue;
	}

	public static Vector2 ListItem2Vector2(IList<string> list, int index)
	{
		Vector2 defualtValue = Vector2.zero;
		return ListItem2Vector2(list, index, defualtValue);
	}

	public static Vector2 ListItem2Vector2(IList<string> list, int index, Vector2 defaultValue)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Vector2(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static Vector2 String2Vector2(string str)
	{
		Vector2 defualtValue = Vector2.zero;
		return String2Vector2(str, defualtValue);
	}

	public static Vector2 String2Vector2(string str, Vector2 defualtValue)
	{
		if (sVectorRegex.IsMatch(str ?? ""))
		{
			string[] strComps = str.Split(',');
			float x = ListItem2Float(strComps, 0);
			float y = ListItem2Float(strComps, 1);
			return new Vector2(x, y);
		}
		return defualtValue;
	}

	public static Vector3 ListItem2Vector3(IList<string> list, int index)
	{
		Vector3 defualtValue = Vector3.zero;
		return ListItem2Vector3(list, index, defualtValue);
	}

	public static Vector3 ListItem2Vector3(IList<string> list, int index, Vector3 defaultValue)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Vector3(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static Vector3 String2Vector3(string str)
	{
		Vector3 defualtValue = Vector3.zero;
		return String2Vector3(str, defualtValue);
	}

	public static Vector3 String2Vector3(string str, Vector3 defualtValue)
	{
		if (sVectorRegex.IsMatch(str ?? ""))
		{
			string[] strComps = str.Split(',');
			float x = ListItem2Float(strComps, 0);
			float y = ListItem2Float(strComps, 1);
			float z = ListItem2Float(strComps, 2);
			return new Vector3(x, y, z);
		}
		return defualtValue;
	}

	public static Vector4 ListItem2Vector4(IList<string> list, int index)
	{
		Vector4 defualtValue = Vector4.zero;
		return ListItem2Vector4(list, index, defualtValue);
	}

	public static Vector4 ListItem2Vector4(IList<string> list, int index, Vector4 defaultValue)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Vector4(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static Vector3 String2Vector4(string str)
	{
		Vector4 defualtValue = Vector4.zero;
		return String2Vector4(str, defualtValue);
	}

	public static Vector4 String2Vector4(string str, Vector4 defualtValue)
	{
		if (sVectorRegex.IsMatch(str ?? ""))
		{
			string[] strComps = str.Split(',');
			float x = ListItem2Float(strComps, 0);
			float y = ListItem2Float(strComps, 1);
			float z = ListItem2Float(strComps, 2);
			float w = ListItem2Float(strComps, 3);
			return new Vector4(x, y, z, w);
		}
		return defualtValue;
	}

	public static Quaternion ListItem2Quaternion(IList<string> list, int index)
	{
		Quaternion defualtValue = Quaternion.identity;
		return ListItem2Quaternion(list, index, defualtValue);
	}

	public static Quaternion ListItem2Quaternion(IList<string> list, int index, Quaternion defaultValue)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Quaternion(list[index], defaultValue);
		}
		return defaultValue;
	}

	public static Quaternion String2Quaternion(string str)
	{
		Quaternion defualtValue = Quaternion.identity;
		return String2Quaternion(str, defualtValue);
	}

	public static Quaternion String2Quaternion(string str, Quaternion defualtValue)
	{
		if (sVectorRegex.IsMatch(str ?? ""))
		{
			string[] strComps = str.Split(',');
			float x = ListItem2Float(strComps, 0);
			float y = ListItem2Float(strComps, 1);
			float z = ListItem2Float(strComps, 2);
			float w = ListItem2Float(strComps, 3);
			return new Quaternion(x, y, z, w);
		}
		return defualtValue;
	}

	public static bool IsVector4(string trimedStr)
	{
		return sVector4Regex.IsMatch(trimedStr ?? "");
	}

	public static List<object> ListItem2List(IList<string> list, int index)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2List(list[index]);
		}
		return null;
	}

	public static List<object> String2List(string str)
	{
		return Json.Deserialize(str) as List<object>;
	}

	public static Dictionary<string, object> ListItem2Dic(IList<string> list, int index)
	{
		if (index >= 0 && index < list.Count)
		{
			return String2Dict(list[index]);
		}
		return null;
	}

	public static Dictionary<string, object> String2Dict(string str)
	{
		return Json.Deserialize(str) as Dictionary<string, object>;
	}
}
