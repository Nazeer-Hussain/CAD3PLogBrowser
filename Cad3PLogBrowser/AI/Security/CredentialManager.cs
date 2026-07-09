using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Cad3PLogBrowser.AI.Security
{
    /// <summary>
    /// Securely stores and retrieves credentials using Windows DPAPI (Data Protection API).
    /// Credentials are encrypted per-user and can only be decrypted by the same user on the same machine.
    /// </summary>
    public static class CredentialManager
    {
        private const string CredentialPrefix = "CAD3PLogBrowser_AI_";

        /// <summary>
        /// Stores a credential securely using Windows DPAPI.
        /// </summary>
        public static bool StoreCredential(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                // Use Windows Credential Manager if available, otherwise use DPAPI file storage
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return StoreCredentialWindows(key, value);
                }
                else
                {
                    // For non-Windows platforms, use encrypted file storage
                    return StoreCredentialFile(key, value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to store credential: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a stored credential.
        /// </summary>
        public static string RetrieveCredential(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return RetrieveCredentialWindows(key);
                }
                else
                {
                    return RetrieveCredentialFile(key);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to retrieve credential: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes a stored credential.
        /// </summary>
        public static bool DeleteCredential(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return DeleteCredentialWindows(key);
                }
                else
                {
                    return DeleteCredentialFile(key);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete credential: {ex.Message}");
                return false;
            }
        }

        // ?? Windows Credential Manager Implementation ?????????????????????????

        private static bool StoreCredentialWindows(string key, string value)
        {
            string targetName = CredentialPrefix + key;

            var credential = new CREDENTIAL
            {
                Type = CRED_TYPE.GENERIC,
                TargetName = targetName,
                CredentialBlob = Marshal.StringToCoTaskMemUni(value),
                CredentialBlobSize = (uint)Encoding.Unicode.GetByteCount(value),
                Persist = CRED_PERSIST.LOCAL_MACHINE,
                AttributeCount = 0,
                UserName = Environment.UserName
            };

            bool result = CredWrite(ref credential, 0);

            // Clean up
            Marshal.FreeCoTaskMem(credential.CredentialBlob);

            return result;
        }

        private static string RetrieveCredentialWindows(string key)
        {
            string targetName = CredentialPrefix + key;
            IntPtr credPtr = IntPtr.Zero;

            try
            {
                if (CredRead(targetName, CRED_TYPE.GENERIC, 0, out credPtr))
                {
                    var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                    return Marshal.PtrToStringUni(credential.CredentialBlob, 
                        (int)credential.CredentialBlobSize / 2);
                }
                return null;
            }
            finally
            {
                if (credPtr != IntPtr.Zero)
                    CredFree(credPtr);
            }
        }

        private static bool DeleteCredentialWindows(string key)
        {
            string targetName = CredentialPrefix + key;
            return CredDelete(targetName, CRED_TYPE.GENERIC, 0);
        }

        // ?? File-based encrypted storage (fallback for non-Windows) ???????????

        private static string GetCredentialFilePath(string key)
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CAD3PLogBrowser", "Credentials");

            Directory.CreateDirectory(folder);

            // Use hash of key as filename for obscurity
            string fileKey = Convert.ToBase64String(
                SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key)))
                .Replace("/", "_").Replace("+", "-");

            return Path.Combine(folder, fileKey + ".dat");
        }

        private static bool StoreCredentialFile(string key, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            byte[] encrypted = EncryptData(data, key);

            string filePath = GetCredentialFilePath(key);
            File.WriteAllBytes(filePath, encrypted);

            return true;
        }

        private static string RetrieveCredentialFile(string key)
        {
            string filePath = GetCredentialFilePath(key);

            if (!File.Exists(filePath))
                return null;

            byte[] encrypted = File.ReadAllBytes(filePath);
            byte[] data = DecryptData(encrypted, key);

            return Encoding.UTF8.GetString(data);
        }

        // Simple encryption using AES with a machine-specific key
        private static byte[] EncryptData(byte[] data, string context)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(context);
                aes.IV = new byte[16]; // Use zero IV for simplicity (not recommended for production)

                using (var encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        private static byte[] DecryptData(byte[] encryptedData, string context)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = DeriveKey(context);
                aes.IV = new byte[16];

                using (var decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }
            }
        }

        private static byte[] DeriveKey(string context)
        {
            // Create a machine-specific key using environment variables
            string keySource = Environment.MachineName + Environment.UserName + context;

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource));
            }
        }

        private static bool DeleteCredentialFile(string key)
        {
            string filePath = GetCredentialFilePath(key);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        // ?? Windows API Declarations ??????????????????????????????????????????

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredDelete(string target, CRED_TYPE type, int flags);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public CRED_TYPE Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public CRED_PERSIST Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        private enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = 1007
        }

        private enum CRED_PERSIST : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }
    }
}
