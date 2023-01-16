using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Back_End.Clases
{
    public class Utilerias
    {
        private AppSettings _appSettings;

        private AppSettings AppSettings
        {
            get
            {
                if (_appSettings == null)
                {
                    _appSettings = new AppSettings();
                }

                return _appSettings;
            }
        }

        public string ModificarToken(string token)
        {
            //abb54145 - 6ec8 - 5a67 - 9c85 - 06c3bb7d650f
            //     0-7   9-12  14-17   19-22   23-34
            //6ec85a67 - abb5 - 4145 - 9c85 - 06c3bb7d650f

            string tokenParte1;
            string tokenParte2;
            string tokenParte3;
            string tokenParte4;
            string tokenParte5;
            string tokenFinal;

            tokenParte1 = token.Substring(9, 9).Replace("-", "");
            tokenParte2 = token.Substring(0, 4);
            tokenParte3 = token.Substring(4, 4);
            tokenParte4 = token.Substring(19, 4);
            tokenParte5 = token.Substring(24);

            tokenFinal = string.Format("{0}-{1}-{2}-{3}-{4}", tokenParte1, tokenParte2, tokenParte3, tokenParte4, tokenParte5);

            return tokenFinal;
        }
        public string Desencryptar(string textoEncryptado)
        {
            // Obtener las llaves necesarias para encriptar y desencriptar
            byte[] key = Encoding.UTF8.GetBytes(this.AppSettings.KeyEncryptacion);
            byte[] iv = Encoding.UTF8.GetBytes(this.AppSettings.IVEncryptacion);

            // Longitud requeridas para las llaves
            int keySize = 32;
            int ivSize = 16;

            // Para evitar errores, se ajutaran las llaves a la longitud necesaria
            Array.Resize(ref key, keySize);
            Array.Resize(ref iv, ivSize);

            // Obtener la representación en bytes del texto cifrado
            byte[] cipherTextBytes = Convert.FromBase64String(textoEncryptado);

            // Crear un arreglo de bytes para almacenar los datos descifrados
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            // Crear una instancia del algoritmo de Rijndael
            Rijndael RijndaelAlg = Rijndael.Create();

            // Crear un flujo en memoria con la representación de bytes de la información cifrada
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);

            // Crear un flujo de descifrado basado en el flujo de los datos
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                         RijndaelAlg.CreateDecryptor(key, iv),
                                                         CryptoStreamMode.Read);

            // Obtener los datos descifrados obteniéndolos del flujo de descifrado
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            // Cerrar los flujos utilizados
            memoryStream.Close();
            cryptoStream.Close();

            // Retornar la representación de texto de los datos descifrados
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
        public string Encryptar(string texto)
        {
            // Obtener las llaves necesarias para encriptar y desencriptar
            byte[] key = Encoding.UTF8.GetBytes(this.AppSettings.KeyEncryptacion);
            byte[] iv = Encoding.UTF8.GetBytes(this.AppSettings.IVEncryptacion);

            // Longitud requeridas para las llaves
            int keySize = 32;
            int ivSize = 16;

            //Para evitar errores, se ajutaran las llaves a la longitud necesaria
            Array.Resize(ref key, keySize);
            Array.Resize(ref iv, ivSize);

            Rijndael RijndaelAlg = Rijndael.Create();
            // Establecer un flujo en memoria para el cifrado
            MemoryStream memoryStream = new MemoryStream();
            // Crear un flujo de cifrado basado en el flujo de los datos
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                         RijndaelAlg.CreateEncryptor(key, iv),
                                                         CryptoStreamMode.Write);

            // Obtener la representación en bytes de la información a cifrar
            byte[] plainMessageBytes = Encoding.UTF8.GetBytes(texto);

            // Cifrar los datos enviándolos al flujo de cifrado
            cryptoStream.Write(plainMessageBytes, 0, plainMessageBytes.Length);
            cryptoStream.FlushFinalBlock();

            // Obtener los datos datos cifrados como un arreglo de bytes
            byte[] cipherMessageBytes = memoryStream.ToArray();

            // Cerrar los flujos utilizados
            memoryStream.Close();
            cryptoStream.Close();

            // Retornar la representación de texto de los datos cifrados
            return Convert.ToBase64String(cipherMessageBytes);
        }
        public static string ByteABase64(byte[] binario)
        {
            return Encoding.ASCII.GetString(binario);
        }

        public static void DestruirDataTable(ref DataTable tb)
        {
            if (tb != null)
            {
                tb.Dispose();
                tb = null;
            }
        }
        public static void DestruirDataSet(ref DataSet dst)
        {
            if (dst != null)
            {
                dst.Dispose();
                dst = null;
            }
        }
        /// <summary>
        /// Destruye un objeto <see cref="SqlDataAdapter"/> validando si esta nulo.
        /// </summary>
        /// <param name="da"><see cref="SqlDataAdapter"/> que quiere destruir.</param>
        public static void DestruirDataAdapter(ref SqlDataAdapter da)
        {
            if (da != null)
            {
                da.Dispose();
                da = null;
            }
        }
        /// <summary>
        /// Destruye un objeto <see cref="SqlCommand"/> validando si esta nulo.
        /// </summary>
        /// <param name="sqlCom"><see cref="SqlCommand"/> que quiere destruir.</param>
        public static void DestruirSqlCommand(ref SqlCommand sqlCom)
        {
            if (sqlCom != null)
            {
                sqlCom.Dispose();
                sqlCom = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0038:Usar coincidencia de patrones", Justification = "No todos los objetos son Disposables")]
        public void DestruirObjeto(ref object obj)
        {
            if (obj != null)
            {
                if (obj is IDisposable)
                {
                    ((IDisposable)obj).Dispose();
                }

                obj = null;
            }
        }
        /// <summary>
        /// Esta funcion realiza la validacion que el <paramref name="row"/> no sea Nothing y el valor de la columna en caso de ser
        /// DbNull regrese el valor por Default.
        /// </summary>
        /// <typeparam name="T">Tipo de datos a regresar</typeparam>
        /// <param name="row"><see cref="DataRow"/> donde estan los datos.</param>
        /// <param name="nombreColumna">Nombre de la columna</param>
        /// <param name="valorDefault">Valor que retornara la funcion en caso de no pasar las validaciones</param>
        /// <returns></returns>
        public static T DefaultDbNull<T>(ref DataRow row, string nombreColumna, T valorDefault)
        {
            if (row == null)
            {
                return valorDefault;
            }
            else
            {
                return DefaultDbNull(row[nombreColumna], valorDefault);
            }
        }
        public static T DefaultDbNull<T>(Object valor, T valorDefault)
        {
            if (valor == DBNull.Value)
            {
                return (T)valorDefault;
            }
            else
            {
                return (T)valor;
            }
        }

        public bool ValidarCorreo(string correo)
        {
            if (correo != null || correo.Length != 0)
            {
                return Regex.IsMatch(correo, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            }
            else
            {
                return false;
            }
        }

        public bool ValidarRFC(string RFC)
        {
            if (RFC != null)
            {
                //ATE 20 10 21 AB6
                //CAG 00 03 16 CM1
                return Regex.IsMatch(RFC, @"^([A-ZÑ\x26]{3,4}([0-9]{2})(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1]))([A-Z\d]{3})?$");
                //var a = / ^([A - ZÑ &]{ 3,4}) ?(?: - ?) ? (\d{ 2} (?: 0[1 - 9] | 1[0 - 2])(?:0[1 - 9] |[12]\d | 3[01])) ?(?: - ?) ? ([A - Z\d]{ 2})([A\d])$/
            }
            else
            {
                return false;
            }
        }

        public List<T> DataTableAList<T>(DataTable tabla) where T : new()
        {
            List<T> list = new List<T>();

            foreach (DataRow row in tabla.Rows)
            {
                list.Add(DataTableAItem<T>(row, tabla.Columns));
            }

            return list;
        }

        public T DataTableAItem<T>(DataRow row, DataColumnCollection columns) where T : new()
        {
            if (row == null || columns == null || columns.Count == 0)
            {
                return default;
            }

            Type entityType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);
            T item = new T();

            foreach (DataColumn col in columns)
            {
                PropertyDescriptor propiedad = properties[col.ColumnName];
                var valor = row[col.ColumnName];

                if (propiedad != null && valor != null && valor != DBNull.Value)
                    propiedad.SetValue(item, row[col.ColumnName]);
            }

            return item;
        }

        public T DataTableAItem<T>(DataRow row) where T : new()
        {
            if (row == null || row.Table == null || row.Table.Rows.Count == 0)
            {
                return default;
            }

            return DataTableAItem<T>(row, row.Table.Columns);
        }

        public T DataTableAItem<T>(DataTable tb, int fila) where T : new()
        {
            // La variable fila inicia en 0 como un arreglo
            if (tb == null || tb.Rows.Count < fila + 1)
            {
                return default;
            }

            return DataTableAItem<T>(tb.Rows[fila], tb.Columns);
        }

        public T DataTableAItem<T>(DataTable tb) where T : new()
        {
            if (tb == null || tb.Rows.Count == 0)
            {
                return default;
            }

            return DataTableAItem<T>(tb.Rows[0], tb.Columns);
        }

        public string GetMyNumberToWords(decimal pValue)
        {
            decimal value;

            value = Math.Truncate(pValue);

            string str;
            switch (value)
            {
                case 0M:
                    {
                        str = "CERO";
                        break;
                    }

                case 1M:
                    {
                        str = "UNO";
                        break;
                    }

                case 2M:
                    {
                        str = "DOS";
                        break;
                    }

                case 3M:
                    {
                        str = "TRES";
                        break;
                    }

                case 4M:
                    {
                        str = "CUATRO";
                        break;
                    }

                case 5M:
                    {
                        str = "CINCO";
                        break;
                    }

                case 6M:
                    {
                        str = "SEIS";
                        break;
                    }

                case 7M:
                    {
                        str = "SIETE";
                        break;
                    }

                case 8M:
                    {
                        str = "OCHO";
                        break;
                    }

                case 9M:
                    {
                        str = "NUEVE";
                        break;
                    }

                case 10M:
                    {
                        str = "DIEZ";
                        break;
                    }

                case 11M:
                    {
                        str = "ONCE";
                        break;
                    }

                case 12M:
                    {
                        str = "DOCE";
                        break;
                    }

                case 13M:
                    {
                        str = "TRECE";
                        break;
                    }

                case 14M:
                    {
                        str = "CATORCE";
                        break;
                    }

                case 15M:
                    {
                        str = "QUINCE";
                        break;
                    }

                case object _ when value < 20M:
                    {
                        str = "DIECI" + GetMyNumberToWords(value - 10M);
                        break;
                    }

                case 20M:
                    {
                        str = "VEINTE";
                        break;
                    }

                case object _ when value < 30M:
                    {
                        if (value - 20M == 1)
                            str = "VEINTIUN";
                        else
                            str = "VEINTI" + GetMyNumberToWords(value - 20M);
                        break;
                    }

                case 30M:
                    {
                        str = "TREINTA";
                        break;
                    }

                case 40M:
                    {
                        str = "CUARENTA";
                        break;
                    }

                case 50M:
                    {
                        str = "CINCUENTA";
                        break;
                    }

                case 60M:
                    {
                        str = "SESENTA";
                        break;
                    }

                case 70M:
                    {
                        str = "SETENTA";
                        break;
                    }

                case 80M:
                    {
                        str = "OCHENTA";
                        break;
                    }

                case 90M:
                    {
                        str = "NOVENTA";
                        break;
                    }

                case object _ when value < 100M:
                    {
                        str = GetMyNumberToWords(Math.Truncate(value / 10M) * 10M) + " Y " + GetMyNumberToWords(value % 10M);
                        break;
                    }

                case 100M:
                    {
                        str = "CIEN";
                        break;
                    }

                case object _ when value < 200M:
                    {
                        str = "CIENTO " + GetMyNumberToWords((value - 100M));
                        break;
                    }

                case 200M:
                case 300M:
                case 400M:
                case 600M:
                case 800M:
                    {
                        str = GetMyNumberToWords(Math.Truncate(value / 100M)) + "CIENTOS";
                        break;
                    }

                case 500M:
                    {
                        str = "QUINIENTOS";
                        break;
                    }

                case 700M:
                    {
                        str = "SETECIENTOS";
                        break;
                    }

                case 900M:
                    {
                        str = "NOVECIENTOS";
                        break;
                    }

                case object _ when value < 1000M:
                    {
                        str = GetMyNumberToWords(Math.Truncate(value / 100M) * 100M) + " " + GetMyNumberToWords(value % 100M);
                        break;
                    }

                case 1000M:
                    {
                        str = "MIL";
                        break;
                    }

                case object _ when value < 2000M:
                    {
                        str = "MIL " + GetMyNumberToWords(value % 1000M);
                        break;
                    }

                case object _ when value < 1000000M:
                    {
                        str = GetMyNumberToWords(Math.Truncate(value / 1000M)) + " MIL";

                        if (((value / 1000M) % 100) == 1)
                            str = str.Replace("UNO", "UN");

                        if (value % 1000M > 0)
                        {
                            if ((value % 1000M) == 1)
                                str += " UNO";
                            else
                                str += " " + GetMyNumberToWords(value % 1000M);
                        }

                        break;
                    }

                case 1000000M:
                    {
                        str = "UN MILLON";
                        break;
                    }

                case object _ when value < 2000000M:
                    {
                        str = "UN MILLON " + GetMyNumberToWords(value % 1000000M);
                        break;
                    }

                case object _ when value < 1000000000000M:
                    {
                        str = GetMyNumberToWords(Math.Truncate(value / 1000000M)) + " MILLONES ";

                        if ((Math.Truncate(value / 1000000M)) % 1000M == 1)
                            str = str.Replace("UNO", "UN");

                        if ((value - Math.Truncate(value / 1000000M) * 1000000M) > 0)
                            str = str + " " + GetMyNumberToWords(value - Math.Truncate(value / 1000000M) * 1000000M);
                        break;
                    }

                case 1000000000000M:
                    {
                        str = "UN BILLON";
                        break;
                    }

                case object _ when value < 2000000000000M:
                    {
                        str = "UN BILLON " + GetMyNumberToWords(value - Math.Truncate(value / 1000000000000M) * 1000000000000M);
                        break;
                    }

                default:
                    {
                        str = GetMyNumberToWords(Math.Truncate(value / 1000000000000M)) + " BILLONES";

                        if (Math.Truncate(value / 1000000000000M) % 100M == 1)
                            str = str.Replace("UNO", "UN");

                        if ((value - Math.Truncate(value / 1000000000000M) * 1000000000000M) > 0)
                            str = str + " " + GetMyNumberToWords(value - Math.Truncate(value / 1000000000000M) * 1000000000000M);
                        break;
                    }
            }

            return str;
        }


        /// <summary>
        ///		Envía un archivo por FTP
        /// </summary>
        public async Task<bool> SubirArchivoFTP(byte[] archivo, string dirArchivo, string nombreArchivo)
        {
            string ftpServerIP = AppSettings.ServidorFTP;
            string ftpUserName = AppSettings.UsuarioFPT;
            string ftpPassword = AppSettings.PasswordFTP;

            FtpWebRequest objFTPRequest;

            // Create FtpWebRequest object 
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + dirArchivo + "/" + nombreArchivo));

            // Set Credintials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            objFTPRequest.EnableSsl = false;
            objFTPRequest.KeepAlive = false;

            // Set the data transfer type.
            objFTPRequest.UseBinary = true;

            // Set request method
            objFTPRequest.Method = WebRequestMethods.Ftp.UploadFile;

            try
            {
                // Get Stream of the file
                Stream objStream = objFTPRequest.GetRequestStream();

                await objStream.WriteAsync(archivo, 0, archivo.Length);

                objStream.Close();

                return true;
            }
            catch
            {

                throw;

            }
        }

        /// <summary>
        ///		Envía un archivo por FTP
        /// </summary>
        public async Task<bool> BorrarArchivoFTP(string dirArchivo, string nombreArchivo)
        {
            string ftpServerIP = AppSettings.ServidorFTP;
            string ftpUserName = AppSettings.UsuarioFPT;
            string ftpPassword = AppSettings.PasswordFTP;

            FtpWebRequest objFTPRequest;

            // Create FtpWebRequest object 
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + dirArchivo + "/" + nombreArchivo));

            // Set Credintials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            objFTPRequest.KeepAlive = false;

            // Set request method
            objFTPRequest.Method = WebRequestMethods.Ftp.DeleteFile;

            try
            {
                var respuesta = (FtpWebResponse)await objFTPRequest.GetResponseAsync();

                if (respuesta.StatusCode == FtpStatusCode.FileActionOK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                throw;

            }
        }


        /// <summary>
        ///		Envía un archivo por FTP
        /// </summary>
        public async Task<bool> CrearCarpetaFTP(string dirArchivo, string nombreNuevaCarpeta)
        {
            string ftpServerIP = AppSettings.ServidorFTP;
            string ftpUserName = AppSettings.UsuarioFPT;
            string ftpPassword = AppSettings.PasswordFTP;

            FtpWebRequest objFTPRequest;

            string carpeta = "ftp://" + ftpServerIP + "/" + dirArchivo + "/" + nombreNuevaCarpeta;


            // Create FtpWebRequest object 
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(carpeta));

            // Set Credintials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            objFTPRequest.KeepAlive = false;

            // Set request method
            objFTPRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

            try
            {
                var respuesta = (FtpWebResponse)await objFTPRequest.GetResponseAsync();

                if (respuesta.StatusCode == FtpStatusCode.PathnameCreated)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {

                return false;

            }
        }

        public async Task<bool> ExisteDirectorioFTP(string directorio)
        {
            string ftpServerIP = AppSettings.ServidorFTP;
            string ftpUserName = AppSettings.UsuarioFPT;
            string ftpPassword = AppSettings.PasswordFTP;

            FtpWebRequest objFTPRequest;

            string carpeta = "ftp://" + ftpServerIP + "/" + directorio;


            // Create FtpWebRequest object 
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(carpeta));

            // Set Credintials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            objFTPRequest.KeepAlive = false;

            // Set request method
            objFTPRequest.Method = WebRequestMethods.Ftp.ListDirectory;

            try
            {
                var respuesta = (FtpWebResponse)await objFTPRequest.GetResponseAsync();

                if (respuesta.StatusCode == FtpStatusCode.DataAlreadyOpen)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {

                return false;

            }
        }
    }
}