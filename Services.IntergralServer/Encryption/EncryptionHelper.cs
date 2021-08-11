using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Integral.WebApi.Encryption
{
    /// <summary>
    /// 加密帮助类（统一采用UTF-8格式）
    /// </summary>
    public class EncryptionHelper
    {
        /// <summary>
        /// 加密关键字
        /// </summary>
        private static string Key { get; set; } = "GhGzPt";
        /// <summary>
        /// 配置加密字符串
        /// </summary>
        /// <param name="key"></param>
        public static void Configuration(string key)
        {
            Key = key;
        }
        /// <summary>
        /// HMACSHA256加密（使用默认秘钥）
        /// </summary>
        /// <param name="value">需要加密的值</param>
        /// <returns>加密后的值</returns>
        public static string HMACSHA256Encryption(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            return HMACSHA256Encryption(value, Key);
        }
        /// <summary>
        /// HMACSHA256加密
        /// </summary>
        /// <param name="value">需要加密的值</param>
        /// <param name="key">秘钥</param>
        /// <returns>加密后的值</returns>
        public static string HMACSHA256Encryption(string value, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("secret key is null");
            if (string.IsNullOrWhiteSpace(value))
                return value;
            var encoding = new UTF8Encoding();
            using (var hmacsha256 = new HMACSHA256(encoding.GetBytes(key)))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(encoding.GetBytes(value));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashmessage.Length; i++)
                {
                    builder.Append(hashmessage[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="value">待加密串</param>
        /// <param name="encoding">加密编码</param>
        /// <returns></returns>
        public static string MD5Encryption(string value, Encoding encoding)
        {
            byte[] b = encoding.GetBytes(value);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string result = "";
            for (int i = 0; i < b.Length; i++)
                result = $"{result}{b[i].ToString("X2")}";
            return result;
        }
        /// <summary>
        /// DES加密（使用默认秘钥）
        /// </summary>
        /// <param name="value">待加密串</param>
        /// <returns></returns>
        public static string DESEncryption(string value)
        {
            return DESEncryption(value, Key);
        }
        /// <summary>
        /// DES加密（使用默认秘钥）
        /// </summary>
        /// <param name="value">待加密串</param>
        /// <param name="key">秘钥</param>
        /// <returns></returns>
        public static string DESEncryption(string value, string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.UTF8.GetBytes(value);
            des.Key = Encoding.ASCII.GetBytes(MD5Encryption(key, Encoding.UTF8).Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(MD5Encryption(key, Encoding.UTF8).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }
        /// <summary>
        /// DES解密（使用默认秘钥）
        /// </summary>
        /// <param name="value">已加密串</param>
        /// <returns></returns>
        public static string DESDecryption(string value)
        {
            return DESDecryption(value, Key);
        }
        /// <summary>
        /// DES解密（使用默认秘钥）
        /// </summary>
        /// <param name="value">已加密串</param>
        /// <param name="key">秘钥</param>
        /// <returns></returns>
        public static string DESDecryption(string value, string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = new byte[value.Length / 2];
            int x, i;
            for (x = 0; x < inputByteArray.Length; x++)
            {
                i = Convert.ToInt32(value.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            des.Key = Encoding.ASCII.GetBytes(MD5Encryption(key, Encoding.UTF8).Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(MD5Encryption(key, Encoding.UTF8).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        /// <summary>
        /// 获取MD5加密的摘要信息（64位）
        /// </summary>
        /// <param name="value">待加密串</param>
        /// <param name="hkey">摘要秘钥</param>
        /// <returns></returns>
        public static string GetMd5Abstract(string value,string hkey)
        {
            return HMACSHA256Encryption(MD5Encryption(value, Encoding.UTF8), hkey);
        }

		/// <summary>
        /// 加密
        /// </summary>
        /// <typeparam name="T">输入类型</typeparam>
        /// <param name="secretKey">密钥</param>
        /// <param name="source">加密原始数据</param>
        /// <returns></returns>
        public static string BaseAseEncrypt<T>(string secretKey, T source)
        {
            if (typeof(T) == typeof(string))
                return AseDese.DesEncrypt(source.ToString(), secretKey);
            return AseDese.DesEncrypt(JsonConvert.SerializeObject(source), secretKey);
        }
        /// <summary>
        /// 解密（返回泛型）
        /// </summary>
        /// <typeparam name="T">输出类型</typeparam>
        /// <param name="secretKey">密钥</param>
        /// <param name="secretStr">加密原始数据</param>
        /// <returns></returns>
        public static T BaseAseDecrypt<T>(string secretKey, string secretStr)
        {
            var result = AseDese.DesDecrypt(secretStr, secretKey);
            if (result == "") return default(T);
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
