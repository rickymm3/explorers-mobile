/** Title:  
  *----------------------------------------------------------
  * File:   ExtensionMethods.cs
  * Author: Brent Kilbasco
  *
  * Copyright (c) Egg Roll Digital 2015
  *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using SimpleJSON;
using Newtonsoft.Json;

using Points = System.Collections.Generic.List<UnityEngine.Vector2>;

namespace ExtensionMethods {
		
		
	/**~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	/// 
	///     Class ExtensionMethods
	///------------------------------------------
	///<summary>
	/// Collection of class extension methods. 
	///</summary>
	/**~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/ 
	public static class ExtensionMethods{
	
		/**~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	      *  ToCurrencyString
	      * -----------------------------------------   
	      * ~ Accepts an int value and returns a string
	      *     money value formatted with commas.
	      *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/ 
		public static string ToCurrencyString(this int pCurrencyValue){
			return pCurrencyValue.ToString("n0");
		}//END ToCurrencyString

		/**~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	      *  ToCurrencyString
	      * -----------------------------------------   
	      * ~ Accepts an float value and returns a string
	      *     money value formatted with commas.
	      *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/ 
		public static string ToCurrencyString(this float pCurrencyValue){

			string retString = pCurrencyValue.ToString();
			string[] retStringAsFloat = retString.Split('.');
			
			if( retStringAsFloat[0].Length > 3 )
				for( int i = retStringAsFloat[0].Length-4; i >= 0; i -= 3 )
					retStringAsFloat[0] = retStringAsFloat[0].Insert( i+1, "," );

			if(retStringAsFloat.Length > 1)
				retString = retStringAsFloat[0] + "." + retStringAsFloat[1];
			
			return retString;
			
		}//END ToCurrencyString


		/**~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	      *  ToTwoDecimals
	      * -----------------------------------------   
	      * ~ Accept
	      *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/ 
		public static float ToTwoDecimals(this float pValue){

			return (float)Math.Round( pValue, 2 );

		}//END ToTwoDecimals


		/// <summary>
		/// DOText function for TextMeshProUGUI components
		/// </summary>
		public static Tweener DOText(this TextMeshProUGUI tmproText, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null) {
			
			return DOTween.To(() => tmproText.text, x => tmproText.text = x, endValue, duration).SetOptions(richTextEnabled, scrambleMode, scrambleChars);
			
		} 
		
		
		/// <summary>
		/// Align the left side of the given RectTransform to the left side of this ScrollRect
		/// </summary>
		public static void LeftAlign(this ScrollRect scrollRect, RectTransform targetRectTransform) {
			scrollRect.HorizontallyAlign(targetRectTransform, 0f);
		}
		
		/// <summary>
		/// Align the right side of the given RectTransform to the right side of this ScrollRect
		/// </summary>
		public static void RightAlign(this ScrollRect scrollRect, RectTransform targetRectTransform) {
			scrollRect.HorizontallyAlign(targetRectTransform, 1f);
		}
		
		/// <summary>
		/// Horizontally align the center of the given RectTransform to the center side of this ScrollRect
		/// </summary>
		public static void HorizontallyCenter(this ScrollRect scrollRect, RectTransform targetRectTransform) {
			scrollRect.HorizontallyAlign(targetRectTransform, 0.5f);
		}
		
		/// <summary>
		/// Horizontally align the ScrollRect to a RectTransform without scrolling past the edge of the content
		/// <param name="alignment"> 0 = left aligned, 1 = right aligned</param>
		/// </summary>
		public static void HorizontallyAlign(this ScrollRect scrollRect, RectTransform targetRectTransform, float alignment) {
			
			scrollRect.horizontalNormalizedPosition = scrollRect.CalculateHorizontalNormalizedPosition(targetRectTransform, alignment);
		}
		
		/// <summary>
		/// Get the normalized position of horizontally aligning the ScrollRect to a RectTransform without scrolling past the edge of the content
		/// <param name="alignment"> 0 = left aligned, 1 = right aligned</param>
		/// </summary>
		public static float CalculateHorizontalNormalizedPosition(this ScrollRect scrollRect, RectTransform targetRectTransform, float alignment) {
			
			// Get world position of the point of the traget transform
			// Alignment of 0 is left side of transform, 1 is right side
			Vector2 targetCenterLocalPosition = targetRectTransform.rect.center;
			float targetCenterOffset = targetRectTransform.rect.width * (alignment - 0.5f);
			Vector3 targetWorldPosition = targetRectTransform.TransformPoint(targetCenterLocalPosition + Vector2.right * targetCenterOffset);
			
			// Convert world position to position relative to scrollRect
			float targetXPosition = scrollRect.content.InverseTransformPoint(targetWorldPosition).x;
			
			// Calculate the new normalized position for the scroll rect by accounting
			// for the extra space on the left and right of the content where it can't 
			// be aligned without scrolling past the edge
			float contentWidth = scrollRect.content.rect.width;
			float scrollRectWidth = ((RectTransform)scrollRect.transform).rect.width;
			float newNormalizedPosition = (targetXPosition - (scrollRectWidth * alignment)) / (contentWidth - scrollRectWidth);
			
			// Clamp value between 0 and 1 so it dosen't scroll past the edge of the content
			return Mathf.Clamp01(newNormalizedPosition);
		}
		
		
		/// <summary>
		/// Align the bottom side of the given RectTransform to the bottom side of this ScrollRect
		/// </summary>
		public static void BottomAlign(this ScrollRect scrollRect, RectTransform targetRectTransform) {
			scrollRect.VerticallyAlign(targetRectTransform, 0f);
		}
		
		/// <summary>
		/// Align the top side of the given RectTransform to the top side of this ScrollRect
		/// </summary>
		public static void TopAlign(this ScrollRect scrollRect, RectTransform targetRectTransform) {
			scrollRect.VerticallyAlign(targetRectTransform, 1f);
		}
		
		/// <summary>
		/// Vertically align the center of the given RectTransform to the center side of this ScrollRect
		/// </summary>
		public static void VerticallyCenter(this ScrollRect scrollRect, RectTransform targetRectTransform) {
			scrollRect.VerticallyAlign(targetRectTransform, 0.5f);
		}
		
		/// <summary>
		/// Vertically align the ScrollRect to a RectTransform without scrolling past the edge of the content
		/// <param name="alignment"> 0 = bottom aligned, 1 = top aligned</param>
		/// </summary>
		public static void VerticallyAlign(this ScrollRect scrollRect, RectTransform targetRectTransform, float alignment) {
			
			scrollRect.verticalNormalizedPosition = scrollRect.CalculateVerticalNormalizedPosition(targetRectTransform, alignment);
		}
		
		/// <summary>
		/// Get the normalized position of vertically aligning the ScrollRect to a RectTransform without scrolling past the edge of the content
		/// <param name="alignment"> 0 = bottom aligned, 1 = top aligned</param>
		/// </summary>
		public static float CalculateVerticalNormalizedPosition(this ScrollRect scrollRect, RectTransform targetRectTransform, float alignment) {
			
			// Get world position of the point of the traget transform
			// Alignment of 0 is bottom side of transform, 1 is top side
			Vector2 targetCenterLocalPosition = targetRectTransform.rect.center;
			float targetCenterOffset = targetRectTransform.rect.height * (alignment - 0.5f);
			Vector3 targetWorldPosition = targetRectTransform.TransformPoint(targetCenterLocalPosition + Vector2.up * targetCenterOffset);

			// Convert world position to position relative to scrollRect
			float targetYPosition = scrollRect.content.InverseTransformPoint(targetWorldPosition).y;
			
			// Calculate the new normalized position for the scroll rect by accounting
			// for the extra space on the top and bottom of the content where it can't 
			// be aligned without scrolling past the edge
			float contentHeight = scrollRect.content.rect.height;
			float scrollRectHeight = ((RectTransform)scrollRect.transform).rect.height;
			float newNormalizedPosition = (targetYPosition - (scrollRectHeight * alignment)) / (contentHeight - scrollRectHeight);
			
			// Clamp value between 0 and 1 so it dosen't scroll past the edge of the content
			return Mathf.Clamp01(newNormalizedPosition);
		}

		//////////////////////////////////////////////////////////// For Strings:

		public static string Format2(this string str, params object[] args) {
			return string.Format(str, args);
		}

        public static string JoinNewLines(this string baseStr, params string[] otherLines) {
            return baseStr + "\n" + otherLines.Join("\n");
        }

        public static string Join(this string[] strArr, string delim) {
            return string.Join(delim, strArr);
        }

        public static string[] Split(this string str, string delim) {
            return str.Split(new string[] { delim }, StringSplitOptions.None);
        }

        //public static string[] Split(this string str, char delim) {
        //	return str.Split(new char[] { delim }, StringSplitOptions.None);
        //}

        //public static string Join<T>(this T[] strArr, string delim) {
        //    return string.Join(delim, strArr);
        //}

        public static string Times(this string str, int repeatCount) {
			string output = "";
			while(repeatCount>0) {
				output += str;
				repeatCount--;
			}
			return output;
		}

		/* Converts a sentence like "titled case like this" to "Titled Case Like This".
		 * (see: http://stackoverflow.com/a/1206029/468206 for reference)
		 */
		public static string ToTitleCase(this string str) {
			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
			return textInfo.ToTitleCase(str);
		}

        // Source: http://csharphelper.com/blog/2014/10/convert-between-pascal-case-camel-case-and-proper-case-in-c/
        public static string ToCamelCase(this string str, string delim="_") {
            // If there are 0 or 1 characters, just return the string.
            if (str == null || str.Length < 2)
                return str;

            // Split the string into words.
            string[] words = str.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            
            // Combine the words.
            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++) {
                string w = words[i].ToLower();
                result += w.Substring(0, 1).ToUpper() + w.Substring(1);
            }

            return result;
        }

		public static List<T> Clone<T>(this List<T> list) {
			List<T> dup = new List<T>();
			dup.AddRange(list);
			return dup;
		}

		public static string ToBase64(this string str) {
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(str);
			return Convert.ToBase64String(plainTextBytes);
		}

		public static string FromBase64(this string base64str) {
			byte[] base64bytes = Convert.FromBase64String(base64str);
			return Encoding.UTF8.GetString(base64bytes);
		}

        public static string ToMD5(this string input) {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create()) {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string ToJSON<K, V>(this IDictionary<K, V> dict) {
            return JsonConvert.SerializeObject(dict);
        }

        public static string ToKeyValueString<K,V>(this Dictionary<K,V> keyValues, string joinerKV="=", string joinerEach=",", bool wrapEachWithBrackets=false) {
            if(keyValues==null || keyValues.Count==0) return "";

            string[] keyValueParts = new string[keyValues.Count];
            int k=0;
            foreach(K key in keyValues.Keys) {
                string str = key.ToString() + joinerKV + keyValues[key].ToString();
                keyValueParts[k++] = wrapEachWithBrackets ? "[" + str + "]" : str;
            }
            return keyValueParts.Join(joinerEach);
        }

        public static List<KeyValuePair<K,float>> ToSortedList<K>(this IDictionary<K, float> dict, bool isDescending=false) {
            var list = new List<KeyValuePair<K, float>>();

            foreach (KeyValuePair<K, float> kv in dict) {
                list.Add(kv);
            }

            list.Sort( (kv1, kv2) => { return Math.Sign(kv1.Value - kv2.Value); });
            if(isDescending) list.Reverse();

            return list;
        }

        public static V[] ValuesArray<K,V>(this IDictionary<K,V> dict) {
            V[] values = new V[dict.Count];
            int v=0;
            foreach(K key in dict.Keys) {
                values[v++] = dict[key];
            }

            return values;
        }

        //public static Color32 ToHexColor32(this string hex, bool isARGB = false) {
        //	int argb = Int32.Parse(hex.Replace("#", ""), NumberStyles.HexNumber);
        //	byte[] channels = new byte[] {
        //		(byte) ((argb >> 24) & 0xff),
        //		(byte) ((argb >> 16) & 0xff),
        //		(byte) ((argb >> 8) & 0xff),
        //		(byte) ((argb >> 0) & 0xff),
        //	};

        //	if (isARGB) return new Color32(channels[1], channels[2], channels[3], channels[0]);
        //	return new Color32(channels[0], channels[1], channels[2], channels[3]);
        //}

        public static string ForEach(this string str, Func<string, string> iterator) {
			string result = "";

			for (int s=0; s<str.Length; s++) {
				result += iterator("" + str[s]);
			}
			
			return result;
		}

		public static Color ToHexColor(this string hex, bool isRGBA=false) {
			hex = hex.Replace("#", "");

			//Convert shorthand hex (ie: #fff or #ffff) to full length hexcodes (6 or 8 hex symbols):
			if(hex.Length<5) hex = hex.ForEach((string ch) => ch + ch);
			if(hex.Length==6) hex = "ff" + hex;

			int argb = Int32.Parse(hex, NumberStyles.HexNumber);

			float[] channels = new float[] {
				(float) ((argb >> 24) & 0xff) / 0xff,
				(float) ((argb >> 16) & 0xff) / 0xff,
				(float) ((argb >> 8) & 0xff) / 0xff,
				(float) ((argb >> 0) & 0xff) / 0xff,
			};

			if (isRGBA) {
				return new Color(channels[0], channels[1], channels[2], channels[3]);
			} else {
				return new Color(channels[1], channels[2], channels[3], channels[0]);
			}
		}

		public static int CountChar(this string str, string sub) {
			int count = 0;
			int lastIndex = 0;
			while(lastIndex!=-1) {
				lastIndex = str.IndexOf(sub, lastIndex);
				if (lastIndex > -1) {
					count++;
					lastIndex += sub.Length;
				}
			}

			return count;
		}

		public static T AsEnum<T>(this string str) {
            string errStr = "Could not resolve '{0}' to Enum {1}".Format2(str, typeof(T));

            if (string.IsNullOrEmpty(str)) {
                Tracer.traceWarn(errStr);
                return default(T);
            }

            T value = default(T);
            try { 
			    value = (T) Enum.Parse(typeof(T), str, true);
            } catch(Exception err) {
                Tracer.traceWarn(errStr + "\n- Using default value instead: " + value);
            }

            return value;
		}

        public static T AsEnum<T>(this JSONNode json, bool decodeAsURI=false) {
            string str = (string)json;
            if(decodeAsURI) str = str.DecodeURL();
            return str.AsEnum<T>();
        }

        private static Regex _IS_NUMERIC = new Regex("^\\d+$");

		public static bool IsNumeric(this string value) {
			return _IS_NUMERIC.IsMatch(value);
		}

		//////////////////////////////////////////////////////////// For Colors:

		public static Color32 ToColor32(this Color clr) {
			return new Color32((byte)(clr.r * 0xff), (byte)(clr.g * 0xff), (byte)(clr.b * 0xff), (byte)(clr.a * 0xff));
		}

		//////////////////////////////////////////////////////////// For Math:
		
		//Convenience math functions (may be a bit excessive on the convenience... meh!)

		public static float Clamp(this float value, float min, float max) {
			return Mathf.Clamp(value, min, max);
		}

		public static float Lerp(this float from, float to, float interpolate=0.5f) {
			return Mathf.Lerp(from, to, interpolate);
		}

		public static Vector2 Lerp(this Vector2 from, Vector2 to, float interpolate=0.5f) {
			return Vector2.Lerp(from, to, interpolate);
		}

		public static Vector2 CloneOffset(this Vector2 from, float offsetX, float offsetY) {
			return new Vector2(from.x + offsetX, from.y + offsetY);
		}

		public static float AngleRelativeTo(this Vector2 from, Vector2 to, bool flipY) {
			return AngleRelativeTo(from, to, 1, flipY ? -1 : 1);
		}

		public static float AngleRelativeTo(this Vector2 from, Vector2 to, float flipX=1, float flipY=1) {
			return Mathf.Atan2((to.y - from.y) * flipY, (to.x - from.x) * flipX) * Mathf.Rad2Deg;
		}

		////////////////////////////////////////////////////////////

		public static float DistanceAll(this Points points) {
			if (points.Count < 2) return 0;
			float dist = 0;
			Vector2 prevPoint = points[0];
			for (int p = 1; p < points.Count; p++) {
				Vector2 point = points[p];
				dist += Vector2.Distance(prevPoint, point);
				prevPoint = point;
			}

			return dist;
		}

		//////////////////////////////////////////////////////////// For Arrays/Lists:

		public static T Last<T>(this T[] arr) {
			return arr.Length==0 ? default(T)  : arr[arr.Length - 1];
		}

		public static T Last<T>(this List<T> list) {
			return list.Count==0 ? default(T) : list[list.Count - 1];
		}

		public static T Pop<T>(this List<T> list) {
			T last = list.Last();
			list.RemoveAt(list.Count - 1);
			return last;
		}

		public static T Shift<T>(this List<T> list) {
			T item = list[0];
			list.RemoveAt(0);
			return item;
		}

		//Returns TRUE if successfully added, FALSE if already in the list:
		public static bool AddUnique<T>(this List<T> list, T other) {
			if (list.Contains(other)) return false;
			list.Add(other);
			return true;
		}

		public static void AddUniques<T>(this List<T> list, List<T> others) {
			foreach (T other in others) {
				if (list.Contains(other)) continue;
				list.Add(other);
			}
		}

		public static void AddUniquesBetween(this List<int> list, int start, int end) {
			for (int i = start; i <= end; i++) {
				if(list.Contains(i)) continue;
				list.Add(i);
			}
		}

		public static void AddMany<T>(this List<T> list, params T[] items) {
			foreach (T item in items) {
				list.Add(item);
			}
		}

		public static List<T> AddRanges<T>(this List<T> list, params List<T>[] otherLists) {
			foreach(List<T> otherList in otherLists) {
				list.AddRange(otherList);
			}

			return list;
		}

		public static bool ContainsAny<T>(this List<T> list, params T[] items) {
			foreach (T item in items) {
				if (list.Contains(item)) return true;
			}
			return false;
		}

		public static bool ContainsAll<T>(this List<T> list, params T[] items) {
			foreach (T item in items) {
				if (!list.Contains(item)) return false;
			}
			return true;
		}

		public static bool ContainsAllBetween(this List<int> list, int start, int end) {
			for(int i=start; i<=end; i++) {
				if(!list.Contains(i)) return false;
			}

			return true;
		}

		public static void ReplaceLast<T>(this T[] arr, T item) {
			arr[arr.Length - 1] = item;
		}

		public static void ReplaceLast<T>(this List<T> list, T item) {
			list[list.Count - 1] = item;
		}

		public static T[] Slice<T>(this T[] data, int start, int end = -1) {
			if (end < 0) end = data.Length - 1;
			int length = 1 + (end - start);
			T[] result = new T[length];
			Array.Copy(data, start, result, 0, length);
			return result;
		}

		public static List<T> Slice<T>(this List<T> data, int start, int end = -1) {
			if (end < 0) end = data.Count - 1;
			int length = 1 + (end - start);
			return data.GetRange(start, length);
		}

		public static T[] UpdateAll<T>(this T[] data, Func<T, T> func) {
			for (int i = 0; i < data.Length; i++) {
				data[i] = func(data[i]);
			}

			return data;
		}

		public static List<T> UpdateAll<T>(this List<T> data, Func<T, T> func) {
			for (int i = 0; i < data.Count; i++) {
				data[i] = func(data[i]);
			}
			
			return data;
		}

		public static int CountUntil<T>(this List<T> list, Func<T, int, bool> boolFunc, int startID=0, int endID=-1) {
			if(endID<0) endID = list.Count - 1;
			int sum = 0;

			for (int i=startID; i<=endID; i++) {
				if (boolFunc(list[i], i)) break;
				sum++;
			}

			return sum;
		}

		public static List<T> RemoveIf<T>(this List<T> list, Func<T, bool> boolFunc) {
			for(int i=list.Count; --i>=0;) {
				T item = list[i];
				if(!boolFunc(item)) continue;
				list.RemoveAt(i);
			}

			return list;
		}

        public static string ToLines(this List<string> list) {
            return list.ToArray().Join("\n");
        }

        public static string Join(this List<string> list, string delim=", ") {
            return list.ToArray().Join(delim);
        }

        public static string Join(this JSONArray jsonArr, string delim=", ") {
            var list = new List<string>();

            foreach (JSONNode json in jsonArr) {
                list.Add((string)json);
            }

            return list.Join(delim);
        }

        //////////////////////////////////////////////////////////// For Transforms:

        ////////// MoveTo*** (immediate position, in local coordinates)

        public static Vector3 MoveToX(this Transform trans, float value) {
            Vector3 pos = trans.localPosition;
            pos.x = value;
            trans.localPosition = pos;
            return pos;
        }

        public static Vector3 MoveToY(this Transform trans, float value) {
            Vector3 pos = trans.localPosition;
            pos.y = value;
            trans.localPosition = pos;
            return pos;
        }

        public static Vector3 MoveToZ(this Transform trans, float value) {
            Vector3 pos = trans.localPosition;
            pos.z = value;
            trans.localPosition = pos;
            return pos;
        }

        public static Vector3 MoveToXY(this Transform trans, float x, float y) {
            Vector3 pos = trans.localPosition;
            pos.x = x;
            pos.y = y;
            trans.localPosition = pos;
            return pos;
        }

        ////////// MoveBy*** (relative from current position, in local coordinates)

        public static Vector3 MoveByX(this Transform trans, float value) {
            Vector3 pos = trans.localPosition;
            pos.x += value;
            trans.localPosition = pos;
            return pos;
        }

        public static Vector3 MoveByY(this Transform trans, float value) {
            Vector3 pos = trans.localPosition;
            pos.y += value;
            trans.localPosition = pos;
            return pos;
        }

        public static Vector3 MoveByZ(this Transform trans, float value) {
            Vector3 pos = trans.localPosition;
            pos.z += value;
            trans.localPosition = pos;
            return pos;
        }

        public static Vector3 MoveByXY(this Transform trans, float x, float y) {
            Vector3 pos = trans.localPosition;
            pos.x += x;
            pos.y += y;
            trans.localPosition = pos;
            return pos;
        }

        //////////////////////////////////////////////////////////// Tween Relative

        public static Tweener TweenMoveByX(this Transform trans, float value, float duration = 1) {
            Vector3 pos = trans.localPosition;
            pos.x += value;
            return trans.DOLocalMoveX(pos.x, duration);
        }

        public static Tweener TweenMoveByY(this Transform trans, float value, float duration = 1) {
            Vector3 pos = trans.localPosition;
            pos.y += value;
            return trans.DOLocalMoveY(pos.y, duration);
        }

        public static Tweener TweenMoveByZ(this Transform trans, float value, float duration = 1) {
            Vector3 pos = trans.localPosition;
            pos.z += value;
            return trans.DOLocalMoveZ(pos.z, duration);
        }

        public static Tweener TweenMoveByXY(this Transform trans, float x, float y, float duration = 1) {
            Vector3 pos = trans.localPosition;
            pos.x += x;
            pos.y += y;
            return trans.DOLocalMove(pos, duration);
        }

        //////////////////////////////////////////////////////////// For GameObjects:

        public static void ForEachChild(this GameObject go, Action<GameObject> callbackForEach) {
            Transform trans = go.transform;
            int total = trans.childCount;
            for(int t=0; t<total; t++) {
                GameObject child = trans.GetChild(t).gameObject;
                callbackForEach(child);
            }
        }

        public static void AddChild(this Component parent, GameObject other) {
            other.transform.parent = parent.transform;
        }

        public static void AddChild(this GameObject parent, GameObject other) {
            other.transform.parent = parent.transform;
        }

        public static void AddChild(this GameObject parent, Component other) {
            other.transform.parent = parent.transform;
        }

        public static T AddClone<T>(this Component parent, T original) where T : Component {
            return parent.gameObject.AddClone(original);
        }

        public static T AddClone<T>(this GameObject parent, T original) where T : Component {
            T dup = GameObject.Instantiate(original);
            dup.transform.parent = parent.transform;
            return dup;
        }

        public static T Clone<T>(this Component parent, GameObject what) where T : Component {
            var clone = GameObject.Instantiate(what);
            clone.transform.parent = what.transform.parent;
            clone.transform.localScale = Vector2.one;
            clone.SetActive(true);

            return clone.GetComponent<T>();
        }

        public static string GetFullName(this Component comp) {
            List<string> names = new List<string>();
            Transform current = comp.transform;

            while(current!=null) {
                names.Add( current.name );

                current = current.parent;
            }
            names.Reverse();

            return names.Join("/");
        }

        /**
         * duration: If negative (-1) then the duration is measured in frames and counts upwards towards 0.
         *          Else, if positive or zero, the duration is in seconds.
         * callback: Any callback / lambda with zero arguments in its method-signature.
         */
        public static void Wait(this MonoBehaviour comp, float duration, Action callback) {
            comp.StartCoroutine(__Wait(duration, callback));
        }

        private static IEnumerator __Wait(float duration, Action callback) {
            if(duration<0) {
                while(duration!=0) {
                    yield return new WaitForEndOfFrame();
                    duration++;
                }
            } else {
                yield return new WaitForSeconds(duration);
            }

            callback();
        }

        public static RectTransform GetRect(this MonoBehaviour mono) {
            return (RectTransform)mono.transform;
        }

        public static RectTransform GetRect(this GameObject go) {
            return (RectTransform) go.transform;
        }

        public static void SetWidthAndHeight(this RectTransform rect, float width, float height=-1) {
            if(height<0) height = width;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public static void SetEnabledInChildren<T>(this MonoBehaviour mono, bool isEnabled) where T : MonoBehaviour {
            T[] fitters = mono.GetComponentsInChildren<T>();
            foreach (var fitter in fitters) {
                fitter.enabled = isEnabled;
            }
        }
        
        // Found at: http://answers.unity3d.com/questions/799429/transformfindstring-no-longer-finds-grandchild.html
        public static Transform FindDeepChild(this Transform aParent, string name) {
            var result = aParent.Find(name);
            if (result != null)
                return result;

            foreach (Transform child in aParent) {
                result = child.FindDeepChild(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static Transform FindChildByRegex(this Transform aParent, string regexPattern, bool includeChildSelf=false, RegexOptions options=RegexOptions.None) {
            return FindChildByRegex(aParent, new Regex(regexPattern, options), includeChildSelf);
        }
        public static Transform FindChildByRegex(this Transform aParent, Regex regex, bool includeSelf = false) {
            if(includeSelf && regex.IsMatch(aParent.name)) {
                return aParent;
            }

            foreach (Transform child in aParent) {
                if (regex.IsMatch(child.name)) return child;

                Transform result = child.FindChildByRegex(regex);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void PopulateListFromEachChild<T>(this List<T> list, GameObject container, string namePattern, bool includeChildSelf=false) {
            Transform trans = container.transform;
            int numActors = container.transform.childCount;
            Type type = typeof(T);

            for (int a = 0; a < numActors; a++) {
                Transform childTrans = trans.GetChild(a);
                Transform foundTrans = childTrans.FindChildByRegex(namePattern, includeChildSelf); //System.Text.RegularExpressions.RegexOptions.IgnoreCase
                if (foundTrans == null) {
                    Debug.LogError("Could not find {0} at index: ".Format2(type) + a);
                    continue;
                }
                list.Add(foundTrans.GetComponent<T>());
            }
        }

        public static GameObject GetOrCreate(this List<GameObject> list, GameObject prefab, Component parent = null, bool isAnchoredZero = false, bool isLastSibling = true) {
            foreach (GameObject go in list) {
                if (go.activeSelf == false) {
                    go.SetActive(true);
                    return go;
                }
            }

            GameObject newItem = (GameObject)GameObject.Instantiate(prefab);
            list.Add(newItem);

            if (parent != null) {
                var rect = newItem.GetRect();
                rect.SetParent(parent.transform);
                rect.localScale = Vector3.one;
                if (isAnchoredZero) rect.anchoredPosition = Vector2.zero;
                if (isLastSibling) rect.SetAsLastSibling();
            }

            return newItem;
        }

        public static T GetOrCreate<T>(this List<T> list, GameObject prefab, Component parent = null, bool isAnchoredZero = false, bool isLastSibling = true) where T : Component {
            foreach (T comp in list) {
                if (comp.gameObject.activeSelf == false) {
                    comp.gameObject.SetActive(true);
                    return comp;
                }
            }

            GameObject go = (GameObject)GameObject.Instantiate(prefab);
            T newItem = go.GetComponent<T>();
            list.Add(newItem);

            if (parent != null) {
                var rect = go.GetRect();
                rect.SetParent(parent.transform);
                rect.localScale = Vector3.one;
                if (isAnchoredZero) rect.anchoredPosition = Vector2.zero;
                if (isLastSibling) rect.SetAsLastSibling();
            }

            return newItem;
        }

        public static void SetActiveForAll(this List<GameObject> list, bool isActive) {
            foreach (GameObject go in list) {
                if (go == null) continue;
                go.SetActive(isActive);
            }
        }

        public static void SetActiveForAll<T>(this List<T> list, bool isActive) where T : Component {
            foreach (T comp in list) {
                if (comp == null) continue;
                comp.gameObject.SetActive(isActive);
            }
        }

        //////////////////////////////////////////////////////////// Buttons

        public static void SetEnabledState(this Button btn, bool isEnabled) {
            btn.interactable = isEnabled;

            var btnLabel = btn.GetComponentInChildren<TextMeshProUGUI>();
            if(btnLabel==null) return;
            btnLabel.alpha = isEnabled ? 1.0f : 0.5f;
        }

        //////////////////////////////////////////////////////////// JSONNode

        public static bool Exists(this JSONNode node) {
            return node!=null;
        }

        public static string AsDecodedURL(this JSONNode json) {
            string str = (string)json;
            if (json == null || str == null) return null;
            return str.DecodeURL();
        }

        public static int[] AsArrayInt(this JSONNode json) {
            JSONArray jsonArr = json.AsArray;
            int[] results = new int[jsonArr.Count];

            for (int i = 0; i < jsonArr.Count; i++) {
                results[i] = jsonArr[i].AsInt;
            }

            return results;
        }

        public static float[] AsArrayFloat(this JSONNode json) {
            JSONArray jsonArr = json.AsArray;
            float[] results = new float[jsonArr.Count];

            for (int i = 0; i < jsonArr.Count; i++) {
                results[i] = jsonArr[i].AsFloat;
            }

            return results;
        }

        //////////////////////////////////////////////////////////// string

        public static bool Exists(this string str) {
            return !string.IsNullOrEmpty(str);
        }

        public static string ToJSONString(this object obj, bool isIndented=false) {
            return JsonConvert.SerializeObject(obj, isIndented ? Formatting.Indented : Formatting.None);
        }

        public static string ToUTF8(this byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] ToUTF8(this string str) {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string DecodeURL(this string url) {
            if(url==null) return null;
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        public static string EncodeURL(this string url) {
            string newUrl;
            while ((newUrl = Uri.EscapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        //////////////////////////////////////////////////// 

        public static Sprite ToSprite(this Texture2D tex) {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        //////////////////////////////////////////////////// Streams:

        private static byte[] PrepareBuffer(this Stream input) {
            //input.Seek(0, SeekOrigin.Begin);

            return new byte[8 * 1024];
        }

        public static void CopyTo(this Stream input, Stream output) {
            byte[] buffer = PrepareBuffer(input);
            
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0) {
                output.Write(buffer, 0, len);
            }
        }

        public static byte[] ToBytes(this Stream input) {
            byte[] buffer = PrepareBuffer(input);

            using (MemoryStream ms = new MemoryStream()) {
                int len;
                while ((len = input.Read(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, len);
                }
                return ms.ToArray();
            }
        }

        public static void WriteToFile(this Stream input, string filename) {
            using (FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                input.CopyTo(file);
            }
        }


        //////////////////////////////////////////////////// DateTime:

        public static string ToNodeDateTime(this DateTime date) {
            return date.ToString("yyyy-MM-ddTHH:mm:ssK");
        }

        public static DateTime FromNodeDateTime(this string str) {
            string[] strSplit = str.Split('T');
            string dateStr = strSplit[0];
            string timeStr = strSplit[1];
            timeStr = timeStr.Substring(0, timeStr.IndexOf("."));
            string fullStr = dateStr + " " + timeStr;
            return DateTime.Parse(fullStr);
        }

        public static string FormatMinSeconds(this int seconds) {
            int timeMins = (int)(seconds / 60);
            int timeSecs = (int)seconds % 60;

            return timeMins.ToString("00") + ":" + timeSecs.ToString("00");
        }

        public static string ToHHMMSS(this float secondsTotal, string spacer = null, bool isMonospaceHTML = false) {
            return ((int)secondsTotal).ToHHMMSS(spacer, isMonospaceHTML);
        }

        public static string ToHHMMSS(this double secondsTotal, string spacer = null, bool isMonospaceHTML = false) {
            return ((int)secondsTotal).ToHHMMSS(spacer, isMonospaceHTML);
        }

        public static string ToHHMMSS(this int secondsTotal, string spacer = null, bool isMonospaceHTML = false) {
            int hours = Mathf.FloorToInt(secondsTotal / 60 / 60);
            int mins = Mathf.FloorToInt(secondsTotal / 60) % 60;
            int seconds = secondsTotal % 60;

            if(spacer==null) spacer = DateTime.Now.Millisecond < 500 ? ":" : " ";

            string HHMMSS = hours.ToString("00") + spacer + mins.ToString("00") + spacer + seconds.ToString("00");

            return isMonospaceHTML ? "<mspace=2.2em>" + HHMMSS + "</mspace>" : HHMMSS;
        }

    }//END ExtensionMethods


}//END namespace

public class EnumUtils {
    public static void ForEach<T>(Action<T> cbForEach) {
        foreach (T value in Enum.GetValues(typeof(T))) {
            cbForEach(value);
        }
    }

    public static string[] ToStringArray<T>() {
        Array enums = Enum.GetValues(typeof(T));
        string[] results = new string[enums.Length];
        for(int i=0; i<enums.Length; i++) {
            results[i] = enums.GetValue(i).ToString();
        }

        return results;
    }
}
